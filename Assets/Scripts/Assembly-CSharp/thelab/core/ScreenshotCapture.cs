using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[RequireComponent(typeof(Camera))]
	[RequireComponent(typeof(OrbitTransform))]
	[ExecuteInEditMode]
	public class ScreenshotCapture : MonoBehaviour
	{
		internal static bool debug = false;

		internal static bool debug_scale = false;

		private static Vector3[] m_bound_corners = new Vector3[8];

		private Camera m_camera;

		private CameraCapture m_capture;

		private OrbitTransform m_orbit;

		public Vector3 angle;

		public Vector3 origin;

		public Texture2D alpha;

		private float m_fov;

		private Vector3 m_angle;

		private Transform m_center;

		private Transform m_tparent;

		private Vector3 m_tposition;

		private Vector3 m_trotation;

		private Vector3 m_tscale;

		private int m_tsibling;

		private List<Renderer> m_rndl;

		private Activity m_capture_call;

		public Camera camera
		{
			get
			{
				if (!m_camera)
				{
					return m_camera = GetComponentInChildren<Camera>();
				}
				return m_camera;
			}
		}

		public CameraCapture capture
		{
			get
			{
				if (!m_capture)
				{
					return m_capture = GetComponentInChildren<CameraCapture>();
				}
				return m_capture;
			}
		}

		public Bounds cameraBounds
		{
			get
			{
				Bounds bounds = default(Bounds);
				Vector3[] array = new Vector3[8];
				camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, array);
				for (int i = 0; i < array.Length; i++)
				{
					bounds.Encapsulate(array[i]);
				}
				return bounds;
			}
		}

		public OrbitTransform orbit
		{
			get
			{
				if (!m_orbit)
				{
					return m_orbit = GetComponent<OrbitTransform>();
				}
				return m_orbit;
			}
		}

		public Texture2D result
		{
			get
			{
				if (!capture)
				{
					return Texture2D.blackTexture;
				}
				return capture.result;
			}
		}

		public static ScreenshotCapture Instantiate(string p_resource = "")
		{
			ScreenshotCapture screenshotCapture = null;
			GameObject original;
			if (!string.IsNullOrEmpty(p_resource))
			{
				original = Resources.Load<GameObject>(p_resource);
				original = UnityEngine.Object.Instantiate(original);
				screenshotCapture = original.GetComponent<ScreenshotCapture>();
				if (!screenshotCapture)
				{
					UnityEngine.Object.Destroy(original);
				}
			}
			else
			{
				original = new GameObject();
				screenshotCapture = original.AddComponent<ScreenshotCapture>();
			}
			if ((bool)original)
			{
				original.SetActive(value: false);
				original.name = "screenshot-capture-" + original.GetHashCode().ToString("x6");
				original.transform.SetAsFirstSibling();
			}
			return screenshotCapture;
		}

		protected static Bounds GetUnifiedBounds(List<Renderer> p_targets)
		{
			if (p_targets.Count <= 0)
			{
				return default(Bounds);
			}
			Bounds bounds = p_targets[0].bounds;
			for (int i = 1; i < p_targets.Count; i++)
			{
				if (p_targets[i].enabled)
				{
					bounds.Encapsulate(p_targets[i].bounds);
				}
			}
			return bounds;
		}

		public static Vector4 ViewportFit(List<Renderer> p_targets, Camera p_camera, float p_scale = 1f)
		{
			Vector4 vector = new Vector4(0f, 0f, 0f, 1f);
			if (p_targets.Count <= 0)
			{
				return vector;
			}
			Camera camera = (p_camera ? p_camera : Camera.current);
			Bounds unifiedBounds = GetUnifiedBounds(p_targets);
			Vector3 center = unifiedBounds.center;
			Vector3 extents = unifiedBounds.extents;
			int num = 0;
			m_bound_corners[num++] = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
			m_bound_corners[num++] = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
			m_bound_corners[num++] = new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z);
			m_bound_corners[num++] = new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z);
			m_bound_corners[num++] = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
			m_bound_corners[num++] = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);
			m_bound_corners[num++] = new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z);
			m_bound_corners[num++] = new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z);
			Vector2 vector2 = Vector2.zero;
			Vector2 vector3 = Vector2.zero;
			for (int i = 0; i < m_bound_corners.Length; i++)
			{
				Vector3 vector4 = m_bound_corners[i];
				if (debug)
				{
					Debug.DrawLine(center, vector4, Color.yellow, 160f, depthTest: false);
				}
				Vector2 vector5 = camera.WorldToViewportPoint(vector4);
				if (i <= 0)
				{
					vector2 = (vector3 = vector5);
					continue;
				}
				vector2 = Vector2.Min(vector5, vector2);
				vector3 = Vector2.Max(vector5, vector3);
			}
			if (debug)
			{
				float nearClipPlane = p_camera.nearClipPlane;
				Vector3[] array = new Vector3[8]
				{
					new Vector3(vector2.x, vector2.y, nearClipPlane),
					new Vector3(vector3.x, vector2.y, nearClipPlane),
					new Vector3(vector3.x, vector3.y, nearClipPlane),
					new Vector3(vector2.x, vector3.y, nearClipPlane),
					new Vector3(0f, 0f, nearClipPlane),
					new Vector3(1f, 0f, nearClipPlane),
					new Vector3(1f, 1f, nearClipPlane),
					new Vector3(0f, 1f, nearClipPlane)
				};
				int[] array2 = new int[24]
				{
					0, 1, 1, 2, 2, 3, 3, 0, 4, 5,
					5, 6, 6, 7, 7, 4, 0, 4, 1, 5,
					2, 6, 3, 7
				};
				for (int j = 0; j < array2.Length; j += 2)
				{
					Debug.DrawLine(m_bound_corners[array2[j]], m_bound_corners[array2[j + 1]], Color.magenta, 160f, depthTest: false);
				}
				array2 = new int[16]
				{
					0, 1, 1, 2, 2, 3, 3, 0, 4, 5,
					5, 6, 6, 7, 7, 4
				};
				for (int k = 0; k < array2.Length; k += 2)
				{
					Vector3 position = array[array2[k]];
					Vector3 position2 = array[array2[k + 1]];
					position = camera.ViewportToWorldPoint(position);
					position2 = camera.ViewportToWorldPoint(position2);
					Debug.DrawLine(position, position2, (k > 7) ? Color.red : Color.green, 160f, depthTest: false);
				}
			}
			Vector2 vector6 = vector3 - vector2;
			float num2 = Mathf.Max(Mathf.Abs(vector6.x), Mathf.Abs(vector6.y));
			num2 = ((num2 <= 0f) ? p_scale : (p_scale / num2));
			vector = center;
			vector.w = num2;
			return vector;
		}

		private void Awake()
		{
			orbit.hideFlags = HideFlags.HideInInspector;
		}

		public Texture2D Capture(int p_width, int p_height, Transform p_target, ScreenshotData p_data, bool p_smooth = false, bool p_mipmap = true)
		{
			return (Texture2D)Capture(p_width, p_height, p_target, p_data, p_smooth, p_preview: false, p_mipmap);
		}

		public Texture Capture(int p_width, int p_height, Transform p_target, ScreenshotData p_data, bool p_smooth, bool p_preview, bool p_mipmap)
		{
			Texture blackTexture = Texture2D.blackTexture;
			if (!p_target)
			{
				return blackTexture;
			}
			OnPreCapture(p_target, p_data);
			OnCapture(p_target, p_data);
			if ((bool)capture)
			{
				capture.async = false;
				capture.width = p_width;
				capture.height = p_height;
				capture.smooth = p_smooth;
				capture.mipmap = p_mipmap;
				capture.captureAlpha = true;
				capture.Capture();
			}
			OnPostCapture(p_target, p_data);
			return result;
		}

		public void CaptureAsync(int p_width, int p_height, Transform p_target, ScreenshotData p_data, bool p_smooth, bool p_preview, bool p_mipmap, Action<Texture> p_callback)
		{
			Texture blackTexture = Texture2D.blackTexture;
			if (!p_target)
			{
				if (p_callback != null)
				{
					p_callback(blackTexture);
				}
				return;
			}
			OnPreCapture(p_target, p_data);
			OnCapture(p_target, p_data);
			OnPreCapture(p_target, p_data);
			OnCapture(p_target, p_data);
			if (m_capture_call != null)
			{
				m_capture_call.Stop();
			}
			m_capture_call = Activity.RunOnce(delegate
			{
				if ((bool)capture)
				{
					capture.async = true;
					capture.width = p_width;
					capture.height = p_height;
					capture.smooth = p_smooth;
					capture.mipmap = p_mipmap;
					capture.captureAlpha = true;
					capture.Capture(delegate(Texture2D p_texture)
					{
						if (p_callback != null)
						{
							p_callback(p_texture);
						}
						OnPostCapture(p_target, p_data);
					});
				}
			});
			m_capture_call.late = true;
		}

		protected void SetupCameraClipPlanes(Transform p_target)
		{
			if ((bool)p_target)
			{
				camera.depth = (debug ? 100 : (-100));
				switch (LayerMask.LayerToName(p_target.gameObject.layer))
				{
				case "DronePart:":
				case "DroneAsset":
					camera.nearClipPlane = 0.05f;
					camera.farClipPlane = 2f;
					break;
				case "MapAsset":
					camera.nearClipPlane = 0.05f;
					camera.farClipPlane = 20f;
					break;
				}
				orbit.hideFlags = HideFlags.HideInInspector;
			}
		}

		protected virtual void OnPreCapture(Transform p_target, ScreenshotData p_data)
		{
			if ((bool)camera)
			{
				SetupCameraClipPlanes(p_target);
				m_fov = camera.fieldOfView;
				m_angle = orbit.angle;
				m_angle.z = camera.transform.localEulerAngles.z;
				float num = (p_data ? p_data.fov : 0f);
				camera.fieldOfView = m_fov + num;
				m_center = new GameObject(base.name + "@center").transform;
				m_tparent = p_target.parent;
				m_tposition = p_target.position;
				m_trotation = p_target.localEulerAngles;
				m_tscale = p_target.localScale;
				m_tsibling = p_target.GetSiblingIndex();
				m_rndl = Hierarchy.FindAll<Renderer>(p_target);
			}
		}

		protected virtual void OnCapture(Transform p_target, ScreenshotData p_data)
		{
			if ((bool)camera)
			{
				Transform center = m_center;
				Vector3 vector = (p_data ? p_data.angle : Vector3.zero);
				Vector3 vector2 = (p_data ? p_data.offset : Vector3.zero);
				float num = (p_data ? p_data.scale : 1f);
				float num2 = (p_data ? p_data.fitScale : 1f);
				center.position = origin;
				orbit.anchor = center.position;
				orbit.distance = 1f;
				orbit.angle = angle + vector;
				camera.transform.localEulerAngles = new Vector3(0f, 0f, angle.z + vector.z);
				p_target.position = center.position;
				p_target.localScale = Vector3.one * num2;
				p_target.localEulerAngles = Vector3.zero;
				Vector4 vector3 = ViewportFit(m_rndl, camera);
				Vector3 vector4 = vector3;
				float num3 = vector3.w;
				if (debug)
				{
					Debug.DrawLine(center.position, vector4, Color.red, 160f, depthTest: true);
				}
				if (p_data.autoFit)
				{
					orbit.anchor = vector4;
					center.position = vector4;
				}
				else
				{
					num3 = 1f;
				}
				p_target.SetParent(null);
				p_target.localScale = Vector3.one;
				p_target.SetParent(center, worldPositionStays: true);
				Vector3 localScale = Vector3.one * num3 * num;
				if (debug && !debug_scale)
				{
					localScale = Vector3.one;
				}
				center.localScale = localScale;
				orbit.anchor += (0f - vector2.x) * orbit.transform.right;
				orbit.anchor += (0f - vector2.y) * orbit.transform.up;
				orbit.anchor += (0f - vector2.z) * orbit.transform.forward;
			}
		}

		protected virtual void OnPostCapture(Transform p_target, ScreenshotData p_data)
		{
			if (!camera)
			{
				return;
			}
			camera.fieldOfView = m_fov;
			orbit.angle = m_angle;
			Vector3 localEulerAngles = camera.transform.localEulerAngles;
			localEulerAngles.z = m_angle.z;
			camera.transform.localEulerAngles = localEulerAngles;
			if (!debug)
			{
				p_target.parent = m_tparent;
				p_target.position = m_tposition;
				p_target.localEulerAngles = m_trotation;
				p_target.localScale = m_tscale;
				p_target.SetSiblingIndex(m_tsibling);
				if ((bool)m_center)
				{
					UnityEngine.Object.Destroy(m_center.gameObject);
				}
			}
		}
	}
}
