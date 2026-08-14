using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace thelab.core
{
	public class Hierarchy : MonoBehaviour
	{
		private Dictionary<string, Component> _ccache;

		private static Dictionary<string, Component> _gocache;

		protected Dictionary<string, Component> m_ccache
		{
			get
			{
				if (_ccache != null)
				{
					return _ccache;
				}
				return _ccache = new Dictionary<string, Component>();
			}
		}

		protected static Dictionary<string, Component> m_gocache
		{
			get
			{
				if (_gocache != null)
				{
					return _gocache;
				}
				return _gocache = new Dictionary<string, Component>();
			}
		}

		public static implicit operator Hierarchy(Transform b)
		{
			if (!b)
			{
				return null;
			}
			return b.GetComponent<Hierarchy>();
		}

		public static implicit operator Transform(Hierarchy b)
		{
			if (!b)
			{
				return null;
			}
			return b.transform;
		}

		static Hierarchy()
		{
		}

		public static void RefreshLayout(Component p_target, Transform p_container, bool p_disable_csf = false)
		{
			if ((bool)p_target)
			{
				ContentSizeFitter component = p_target.GetComponent<ContentSizeFitter>();
				if ((bool)component)
				{
					component.enabled = true;
					component.SetLayoutHorizontal();
					component.SetLayoutVertical();
				}
				if ((bool)p_container)
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)p_container);
				}
				if (p_disable_csf && (bool)component)
				{
					component.enabled = false;
				}
			}
		}

		public static void RefreshLayout(Component p_target, bool p_disable_csf = false)
		{
			RefreshLayout(p_target, null, p_disable_csf);
		}

		public static float GetConstantSize(Vector3 position, float p_size, float p_default = 5f)
		{
			Camera current = Camera.current;
			position = Gizmos.matrix.MultiplyPoint(position);
			if ((bool)current)
			{
				Transform transform = current.transform;
				Vector3 position2 = transform.position;
				float z = Vector3.Dot(position - position2, transform.TransformDirection(new Vector3(0f, 0f, 1f)));
				Vector3 vector = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(0f, 0f, z)));
				Vector3 vector2 = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(1f, 0f, z)));
				float magnitude = (vector - vector2).magnitude;
				return p_size / Mathf.Max(magnitude, 0.0001f);
			}
			return p_default;
		}

		public static int GetDepth(Transform p_target)
		{
			int num = 0;
			Transform transform = (p_target ? p_target.parent : null);
			while ((bool)transform)
			{
				num++;
				transform = transform.parent;
			}
			return num;
		}

		public static int GetGlobalSiblingIndex(Transform p_target)
		{
			Transform t = (p_target ? p_target : null);
			if (!t)
			{
				return -1;
			}
			Scene activeScene = SceneManager.GetActiveScene();
			if (!activeScene.IsValid())
			{
				return -1;
			}
			GameObject[] rootGameObjects = activeScene.GetRootGameObjects();
			int k = 0;
			bool has_found = false;
			Action<Transform> p_callback = delegate(Transform it)
			{
				if (!has_found)
				{
					if (it != t)
					{
						k++;
					}
					else
					{
						has_found = true;
					}
				}
			};
			for (int num = 0; num < rootGameObjects.Length; num++)
			{
				if (has_found)
				{
					break;
				}
				Transform p_target2 = rootGameObjects[num].transform;
				k = 0;
				Traverse(p_target2, p_callback);
			}
			if (!has_found)
			{
				return -1;
			}
			return k;
		}

		public static Vector2 WorldToAnchorPosition(RectTransform p_container, Camera p_camera, Canvas p_canvas, Vector3 p_position, RectOffset p_margin, out bool p_inbounds)
		{
			RectTransform rectTransform = (p_container ? ((RectTransform)p_container.parent) : null);
			Camera camera = (p_camera ? p_camera : Camera.main);
			p_inbounds = false;
			if (!camera)
			{
				if (!p_container)
				{
					return Vector2.zero;
				}
				return p_container.anchoredPosition;
			}
			float num = ((p_margin == null) ? 0f : ((float)p_margin.left));
			float num2 = ((p_margin == null) ? 0f : ((float)p_margin.right));
			float num3 = ((p_margin == null) ? 0f : ((float)p_margin.top));
			float num4 = ((p_margin == null) ? 0f : ((float)p_margin.bottom));
			Vector3 position = camera.transform.position;
			float num5 = Vector3.Dot(p_position - position, camera.transform.forward);
			Vector2 vector = camera.WorldToViewportPoint(p_position);
			if (num5 <= 0f)
			{
				Vector2 vector2 = vector - new Vector2(0.5f, 0.5f);
				vector2.Normalize();
				vector += vector2 * 2f;
				vector.x = 1f - vector.x;
				vector.y = 1f - vector.y;
			}
			Vector2 scale = new Vector2(Screen.width, Screen.height);
			Vector2 screenPoint = vector;
			screenPoint.Scale(scale);
			p_inbounds = true;
			if (screenPoint.x <= 0f)
			{
				p_inbounds = false;
			}
			else if (screenPoint.x >= scale.x)
			{
				p_inbounds = false;
			}
			else if (screenPoint.y <= 0f)
			{
				p_inbounds = false;
			}
			else if (screenPoint.y >= scale.y)
			{
				p_inbounds = false;
			}
			if ((bool)p_canvas)
			{
				camera = ((p_canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : p_canvas.worldCamera);
			}
			Vector2 localPoint = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, camera, out localPoint);
			Rect rect = RectTransformUtility.PixelAdjustRect(rectTransform, p_canvas);
			float min = num;
			float max = rect.width - num2;
			float max2 = 0f - num3;
			float min2 = 0f - (rect.height - num4);
			localPoint.x = Mathf.Clamp(localPoint.x, min, max);
			localPoint.y = Mathf.Clamp(localPoint.y, min2, max2);
			return localPoint;
		}

		public static Vector2 WorldToAnchorPosition(RectTransform p_container, Camera p_camera, Canvas p_canvas, Vector3 p_position, RectOffset p_margin)
		{
			bool p_inbounds = false;
			return WorldToAnchorPosition(p_container, p_camera, p_canvas, p_position, p_margin, out p_inbounds);
		}

		public static Vector2 WorldToAnchorPosition(RectTransform p_container, Camera p_camera, Canvas p_canvas, Vector3 p_position)
		{
			return WorldToAnchorPosition(p_container, p_camera, p_canvas, p_position, null);
		}

		public static Vector2 WorldToAnchorPosition(RectTransform p_container, Canvas p_canvas, Vector3 p_position, RectOffset p_margin)
		{
			return WorldToAnchorPosition(p_container, null, p_canvas, p_position, p_margin);
		}

		public static Vector2 WorldToAnchorPosition(RectTransform p_container, Canvas p_canvas, Vector3 p_position)
		{
			return WorldToAnchorPosition(p_container, null, p_canvas, p_position, null);
		}

		protected static void BaseTraverseReverse<T>(Transform p_target, Delegate p_callback) where T : Component
		{
			Transform transform = (p_target ? p_target.parent : null);
			while ((bool)transform)
			{
				T val = (transform ? transform.GetComponent<T>() : null);
				if ((bool)val && (object)p_callback != null)
				{
					object obj = p_callback.DynamicInvoke(val);
					if (obj is bool && !(bool)obj)
					{
						break;
					}
				}
				transform = transform.parent;
			}
		}

		protected static void BaseTraverse<T>(Transform p_target, Delegate p_callback, bool p_dfs = true, bool p_cached = false) where T : Component
		{
			if (!p_target)
			{
				return;
			}
			if (p_dfs)
			{
				T val = ((!p_cached) ? (p_target ? p_target.GetComponent<T>() : null) : (p_target ? GetComponent<T>(p_target.gameObject) : null));
				if ((bool)val && (object)p_callback != null)
				{
					object obj = p_callback.DynamicInvoke(val);
					if (obj is bool && !(bool)obj)
					{
						return;
					}
				}
				for (int i = 0; i < p_target.childCount; i++)
				{
					BaseTraverse<T>(p_target.GetChild(i), p_callback, p_dfs, p_cached);
				}
				return;
			}
			List<Transform> list = new List<Transform>();
			int j = 0;
			list.Add(p_target);
			for (; j < list.Count; j++)
			{
				Transform transform = list[j];
				bool flag = true;
				T val = ((!p_cached) ? (transform ? transform.GetComponent<T>() : null) : (transform ? GetComponent<T>(transform.gameObject) : null));
				if ((bool)val && (object)p_callback != null)
				{
					object obj2 = p_callback.DynamicInvoke(val);
					if (obj2 is bool && !(bool)obj2)
					{
						flag = false;
					}
				}
				if (flag && (bool)transform)
				{
					for (int k = 0; k < transform.childCount; k++)
					{
						list.Add(transform.GetChild(k));
					}
				}
			}
		}

		public static void TraverseReverse<T>(Transform p_target, Action<T> p_callback) where T : Component
		{
			BaseTraverseReverse<T>(p_target, p_callback);
		}

		public static void TraverseReverse<T>(Transform p_target, Predicate<T> p_callback) where T : Component
		{
			BaseTraverseReverse<T>(p_target, p_callback);
		}

		public static void Traverse<T>(Transform p_target, Action<T> p_callback, bool p_dfs = true, bool p_cached = false) where T : Component
		{
			BaseTraverse<T>(p_target, p_callback, p_dfs, p_cached);
		}

		public static void Traverse<T>(Transform p_target, Predicate<T> p_callback, bool p_dfs = true, bool p_cached = false) where T : Component
		{
			BaseTraverse<T>(p_target, p_callback, p_dfs, p_cached);
		}

		public static int TraverseDestroy(Transform p_target, float p_delay = 1f / 60f)
		{
			if (!p_target)
			{
				return 0;
			}
			p_target.gameObject.SetActive(value: false);
			float p_count = 0f;
			BaseTraverseDestroy(p_target, p_delay, ref p_count);
			return (int)p_count;
		}

		protected static void BaseTraverseDestroy(Transform p_target, float p_delay, ref float p_count)
		{
			int childCount = p_target.childCount;
			for (int i = 0; i < childCount; i++)
			{
				BaseTraverseDestroy(p_target.GetChild(i), p_delay, ref p_count);
			}
			if ((bool)p_target)
			{
				UnityEngine.Object.Destroy(p_target.gameObject, p_count * p_delay);
				p_count += 1f;
			}
		}

		public static List<T> FindAll<T>(Transform p_target, bool p_dfs, int p_max) where T : Component
		{
			bool has_found = false;
			List<T> res = new List<T>();
			Traverse(p_target, delegate(T it)
			{
				if (has_found)
				{
					return false;
				}
				res.Add(it);
				if (p_max > 0 && res.Count >= p_max)
				{
					has_found = true;
				}
				return true;
			}, p_dfs);
			return res;
		}

		public static List<T> FindAll<T>(Transform p_target, bool p_dfs) where T : Component
		{
			return FindAll<T>(p_target, p_dfs, 0);
		}

		public static List<T> FindAll<T>(Transform p_target) where T : Component
		{
			return FindAll<T>(p_target, p_dfs: true, 0);
		}

		public static T Find<T>(Transform p_target, bool p_dfs) where T : Component
		{
			List<T> list = FindAll<T>(p_target, p_dfs, 1);
			if (list.Count > 0)
			{
				return list[0];
			}
			return null;
		}

		public static T Find<T>(Transform p_target) where T : Component
		{
			return Find<T>(p_target, p_dfs: true);
		}

		public static List<T> FindAllReverse<T>(Transform p_target, int p_max = 0) where T : Component
		{
			bool has_found = false;
			List<T> res = new List<T>();
			TraverseReverse(p_target, delegate(T it)
			{
				if (has_found)
				{
					return false;
				}
				res.Add(it);
				if (p_max > 0 && res.Count >= p_max)
				{
					has_found = true;
				}
				return true;
			});
			return res;
		}

		public static T FindReverse<T>(Transform p_target) where T : Component
		{
			List<T> list = FindAllReverse<T>(p_target, 1);
			if (list.Count > 0)
			{
				return list[0];
			}
			return null;
		}

		public static Transform Find(Transform p_target, string p_path, string p_separator = ".")
		{
			string text = (string.IsNullOrEmpty(p_separator) ? "." : p_separator);
			string[] array = p_path.Split(text[0]);
			Transform transform = p_target;
			for (int i = 0; i < array.Length; i++)
			{
				transform = transform.Find(array[i]);
				if (!transform)
				{
					return null;
				}
			}
			return transform;
		}

		public static T Find<T>(Transform p_target, string p_path, string p_separator = ".") where T : Component
		{
			Transform transform = Find(p_target, p_path, p_separator);
			T result = null;
			if ((bool)transform)
			{
				return transform.GetComponent<T>();
			}
			return result;
		}

		public static string Path(Transform p_target, Transform p_parent = null)
		{
			if (!p_target)
			{
				return "";
			}
			if ((bool)p_parent)
			{
				if (p_target == p_parent)
				{
					return "";
				}
				if (!p_target.IsChildOf(p_parent))
				{
					return "";
				}
			}
			string text = "";
			Transform transform = p_target;
			int num = 200;
			while ((bool)transform)
			{
				if (transform != p_target)
				{
					text = "." + text;
				}
				text = transform.name + text;
				transform = transform.parent;
				if (transform == p_parent || num-- <= 0)
				{
					break;
				}
			}
			return text;
		}

		public void Traverse<T>(Action<T> p_callback, bool p_dfs = true) where T : Component
		{
			Traverse(base.transform, p_callback, p_dfs);
		}

		public void Traverse(Action<Transform> p_callback, bool p_dfs = true)
		{
			Traverse(base.transform, p_callback, p_dfs);
		}

		public T Find<T>(bool p_dfs = true) where T : Component
		{
			return Find<T>(base.transform, p_dfs);
		}

		public List<T> FindAll<T>(bool p_dfs = true) where T : Component
		{
			return FindAll<T>(base.transform, p_dfs);
		}

		public T FindReverse<T>() where T : Component
		{
			return FindReverse<T>(base.transform);
		}

		public List<T> FindAllReverse<T>() where T : Component
		{
			return FindAllReverse<T>(base.transform);
		}

		public Transform Find(string p_path, string p_separator = ".")
		{
			return Find(base.transform, p_path, p_separator);
		}

		public T Find<T>(string p_path, string p_separator = ".") where T : Component
		{
			return Find<T>(base.transform, p_path, p_separator);
		}

		public T GetComponent<T>(string p_id) where T : Component
		{
			T val = null;
			if (m_ccache.ContainsKey(p_id))
			{
				val = (T)m_ccache[p_id];
			}
			if (!val)
			{
				val = GetComponent<T>();
			}
			if ((bool)val)
			{
				m_ccache[p_id] = val;
			}
			return val;
		}

		public static T GetComponent<T>(GameObject p_target) where T : Component
		{
			T val = null;
			if (!p_target)
			{
				return val;
			}
			string key = p_target.GetInstanceID() + "-" + typeof(T).Name;
			if (m_gocache.ContainsKey(key))
			{
				val = (T)m_gocache[key];
			}
			if (!val)
			{
				val = p_target.GetComponent<T>();
			}
			if ((bool)val)
			{
				m_gocache[key] = val;
			}
			return val;
		}

		public static Vector3 GetAveragePosition<T>(IList<T> p_list, bool p_local) where T : Component
		{
			Vector3 vector = ((p_list.Count <= 0) ? Vector3.zero : ((!p_list[0]) ? Vector3.zero : (p_local ? p_list[0].transform.localPosition : p_list[0].transform.position)));
			float num = ((p_list.Count <= 0) ? 0f : (p_list[0] ? 1f : 0f));
			for (int i = 0; i < p_list.Count; i++)
			{
				vector += (p_list[i] ? (p_local ? p_list[i].transform.localPosition : p_list[i].transform.position) : Vector3.zero);
				num += (p_list[i] ? 1f : 0f);
			}
			if (!(num <= 0f))
			{
				return vector / num;
			}
			return vector;
		}

		public static Vector3 GetAveragePosition(IList<Transform> p_list)
		{
			return GetAveragePosition(p_list, p_local: false);
		}

		public static bool HasChange(IList<Transform> p_list)
		{
			for (int i = 0; i < p_list.Count; i++)
			{
				if (p_list[i].hasChanged)
				{
					return true;
				}
			}
			return false;
		}
	}
}
