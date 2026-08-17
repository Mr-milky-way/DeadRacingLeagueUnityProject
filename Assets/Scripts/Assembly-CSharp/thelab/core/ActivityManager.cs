using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;

namespace thelab.core
{
	public class ActivityManager : MonoBehaviour
	{
		internal static ActivityManager m_instance;

		internal static GameObject m_instance_go;

		internal static ActivityManager current => m_instance;

		private List<IUpdateable> m_updates;

		private List<IConditionalUpdateable> m_conditional_updates;

		private List<ILateUpdateable> m_late_updates;

		private List<IFixedUpdateable> m_fixed_updates;

		[SerializeField]
		private List<Activity> m_activities;

		[SerializeField]
		private List<ActivityBehaviour> m_behaviours;

		public bool profilerEnabled;

		private List<Thread> m_threads;

		internal int m_nextCore;

		internal static ActivityManager instance
		{
			get
			{
				if (!(m_instance == null))
				{
					return m_instance;
				}
				return m_instance = GetUnique();
			}
		}

		internal List<IUpdateable> updates
		{
			get
			{
				if (m_updates != null)
				{
					return m_updates;
				}
				return m_updates = new List<IUpdateable>();
			}
		}

		internal List<IConditionalUpdateable> conditional_updates
		{
			get
			{
				if (m_conditional_updates != null)
				{
					return m_conditional_updates;
				}
				return m_conditional_updates = new List<IConditionalUpdateable>();
			}
		}

		internal List<ILateUpdateable> late_updates
		{
			get
			{
				if (m_late_updates != null)
				{
					return m_late_updates;
				}
				return m_late_updates = new List<ILateUpdateable>();
			}
		}

		internal List<IFixedUpdateable> fixed_updates
		{
			get
			{
				if (m_fixed_updates != null)
				{
					return m_fixed_updates;
				}
				return m_fixed_updates = new List<IFixedUpdateable>();
			}
		}

		internal List<Activity> activities
		{
			get
			{
				if (m_activities != null)
				{
					return m_activities;
				}
				return m_activities = new List<Activity>();
			}
		}

		internal List<ActivityBehaviour> behaviours
		{
			get
			{
				if (m_behaviours != null)
				{
					return m_behaviours;
				}
				return m_behaviours = new List<ActivityBehaviour>();
			}
		}

		public int cores => Environment.ProcessorCount;

		public List<Thread> threads
		{
			get
			{
				if (m_threads != null)
				{
					return m_threads;
				}
				return m_threads = new List<Thread>();
			}
		}

		internal int nextCore => m_nextCore = (m_nextCore + 1) % cores;

