using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace thelab.mvc
{
	public class Element<T> : Element where T : BaseApplication
	{
		public new T app => (T)base.app;
	}
	public class Element : MonoBehaviour
	{
		private BaseApplication m_app;

		private Dictionary<string, object> _store;

		public BaseApplication app
		{
			get
			{
				if ((bool)m_app)
				{
					return m_app;
				}
				m_app = AssertFindReverse<BaseApplication>();
				if ((bool)m_app)
				{
					return m_app;
				}
				return m_app = Assert(m_app, p_global: true);
			}
		}

		public string transformPath
		{
			get
			{
				string text = "";
				Transform parent = base.transform;
				while ((bool)parent)
				{
					text = parent.name + text;
					parent = parent.parent;
					if ((bool)parent)
					{
						text = "." + text;
					}
				}
				return text;
			}
		}

		public bool validContext
		{
			get
			{
				if (!this)
				{
					return false;
				}
				if (!base.gameObject)
				{
					return false;
				}
				if (!app)
				{
					return false;
				}
				return true;
			}
		}

		private Dictionary<string, object> m_store
		{
			get
			{
				if (_store != null)
				{
					return _store;
				}
				return _store = new Dictionary<string, object>();
			}
		}

		public T Assert<T>(T p_var, bool p_global = false) where T : UnityEngine.Object
		{
			Transform transform = ((this == null) ? null : (base.gameObject ? base.transform : null));
			if (!transform)
			{
				return null;
			}
			if (!(p_var == null))
			{
				return p_var;
			}
			if (!p_global)
			{
				return transform.GetComponentInChildren<T>();
			}
			return UnityEngine.Object.FindObjectOfType<T>();
		}

		public T Assert<T>(string p_key, bool p_global = false) where T : UnityEngine.Object
		{
			Transform transform = (app ? app.transform : null);
			Transform transform2 = (base.transform ? base.transform : null);
			T v = null;
			if (m_store.ContainsKey(p_key))
			{
				v = (T)m_store[p_key];
				if ((bool)v)
				{
					return v;
				}
			}
			if (p_global && !typeof(T).IsSubclassOf(typeof(Component)))
			{
				v = UnityEngine.Object.FindObjectOfType<T>();
			}
			else
			{
				Transform transform3 = (p_global ? transform : transform2);
				if ((bool)transform3)
				{
					Hierarchy.Traverse(transform3, delegate(Transform it)
					{
						if (v != null)
						{
							return false;
						}
						v = it.GetComponent<T>();
						return true;
					});
				}
			}
			if ((bool)transform2 && (bool)transform && v == null)
			{
				Debug.Log("Element> Assert Failed [" + Hierarchy.Path(transform2, transform) + "]");
			}
			m_store[p_key] = v;
			return v;
		}

		public T AssertLocal<T>(string p_key) where T : UnityEngine.Object
		{
			T val;
			if (m_store.ContainsKey(p_key))
			{
				val = (T)m_store[p_key];
				if ((bool)val)
				{
					return val;
				}
			}
			if (this == null)
			{
				return null;
			}
			if (!base.gameObject)
			{
				return null;
			}
			if (!base.transform)
			{
				return null;
			}
			val = GetComponent<T>();
			m_store[p_key] = val;
			return val;
		}

		public T AssertLocal<T>(T p_var, string p_store = "") where T : UnityEngine.Object
		{
			T val = null;
			if (p_store != "" && m_store.ContainsKey(p_store))
			{
				val = (T)m_store[p_store];
				if ((bool)val)
				{
					return val;
				}
			}
			if (!base.gameObject)
			{
				return null;
			}
			if (!base.transform)
			{
				return null;
			}
			val = p_var ?? (p_var = GetComponent<T>());
			if (p_store != "")
			{
				m_store[p_store] = val;
			}
			return val;
		}

		public T AssertParent<T>(string p_key) where T : UnityEngine.Object
		{
			T val;
			if (m_store.ContainsKey(p_key))
			{
				val = (T)m_store[p_key];
				if ((bool)val)
				{
					return val;
				}
			}
			val = (base.transform ? GetComponentInParent<T>() : null);
			m_store[p_key] = val;
			return val;
		}

		public T AssertParent<T>(T p_var, string p_store = "") where T : UnityEngine.Object
		{
			T val = null;
			if (p_store != "" && m_store.ContainsKey(p_store))
			{
				val = (T)m_store[p_store];
				if ((bool)val)
				{
					return val;
				}
			}
			val = ((!(p_var == null)) ? p_var : (p_var = (base.transform ? GetComponentInParent<T>() : null)));
			if (p_store != "")
			{
				m_store[p_store] = val;
			}
			return val;
		}

		public T AssertCache<T>(string p_store, T p_value)
		{
			T val = default(T);
			if (m_store.ContainsKey(p_store))
			{
				val = (T)m_store[p_store];
				if (val != null)
				{
					return val;
				}
			}
			m_store[p_store] = p_value;
			return p_value;
		}

		public T Cast<T>()
		{
			return (T)(object)this;
		}

		public T Find<T>(string p_path) where T : Component
		{
			List<string> list = new List<string>(p_path.Split('.'));
			if (list.Count <= 0)
			{
				return null;
			}
			if (!validContext)
			{
				return null;
			}
			Transform transform = base.transform;
			while (list.Count > 0)
			{
				string n = list[0];
				list.RemoveAt(0);
				transform = transform.Find(n);
				if (transform == null)
				{
					return null;
				}
			}
			return transform.GetComponent<T>();
		}

		public T AssertFind<T>(string p_path) where T : Component
		{
			T val;
			if (m_store.ContainsKey(p_path))
			{
				val = (T)m_store[p_path];
				if ((bool)val)
				{
					return val;
				}
			}
			val = Find<T>(p_path);
			if ((bool)val)
			{
				m_store[p_path] = val;
			}
			return val;
		}

		public T AssertFindReverse<T>() where T : Component
		{
			if (!this)
			{
				return null;
			}
			if (!base.gameObject)
			{
				return null;
			}
			T val = null;
			int num = 0;
			string text = "$parent-" + typeof(T).Name + "-";
			Transform parent = base.transform.parent;
			while ((bool)parent)
			{
				string key = text + num;
				if (m_store.ContainsKey(key))
				{
					val = (T)m_store[key];
					break;
				}
				val = parent.GetComponent<T>();
				if ((bool)val)
				{
					m_store[key] = val;
					break;
				}
				parent = parent.parent;
				num++;
			}
			return val;
		}

		public virtual void Notify(string p_event, params object[] p_data)
		{
			if ((bool)app)
			{
				app.Notify(p_event, this, p_data);
			}
		}

		public virtual void Notify(float p_delay, string p_event, params object[] p_data)
		{
			if ((bool)app)
			{
				app.Notify(p_delay, p_event, this, p_data);
			}
		}

		public void Traverse(Predicate<Transform> p_callback)
		{
			OnTraverseStep(base.transform, p_callback);
		}

		public string Path(Component p_child, string p_separator = ".")
		{
			if (!p_child)
			{
				return "";
			}
			if (!p_child.transform.IsChildOf(base.transform))
			{
				return "";
			}
			Transform parent = p_child.transform;
			string text = "";
			while ((bool)parent)
			{
				text = parent.name + text;
				if (parent == base.transform)
				{
					break;
				}
				parent = parent.parent;
				if (!parent)
				{
					break;
				}
				text = p_separator + text;
			}
			return text;
		}

		private void OnTraverseStep(Transform p_target, Predicate<Transform> p_callback)
		{
			if (!p_target || p_callback(p_target))
			{
				for (int i = 0; i < p_target.childCount; i++)
				{
					OnTraverseStep(p_target.GetChild(i), p_callback);
				}
			}
		}

		public void RunOnce(float p_delay, Action p_callback)
		{
			StartCoroutine(TimedCallback(p_delay, p_callback));
		}

		private IEnumerator TimedCallback(float p_delay, Action p_callback)
		{
			float timeScale = Time.timeScale;
			yield return new WaitForSeconds(p_delay * timeScale);
			p_callback?.Invoke();
		}

		public MonoActivity Run(Predicate<float> callback, float delay = 0f, bool unscaledTime = false)
		{
			return this.MonoActivityRun(callback, delay);
		}

		public MonoActivity Run(Func<bool> callback, float delay = 0f, bool unscaledTime = false)
		{
			return this.MonoActivityRun(callback, delay);
		}

		public MonoActivity RunOnce(Action callback, float delay = 0f, bool unscaledTime = false)
		{
			return this.MonoActivityRunOnce(callback, delay);
		}

		public void Log(object p_msg, int p_verbose = 0)
		{
			if ((bool)app && p_verbose <= app.verbose)
			{
				Debug.Log(GetType().Name + "> " + p_msg);
			}
		}
	}
}
