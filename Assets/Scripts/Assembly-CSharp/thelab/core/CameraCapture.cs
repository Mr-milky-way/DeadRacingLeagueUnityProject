using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.Rendering;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class CameraCapture : MonoBehaviour
	{
		[SerializeField]
		private Camera m_camera;

		public RenderingPath path = RenderingPath.DeferredShading;

		public int width = 256;

		public int height = 256;

		public Texture2D alpha;

		public Texture2D result;

		public bool smooth;

		public bool async;

		public bool mipmap;

		public bool captureAlpha;

		public RenderTexture lastCaptureRT;

		public Texture2D lastCaptureTex;

		public Texture2D lastCaptureAlpha;

		public Camera camera
		{
			get
			{
				if (!m_camera)
				{
					return m_camera = GetComponent<Camera>();
				}
				return m_camera;
			}
			set
			{
				m_camera = value;
			}
		}

		public void Capture(Action<Texture2D> p_callback, bool p_defer = false)
		{
			Camera c = camera;
			if (!c)
			{
				return;
			}
			string text = base.gameObject.GetInstanceID().ToString("x6");
			int num = width;
			int num2 = height;
			int stw = num;
			int sth = num2;
			if (smooth)
			{
				stw *= 2;
				sth *= 2;
			}
			RenderTexture active_rt = RenderTexture.active;
			RenderTexture camera_rt = c.targetTexture;
			Color backgroundColor = c.backgroundColor;
			RenderTextureFormat renderTextureFormat = (c.allowHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
			TextureFormat textureFormat = (c.allowHDR ? TextureFormat.RGBAHalf : TextureFormat.ARGB32);
			renderTextureFormat = RenderTextureFormat.ARGB32;
			textureFormat = TextureFormat.ARGB32;
			switch (OS.context)
			{
			case "xbs":
			case "xb":
				renderTextureFormat = RenderTextureFormat.ARGBHalf;
				textureFormat = TextureFormat.RGBAHalf;
				break;
			}
			bool flag = smooth || mipmap;
			camera_rt = c.targetTexture;
			RenderTexture rt = RenderTexture.GetTemporary(stw, sth, 24, renderTextureFormat);
			rt.Release();
			rt.useMipMap = flag;
			rt.Create();
			PostProcessingBehaviour component = c.GetComponent<PostProcessingBehaviour>();
			bool flag2 = (bool)component && component.enabled;
			if ((bool)component)
			{
				component.enabled = false;
			}
			Color backgroundColor2 = c.backgroundColor;
			c.targetTexture = rt;
			c.backgroundColor = new Color(backgroundColor2.r, backgroundColor2.g, backgroundColor2.b, (path == RenderingPath.Forward) ? backgroundColor2.a : 0f);
			c.renderingPath = RenderingPath.Forward;
			c.Render();
			alpha = new Texture2D(stw, sth, textureFormat, flag);
			alpha.hideFlags = HideFlags.HideAndDontSave;
			alpha.name = "capture-" + text + "-alpha";
			RenderTexture.active = c.targetTexture;
			alpha.ReadPixels(new Rect(0f, 0f, stw, sth), 0, 0);
			RenderTexture.active = active_rt;
			alpha.Apply(updateMipmaps: true, makeNoLongerReadable: false);
			alpha = TextureToSmooth(alpha);
			if ((bool)component)
			{
				component.enabled = flag2;
			}
			c.targetTexture = null;
			if ((bool)rt)
			{
				RenderTexture.ReleaseTemporary(rt);
			}
			rt = RenderTexture.GetTemporary(stw, sth, 24, renderTextureFormat);
			rt.Release();
			rt.useMipMap = flag;
			rt.Create();
			c.targetTexture = rt;
			c.backgroundColor = backgroundColor;
			c.renderingPath = path;
			c.Render();
			result = new Texture2D(stw, sth, textureFormat, flag);
			result.hideFlags = HideFlags.HideAndDontSave;
			result.name = "capture-" + text + "-result";
			Action on_complete = delegate
			{
				result = TextureToSmooth(result);
				if (captureAlpha)
				{
					TransferAlpha();
				}
				if (p_callback != null)
				{
					p_callback(result);
				}
			};
			Action finish_result = delegate
			{
				if (async)
				{
					AsyncGPUReadback.Request(rt, 0, result.format, delegate(AsyncGPUReadbackRequest p_request)
					{
						NativeArray<byte> data = p_request.GetData<byte>();
						NativeArray<byte> rawTextureData = result.GetRawTextureData<byte>();
						NativeArray<byte>.Copy(data, rawTextureData, data.Length);
						result.Apply(updateMipmaps: true, makeNoLongerReadable: false);
						if ((bool)rt)
						{
							RenderTexture.ReleaseTemporary(rt);
						}
						on_complete();
					});
				}
				else
				{
					RenderTexture.active = c.targetTexture;
					result.ReadPixels(new Rect(0f, 0f, stw, sth), 0, 0);
					RenderTexture.active = active_rt;
					result.Apply(updateMipmaps: true, makeNoLongerReadable: false);
					c.targetTexture = camera_rt;
					if ((bool)rt)
					{
						RenderTexture.ReleaseTemporary(rt);
					}
					on_complete();
				}
			};
			if (p_defer)
			{
				int fc = 0;
				Activity.Run((Func<bool>)delegate
				{
					if (fc < 3)
					{
						fc++;
						return true;
					}
					finish_result();
					return false;
				}, 0f, false);
			}
			else
			{
				finish_result();
			}
		}

		public void Capture()
		{
			Capture(null);
		}

		protected Texture2D TextureToSmooth(Texture2D p_original)
		{
			if (!p_original)
			{
				return null;
			}
			int num = p_original.width;
			int num2 = p_original.height;
			int num3 = Mathf.Max(1, smooth ? (num / 2) : num);
			int num4 = Mathf.Max(1, smooth ? (num2 / 2) : num2);
			Texture2D texture2D = new Texture2D(num3, num4, p_original.format, mipmap);
			texture2D.hideFlags = p_original.hideFlags;
			int miplevel = Mathf.Min(p_original.mipmapCount, smooth ? 1 : 0);
			Color[] pixels = p_original.GetPixels(miplevel);
			texture2D.SetPixels(pixels);
			texture2D.name = p_original.name;
			texture2D.Apply(mipmap, makeNoLongerReadable: false);
			UnityEngine.Object.Destroy(p_original);
			return texture2D;
		}

		protected void TransferAlpha()
		{
			if ((bool)alpha)
			{
				Color[] pixels = result.GetPixels(0);
				Color[] pixels2 = alpha.GetPixels(0);
				int num = Mathf.Min(pixels.Length, pixels2.Length);
				for (int i = 0; i < num; i++)
				{
					pixels[i].a = pixels2[i].a;
				}
				result.SetPixels(pixels);
				result.Apply(mipmap, makeNoLongerReadable: false);
			}
		}

		protected void AsyncCallback(Action p_callback)
		{
			if (Application.isPlaying)
			{
				Activity.RunOnce(p_callback);
			}
		}
	}
}
