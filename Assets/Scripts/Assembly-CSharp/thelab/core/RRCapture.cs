using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class RRCapture : MonoBehaviour
	{
		[SerializeField]
		private RenderTexture m_reflectionTexture;

		[SerializeField]
		private RenderTexture m_refractionTexture;

		public Material[] materials;

		public bool reflect = true;

		public bool refract = true;

		public int reflectionSize = 256;

		public int refractionSize = 256;

		public float clipPlaneOffset = 0.07f;

		public LayerMask reflectLayers = -1;

		public LayerMask refractLayers = -1;

		[SerializeField]
		private Camera m_reflectionCamera;

		[SerializeField]
		private Camera m_refractionCamera;

		public string reflectionProperty = "_ReflectionCapture";

		public string refractionProperty = "_RefractionCapture";

		private bool m_render_lock;

		private Dictionary<Camera, Skybox> m_cached_skybox = new Dictionary<Camera, Skybox>();

		private Transform m_transform;

		private Camera m_scene_camera;

		private static Texture2D m_default_tex;

		public bool supported => true;

		public RenderTexture reflectionTexture
		{
			get
			{
				reflectionSize = Mathf.Max(reflectionSize, 1);
				m_reflectionTexture = AssertRT(m_reflectionTexture, reflectionSize);
				return m_reflectionTexture;
			}
		}

		public RenderTexture refractionTexture
		{
			get
			{
				refractionSize = Mathf.Max(refractionSize, 1);
				m_refractionTexture = AssertRT(m_refractionTexture, refractionSize);
				return m_refractionTexture;
			}
		}

		public Camera reflectionCamera
		{
			get
			{
				if ((bool)m_reflectionCamera)
				{
					return m_reflectionCamera;
				}
				return m_reflectionCamera = AssertCamera("reflection");
			}
		}

		public Camera refractionCamera
		{
			get
			{
				if ((bool)m_refractionCamera)
				{
					return m_refractionCamera;
				}
				return m_refractionCamera = AssertCamera("refraction");
			}
		}

		public new Transform transform => m_transform;

		protected void Awake()
		{
			m_transform = base.transform;
			if (!supported)
			{
				Debug.LogWarning("RRCapture> Hardware don't offer support.");
			}
		}

		public void OnWillRenderObject()
		{
			if (!base.enabled || !supported)
			{
				return;
			}
			Camera current = Camera.current;
			if ((bool)current && !current.orthographic && !m_render_lock)
			{
				m_render_lock = true;
				Vector3 position = transform.position;
				Vector3 up = transform.up;
				UpdateCameraModes(current, reflectionCamera);
				UpdateCameraModes(current, refractionCamera);
				if (reflect)
				{
					float w = 0f - (Vector3.Dot(up, position) + clipPlaneOffset);
					Vector4 plane = new Vector4(up.x, up.y, up.z, w);
					Matrix4x4 reflectionMat = Matrix4x4.zero;
					CalculateReflectionMatrix(ref reflectionMat, plane);
					Vector3 position2 = current.transform.position;
					Vector3 position3 = reflectionMat.MultiplyPoint(position2);
					reflectionCamera.worldToCameraMatrix = current.worldToCameraMatrix * reflectionMat;
					Vector4 clipPlane = CameraSpacePlane(reflectionCamera, position, up, 1f);
					reflectionCamera.projectionMatrix = current.CalculateObliqueMatrix(clipPlane);
					int value = reflectLayers.value;
					value = -17 & value;
					value = -33 & value;
					reflectionCamera.cullingMask = value;
					reflectionCamera.useOcclusionCulling = false;
					reflectionCamera.targetTexture = reflectionTexture;
					GL.invertCulling = true;
					reflectionCamera.transform.position = position3;
					Vector3 eulerAngles = current.transform.eulerAngles;
					reflectionCamera.transform.eulerAngles = new Vector3(0f - eulerAngles.x, eulerAngles.y, eulerAngles.z);
					reflectionCamera.Render();
					reflectionCamera.transform.position = position2;
					GL.invertCulling = false;
				}
				if (refract)
				{
					int value2 = refractLayers.value;
					value2 = -17 & value2;
					value2 = -33 & value2;
					refractionCamera.worldToCameraMatrix = current.worldToCameraMatrix;
					Vector4 clipPlane2 = CameraSpacePlane(refractionCamera, position, up, -1f);
					refractionCamera.projectionMatrix = current.CalculateObliqueMatrix(clipPlane2);
					refractionCamera.cullingMask = value2;
					refractionCamera.useOcclusionCulling = false;
					refractionCamera.targetTexture = refractionTexture;
					refractionCamera.transform.position = current.transform.position;
					refractionCamera.transform.rotation = current.transform.rotation;
					refractionCamera.Render();
				}
				if (reflect || refract)
				{
					RefreshProperties();
				}
				if (reflect)
				{
					Shader.EnableKeyword("RR_REFLECTION_ENABLED");
				}
				else
				{
					Shader.DisableKeyword("RR_REFLECTION_ENABLED");
				}
				if (refract)
				{
					Shader.EnableKeyword("RR_REFRACTION_ENABLED");
				}
				else
				{
					Shader.DisableKeyword("RR_REFRACTION_ENABLED");
				}
				bool flag = (bool)m_scene_camera && current == m_scene_camera;
				if (!flag && !m_scene_camera)
				{
					flag = current.name == "SceneCamera";
				}
				if (flag)
				{
					m_scene_camera = current;
					Shader.EnableKeyword("RR_SCENE_VIEW_ON");
					Shader.DisableKeyword("RR_SCENE_VIEW_OFF");
				}
				else
				{
					Shader.EnableKeyword("RR_SCENE_VIEW_OFF");
					Shader.DisableKeyword("RR_SCENE_VIEW_ON");
				}
				Shader.SetGlobalVector("_RRCaptureSize", new Vector4(reflectionSize, reflectionSize, refractionSize, refractionSize));
				m_render_lock = false;
			}
		}

		public void Clear()
		{
			if ((bool)m_reflectionTexture)
			{
				Object.DestroyImmediate(m_reflectionTexture);
				m_reflectionTexture = null;
			}
			if ((bool)m_refractionTexture)
			{
				Object.DestroyImmediate(m_refractionTexture);
				m_refractionTexture = null;
			}
			if ((bool)m_reflectionCamera)
			{
				Object.DestroyImmediate(m_reflectionCamera.gameObject);
			}
			if ((bool)m_refractionCamera)
			{
				Object.DestroyImmediate(m_refractionCamera.gameObject);
			}
		}

		protected void OnDisable()
		{
			Clear();
			RefreshProperties();
		}

		protected void OnDestroy()
		{
			Clear();
		}

		protected Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
		{
			Vector3 point = pos + normal * clipPlaneOffset;
			Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
			Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
			Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
			return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
		}

		protected void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
		{
			reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
			reflectionMat.m01 = -2f * plane[0] * plane[1];
			reflectionMat.m02 = -2f * plane[0] * plane[2];
			reflectionMat.m03 = -2f * plane[3] * plane[0];
			reflectionMat.m10 = -2f * plane[1] * plane[0];
			reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
			reflectionMat.m12 = -2f * plane[1] * plane[2];
			reflectionMat.m13 = -2f * plane[3] * plane[1];
			reflectionMat.m20 = -2f * plane[2] * plane[0];
			reflectionMat.m21 = -2f * plane[2] * plane[1];
			reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
			reflectionMat.m23 = -2f * plane[3] * plane[2];
			reflectionMat.m30 = 0f;
			reflectionMat.m31 = 0f;
			reflectionMat.m32 = 0f;
			reflectionMat.m33 = 1f;
		}

		protected void UpdateCameraModes(Camera p_src, Camera p_dst)
		{
			if (p_dst == null)
			{
				return;
			}
			p_dst.clearFlags = p_src.clearFlags;
			p_dst.backgroundColor = p_src.backgroundColor;
			if (p_src.clearFlags == CameraClearFlags.Skybox)
			{
				Skybox skybox2;
				if (!m_cached_skybox.ContainsKey(p_src))
				{
					Skybox skybox = (m_cached_skybox[p_src] = p_src.GetComponent<Skybox>());
					skybox2 = skybox;
				}
				else
				{
					skybox2 = m_cached_skybox[p_src];
				}
				Skybox skybox3 = skybox2;
				Skybox skybox4;
				if (!m_cached_skybox.ContainsKey(p_dst))
				{
					Skybox skybox = (m_cached_skybox[p_dst] = p_dst.GetComponent<Skybox>());
					skybox4 = skybox;
				}
				else
				{
					skybox4 = m_cached_skybox[p_dst];
				}
				Skybox skybox5 = skybox4;
				if (!skybox3 || !skybox3.material)
				{
					skybox5.enabled = false;
				}
				else
				{
					skybox5.enabled = true;
					skybox5.material = skybox3.material;
				}
			}
			p_dst.farClipPlane = p_src.farClipPlane;
			p_dst.nearClipPlane = p_src.nearClipPlane;
			p_dst.orthographic = p_src.orthographic;
			p_dst.fieldOfView = p_src.fieldOfView;
			p_dst.aspect = p_src.aspect;
			p_dst.orthographicSize = p_src.orthographicSize;
		}

		protected Camera AssertCamera(string p_prefix)
		{
			string text = "capture." + p_prefix + "-" + ((uint)GetInstanceID()).ToString("x6");
			GameObject gameObject = GameObject.Find(text);
			Camera camera;
			if (!gameObject)
			{
				gameObject = new GameObject(text);
				camera = gameObject.AddComponent<Camera>();
				gameObject.AddComponent<Skybox>();
				gameObject.AddComponent<FlareLayer>();
				gameObject.hideFlags = HideFlags.HideAndDontSave;
			}
			else
			{
				camera = gameObject.GetComponent<Camera>();
			}
			camera.enabled = false;
			camera.transform.position = transform.position;
			camera.transform.rotation = transform.rotation;
			return camera;
		}

		protected void RefreshProperties()
		{
			if (materials == null || materials.Length == 0)
			{
				return;
			}
			if (!m_default_tex)
			{
				m_default_tex = new Texture2D(1, 1);
				m_default_tex.SetPixel(0, 0, Color.black);
				m_default_tex.name = "rr-pixel-black";
				m_default_tex.Apply();
			}
			for (int i = 0; i < materials.Length; i++)
			{
				Material material = materials[i];
				if (!material)
				{
					continue;
				}
				if (reflect)
				{
					if (string.IsNullOrEmpty(reflectionProperty))
					{
						continue;
					}
					string text = reflectionProperty;
					Texture texture = (m_reflectionTexture ? ((Texture)m_reflectionTexture) : ((Texture)m_default_tex));
					if (material.HasProperty(text) && material.GetTexture(text) != texture)
					{
						material.SetTexture(text, texture);
					}
				}
				if (refract && !string.IsNullOrEmpty(refractionProperty))
				{
					string text = refractionProperty;
					Texture texture = (m_refractionTexture ? ((Texture)m_refractionTexture) : ((Texture)m_default_tex));
					if (material.HasProperty(text) && material.GetTexture(text) != texture)
					{
						material.SetTexture(text, texture);
					}
				}
			}
		}

		private RenderTexture AssertRT(RenderTexture p_tex, int p_size)
		{
			bool flag = false;
			if (!p_tex)
			{
				flag = true;
			}
			else if (p_tex.width != p_size)
			{
				flag = true;
			}
			if (flag)
			{
				if ((bool)p_tex)
				{
					Object.DestroyImmediate(p_tex);
				}
				int num = Mathf.Max(1, p_size);
				p_tex = new RenderTexture(num, num, 16);
				p_tex.isPowerOfTwo = true;
				p_tex.hideFlags = HideFlags.HideAndDontSave;
				p_tex.name = "rr-capture-rt-" + num;
			}
			return p_tex;
		}
	}
}
