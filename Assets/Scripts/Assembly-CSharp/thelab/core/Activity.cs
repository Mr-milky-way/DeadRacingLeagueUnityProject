using System;
using System.Collections;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class Activity : IUpdateable, ILateUpdateable
	{
		private DateTime m_tst;

		private float m_tick;

		public string name;

		public bool unscaledTime;

		public bool late;

		private ActivityManager m_manager;

		private bool m_has_context;

		private object m_context;

		public bool paused;

		public bool editor;

		public Action<Activity> OnActivityExecute;

		public Action<Activity> OnActivityStart;

		public Action<Activity> OnActivityStop;

		public float elapsed { get; internal set; }

		public float deltaTime { get; internal set; }

		private TimeSpan m_tsp => DateTime.UtcNow - m_tst;

		public bool threaded { get; internal set; }

		public int core { get; internal set; }

		public ActivityManager manager
		{
			get
			{
				if (!ReferenceEquals(m_manager, null))
				{
					return m_manager;
				}
				if (!m_manager)
				{
					return m_manager = ActivityManager.instance;
				}
				return m_manager;
			}
		}

		public bool active
		{
			get
			{
				if (!m_manager)
				{
					return false;
				}
				IList list = m_manager.GetList(this);
				if (list == null)
				{
					return false;
				}
				return list.IndexOf(this) >= 0;
			}
		}

		public bool running
		{
			get
			{
				if (elapsed >= 0f)
				{
					return active;
				}
				return false;
			}
		}

		public object context
		{
			get
			{
				return m_context;
			}
			set
			{
				m_has_context = value != null;
				m_context = value;
			}
		}

		public bool valid
		{
			get
			{
				if (!m_has_context)
				{
					return true;
				}
				if (!(context is Component))
				{
					return context != null;
				}
				return (Component)context != null;
			}
		}

		public static void Init()
		{
			_ = ActivityManager.instance;
		}

		public static bool Add(object p_node)
		{
			return ActivityManager.instance.Add(p_node);
		}

		public static bool Remove(object p_node)
		{
			ActivityManager current = ActivityManager.current;
			return current && current.Remove(p_node);
		}

		public static void Clear()
		{
			ActivityManager.instance.Clear();
		}

		public static Activity Run(Action<Activity> p_callback, float p_delay = 0f, bool p_threaded = false)
		{
			Activity activity = new Activity(p_delay, p_threaded);
			activity.OnActivityExecute = p_callback;
			activity.Start();
			return activity;
		}

		public static Activity Run(Predicate<float> p_callback, float p_delay = 0f, bool p_threaded = false)
		{
			Activity activity = new Activity(p_delay, p_threaded);
			activity.OnActivityExecute = delegate(Activity a)
			{
				if (p_callback == null || !p_callback(a.elapsed))
				{
					a.Stop();
				}
			};
			activity.Start();
			return activity;
		}

		public static Activity Run(Func<bool> p_callback, float p_delay = 0f, bool p_threaded = false)
		{
			Activity activity = new Activity(p_delay, p_threaded);
			activity.OnActivityExecute = delegate(Activity a)
			{
				if (p_callback == null || !p_callback())
				{
					a.Stop();
				}
			};
			activity.Start();
			return activity;
		}

		public static Activity Run(Action p_callback, float p_duration = 0f, float p_delay = 0f, bool p_threaded = false)
		{
			Activity activity = new Activity(p_delay, p_threaded);
			activity.OnActivityExecute = delegate(Activity a)
			{
				if (a.elapsed >= p_duration)
				{
					a.Stop();
				}
				if (p_callback != null)
				{
					p_callback();
				}
			};
			activity.Start();
			return activity;
		}

		public static Activity RunOnce(Action p_callback, float p_delay = 0f, bool p_threaded = false)
		{
			Activity activity = new Activity(p_delay, p_threaded);
			activity.OnActivityExecute = delegate(Activity a)
			{
				a.Stop();
				if (p_callback != null)
				{
					p_callback();
				}
			};
			activity.Start();
			return activity;
		}

		public Activity(float p_delay = 0f, bool p_threaded = false, bool p_editor = false)
		{
			elapsed = 0f - p_delay;
			threaded = p_threaded;
			editor = p_editor;
			unscaledTime = true;
			if (threaded)
			{
				core = manager.nextCore;
			}
			paused = false;
			name = GetType().Name + "-" + GetHashCode().ToString("X");
		}

		public void Stop()
		{
			if (!editor && m_manager && m_manager.Remove(this))
			{
				OnStop();
				if (OnActivityStop != null)
				{
					OnActivityStop(this);
				}
			}
		}

		public void Start()
		{
			m_tst = DateTime.UtcNow;
			m_tick = 0f;
			if (!editor)
			{
				manager.Add(this);
			}
		}

		public virtual void OnUpdate()
		{
			if (!late)
			{
				Step();
			}
		}

		public virtual void OnLateUpdate()
		{
			if (late)
			{
				Step();
			}
		}

		private void Step()
		{
			if (paused)
			{
				return;
			}
			if (!threaded && Application.isPlaying)
			{
				deltaTime = (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
			}
			else
			{
				float num = (float)m_tsp.TotalMilliseconds;
				deltaTime = (num - m_tick) * 0.001f;
				if (deltaTime <= 0f)
				{
					deltaTime = 1f / 60f;
				}
				m_tick = num;
			}
			bool num2 = elapsed <= 0f;
			elapsed += deltaTime;
			bool flag = elapsed >= 0f;
			if (num2 && flag)
			{
				OnStart();
				if (OnActivityStart != null)
				{
					OnActivityStart(this);
				}
			}
			if (flag)
			{
				OnExecute();
				if (OnActivityExecute != null)
				{
					OnActivityExecute(this);
				}
			}
		}

		protected virtual void OnStart()
		{
		}

		protected virtual void OnStop()
		{
		}

		protected virtual void OnExecute()
		{
		}
	}
}
