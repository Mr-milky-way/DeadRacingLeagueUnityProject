using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace drl.debug
{
	public class ThreadingDebug : MonoBehaviour
	{
		public enum Mode
		{
			Unity = 0,
			Thread = 1
		}

		public Mode mode;

		public RawImage image;

		public Texture2D texture;

		public int size = 128;

		public Color[] pixels;

		public float[] timers;

		public float dt;

		public bool can_apply;

		public bool resize;

		private Thread thread_loop;

		protected void Start()
		{
			Create(size);
		}

		protected void CreateThread()
		{
			thread_loop = new Thread((ThreadStart)delegate
			{
				DateTime now = DateTime.Now;
				DateTime dateTime = now;
				while (true)
				{
					if (mode == Mode.Thread)
					{
						now = DateTime.Now;
						TimeSpan timeSpan = dateTime - now;
						dateTime = now;
						dt = (float)timeSpan.TotalMilliseconds * 10f * 0.001f;
						Refresh();
					}
					Thread.Sleep(1);
				}
			});
			thread_loop.Start();
		}

		protected void Create(int p_size)
		{
			if (thread_loop != null)
			{
				thread_loop.Abort();
			}
			if ((bool)texture)
			{
				UnityEngine.Object.Destroy(texture);
			}
			texture = new Texture2D(p_size, p_size, TextureFormat.RGBA32, mipChain: false);
			texture.filterMode = FilterMode.Point;
			pixels = new Color[p_size * p_size];
			image.texture = texture;
			timers = new float[pixels.Length];
			for (int i = 0; i < timers.Length; i++)
			{
				timers[i] = UnityEngine.Random.value * 45f;
			}
			CreateThread();
		}

		protected void OnGUI()
		{
			GUIStyle gUIStyle = new GUIStyle();
			gUIStyle.fontSize = 24;
			gUIStyle.normal.textColor = Color.red;
			GUI.Label(new Rect(15f, 15f, 200f, 46f), mode.ToString() + " / " + size + "px", gUIStyle);
		}

		public void Update()
		{
			if (mode == Mode.Unity)
			{
				dt = Time.unscaledDeltaTime;
				Refresh();
			}
			Apply();
			if (Input.GetKeyDown(KeyCode.Alpha0))
			{
				for (int i = 0; i < timers.Length; i++)
				{
					timers[i] = 0f;
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				can_apply = false;
				mode = Mode.Unity;
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				can_apply = false;
				mode = Mode.Thread;
			}
			if (!resize && Input.GetKeyDown(KeyCode.Alpha3))
			{
				resize = true;
				size = 256;
			}
			if (!resize && Input.GetKeyDown(KeyCode.Alpha4))
			{
				resize = true;
				size = 512;
			}
			if (!resize && Input.GetKeyDown(KeyCode.Alpha5))
			{
				resize = true;
				size = 1024;
			}
			if (!resize && Input.GetKeyDown(KeyCode.Alpha6))
			{
				resize = true;
				size = 2048;
			}
		}

		public void Apply()
		{
			if (!resize && can_apply)
			{
				texture.SetPixels(pixels);
				texture.Apply();
				can_apply = false;
			}
			if (resize)
			{
				Create(size);
				resize = false;
			}
		}

		public void Refresh()
		{
			if (!resize && !can_apply)
			{
				for (int i = 0; i < pixels.Length; i++)
				{
					float num = i % size;
					float num2 = i / size;
					float num3 = num / (float)size;
					float num4 = num2 / (float)size;
					float num5 = timers[i] * 0.5f;
					float num6 = Mathf.Sin(360f * new Vector2(num3 + num5, num4 + num5).magnitude * ((float)Math.PI / 180f));
					num6 = (num6 + 1f) * 0.5f;
					pixels[i].r = (pixels[i].g = (pixels[i].b = Mathf.Lerp(0f, 1f, num6)));
					pixels[i].a = 1f;
					timers[i] += dt;
				}
				can_apply = true;
			}
		}
	}
}