		internal static ActivityManager GetUnique()
		{
			ActivityManager[] array = UnityEngine.Object.FindObjectsOfType<ActivityManager>();
			ActivityManager activityManager;
			if (array.Length == 0)
			{
				activityManager = new GameObject
				{
					name = "activity-manager"
				}.AddComponent<ActivityManager>();
				activityManager.m_nextCore = 0;
			}
			else
			{
				activityManager = array[0];
				for (int i = 1; i < array.Length; i++)
				{
					if (activityManager.GetInstanceID() < array[i].GetInstanceID())
					{
						activityManager = array[i];
					}
				}
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] != activityManager)
					{
						UnityEngine.Object.DestroyImmediate(array[j].gameObject);
					}
				}
			}
			m_instance_go = (activityManager ? activityManager.gameObject : null);
			return activityManager;
		}

		private void OnDestroy()
		{
			if (m_instance == this)
			{
				m_instance = null;
				m_instance_go = null;
			}
		}

		public bool Add(object p_node)
		{
			IList list = GetList(p_node);
			if (list.IndexOf(p_node) >= 0)
			{
				return false;
			}
			list.Add(p_node);
			return true;
		}

		public bool Remove(object p_node)
		{
			IList list = GetList(p_node);
			if (list == null)
			{
				return false;
			}
			if (list.IndexOf(p_node) < 0)
			{
				return false;
			}
			list.Remove(p_node);
			return true;
		}

		public void Clear()
		{
			foreach (Thread thread in threads)
			{
				thread.Abort();
			}
			behaviours.Clear();
			activities.Clear();
			updates.Clear();
			conditional_updates.Clear();
			late_updates.Clear();
			fixed_updates.Clear();
		}

		public int RemoveByName(string p_name)
		{
			int num = 0;
			List<Activity> list = activities;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].name == p_name)
				{
					list.RemoveAt(i--);
					num++;
				}
			}
			List<ActivityBehaviour> list2 = behaviours;
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j].name == p_name)
				{
					list2.RemoveAt(j--);
					num++;
				}
			}
			return num;
		}

		public IList GetList(object p_node)
		{
			if (p_node is ActivityBehaviour)
			{
				return behaviours;
			}
			if (p_node is Activity)
			{
				return activities;
			}
			if (p_node is IUpdateable)
			{
				return updates;
			}
			if (p_node is IConditionalUpdateable)
			{
				return conditional_updates;
			}
			if (p_node is ILateUpdateable)
			{
				return late_updates;
			}
			if (p_node is IFixedUpdateable)
			{
				return fixed_updates;
			}
			return null;
		}

		public void ThreadStart()
		{
		}

		private void CreateThread(int p_id)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				while (true)
				{
					ThreadUpdate(p_id);
					Thread.Sleep(0);
				}
			});
			threads.Add(thread);
			thread.Name = "@activity-" + p_id;
			thread.Priority = System.Threading.ThreadPriority.AboveNormal;
			thread.Start();
		}

		public void Update()
		{
			UpdateStep();
		}

		private void UpdateStep()
		{
			bool flag = profilerEnabled && Profiler.enabled;
			List<IUpdateable> list = updates;
			for (int i = 0; i < list.Count; i++)
			{
				if ((list[i] is Component && (Component)list[i] == null) || list[i] == null)
				{
					list.RemoveAt(i--);
					continue;
				}
				if (flag)
				{
					_ = "interface." + i;
				}
				list[i].OnUpdate();
			}
			List<Activity> list2 = activities;
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j] == null || !list2[j].valid)
				{
					list2.RemoveAt(j--);
				}
				else
				{
					if (list2[j].threaded)
					{
						continue;
					}
					if (flag)
					{
						Component component = ((list2[j].context is Component) ? ((Component)list2[j].context) : null);
						if (!component)
						{
							list2[j].name.ToLower();
						}
						else
						{
							_ = "activity." + component.name;
						}
					}
					list2[j].OnUpdate();
				}
			}
			List<ActivityBehaviour> list3 = behaviours;
			for (int k = 0; k < list3.Count; k++)
			{
				if (!list3[k])
				{
					list3.RemoveAt(k--);
					continue;
				}
				if (flag)
				{
					Component component2 = list3[k];
					_ = "activity." + component2.name;
				}
				if (list3[k] is IUpdateable)
				{
					((IUpdateable)list3[k]).OnUpdate();
				}
				if (list3[k] is IConditionalUpdateable && !((IConditionalUpdateable)list3[k]).OnConditionUpdate())
				{
					list3.RemoveAt(k--);
				}
			}
		}

		public void LateUpdate()
		{
			bool flag = profilerEnabled && Profiler.enabled;
			List<ILateUpdateable> list = late_updates;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == null)
				{
					list.RemoveAt(i--);
					continue;
				}
				if (flag)
				{
					_ = "interface." + i;
				}
				list[i].OnLateUpdate();
			}
			List<Activity> list2 = activities;
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j] == null || !list2[j].valid)
				{
					list2.RemoveAt(j--);
				}
				else
				{
					if (list2[j].threaded)
					{
						continue;
					}
					if (flag)
					{
						Component component = ((list2[j].context is Component) ? ((Component)list2[j].context) : null);
						if (!component)
						{
							list2[j].name.ToLower();
						}
						else
						{
							_ = "activity." + component.name;
						}
					}
					list2[j].OnLateUpdate();
				}
			}
			List<ActivityBehaviour> list3 = behaviours;
			for (int k = 0; k < list3.Count; k++)
			{
				if (!list3[k])
				{
					list3.RemoveAt(k--);
					continue;
				}
				if (list3[k] is ILateUpdateable)
				{
					ILateUpdateable obj = (ILateUpdateable)list3[k];
					if (flag)
					{
						Component component2 = list3[k];
						_ = "activity." + component2.name;
					}
					obj.OnLateUpdate();
				}
			}
		}

		public void ThreadUpdate(int p_core)
		{
			if (!this)
			{
				Thread.CurrentThread.Abort();
				return;
			}
			List<Activity> list = activities;
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].valid)
				{
					list.RemoveAt(i--);
				}
				else if (list[i].threaded && list[i].core == p_core)
				{
					list[i].OnUpdate();
				}
			}
		}

		public void FixedUpdate()
		{
			bool flag = profilerEnabled && Profiler.enabled;
			List<IFixedUpdateable> list = fixed_updates;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == null)
				{
					list.RemoveAt(i--);
					continue;
				}
				if (flag)
				{
					_ = "interface." + i;
				}
				list[i].OnFixedUpdate();
			}
			List<ActivityBehaviour> list2 = behaviours;
			for (int j = 0; j < list2.Count; j++)
			{
				if (!list2[j])
				{
					list2.RemoveAt(j--);
					continue;
				}
				if (list2[j] is IFixedUpdateable)
				{
					IFixedUpdateable obj = (IFixedUpdateable)list2[j];
					if (flag)
					{
						Component component = list2[j];
						_ = "activity." + component.name;
					}
					obj.OnFixedUpdate();
				}
			}
		}
	}
}
