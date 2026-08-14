using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[RequireComponent(typeof(AssetLibrary))]
	public class UIScreenManager : Hierarchy
	{
		public static float defaultDuration = 0.25f;

		private AssetLibrary m_library;

		[SerializeField]
		private List<UIScreen> m_history;

		private static Transform _pool;

		public AssetLibrary library
		{
			get
			{
				if (!m_library)
				{
					return m_library = GetComponent<AssetLibrary>();
				}
				return m_library;
			}
		}

		public UIScreen front => Get(0);

		public UIScreen back => Get(100000);

		public List<UIScreen> history
		{
			get
			{
				if (m_history != null)
				{
					return m_history;
				}
				return m_history = new List<UIScreen>();
			}
		}

		public string path
		{
			get
			{
				string text = "";
				for (int i = 0; i < history.Count; i++)
				{
					text += history[i].title;
					if (i < history.Count - 1)
					{
						text += "/";
					}
				}
				return text;
			}
		}

		private Transform m_pool
		{
			get
			{
				if (!_pool)
				{
					return _pool = new GameObject("$screen-manager-pool").transform;
				}
				return _pool;
			}
		}

		public int count => GetScreens().Count;

		public List<UIScreen> GetScreens(bool p_only_open)
		{
			UIScreen uIScreen = null;
			List<UIScreen> list = new List<UIScreen>();
			int childCount = base.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				int index = childCount - 1 - i;
				Transform child = base.transform.GetChild(index);
				if ((bool)child)
				{
					uIScreen = child.GetComponent<UIScreen>();
					if ((bool)uIScreen && (!p_only_open || uIScreen.open))
					{
						list.Add(uIScreen);
					}
				}
			}
			return list;
		}

		public List<UIScreen> GetScreens()
		{
			return GetScreens(p_only_open: true);
		}

		public UIScreen Get(int p_index)
		{
			UIScreen uIScreen = null;
			int num = 0;
			int childCount = base.transform.childCount;
			int num2 = Mathf.Clamp(p_index, 0, childCount - 1);
			for (int i = 0; i < childCount; i++)
			{
				int index = childCount - 1 - i;
				Transform child = base.transform.GetChild(index);
				if (!child)
				{
					continue;
				}
				uIScreen = child.GetComponent<UIScreen>();
				if ((bool)uIScreen)
				{
					if (num == num2)
					{
						return uIScreen;
					}
					num++;
				}
			}
			return uIScreen;
		}

		public T Get<T>(string p_id, bool p_create) where T : UIScreen
		{
			T val = Find<T>(p_id);
			if (p_create || val == null)
			{
				val = library.Instantiate<T>(p_id);
			}
			if ((bool)val)
			{
				val.name = p_id;
				val.transition = 0f;
			}
			return val;
		}

		public T Get<T>(string p_id) where T : UIScreen
		{
			return Get<T>(p_id, p_create: true);
		}

		public UIScreen Get(string p_id, bool p_create)
		{
			return Get<UIScreen>(p_id, p_create);
		}

		public UIScreen Get(string p_id)
		{
			return Get<UIScreen>(p_id, p_create: true);
		}

		public bool IsOpen(UIScreen p_screen)
		{
			if (!p_screen)
			{
				return false;
			}
			UIScreen uIScreen = null;
			int childCount = base.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				int index = childCount - 1 - i;
				Transform child = base.transform.GetChild(index);
				if (!child)
				{
					continue;
				}
				uIScreen = child.GetComponent<UIScreen>();
				if ((bool)uIScreen)
				{
					if (uIScreen != p_screen)
					{
						return false;
					}
					return uIScreen.isActiveAndEnabled;
				}
			}
			return false;
		}

		public bool IsOpen(string p_id)
		{
			UIScreen p_screen = Find<UIScreen>(p_id);
			return IsOpen(p_screen);
		}

		public bool IsInHistory(string p_id)
		{
			return Find<UIScreen>(p_id) != null;
		}

		public bool IsOpen<T>() where T : Behaviour
		{
			T val = Find<T>();
			if (!val)
			{
				return false;
			}
			return val.isActiveAndEnabled;
		}

		public T GetOpen<T>() where T : Behaviour
		{
			if (!IsOpen<T>())
			{
				return null;
			}
			return Find<T>();
		}

		public bool IsClosed(UIScreen p_screen)
		{
			return !IsOpen(p_screen);
		}

		public bool IsClosed(string p_id)
		{
			return !IsOpen(p_id);
		}

		public bool IsClosed<T>() where T : Behaviour
		{
			return !IsOpen<T>();
		}

		public UIScreen Open(UIScreen p_screen, bool p_close_all, float p_duration, float p_delay)
		{
			if (!p_screen)
			{
				return p_screen;
			}
			Sort(p_screen);
			history.Add(p_screen);
			if (p_close_all)
			{
				List<UIScreen> screens = GetScreens();
				for (int i = 0; i < screens.Count; i++)
				{
					if ((bool)screens[i] && screens[i] != p_screen)
					{
						Close(screens[i], p_duration, 0f);
					}
				}
			}
			return p_screen;
		}

		public UIScreen Open(UIScreen p_screen, float p_duration, float p_delay)
		{
			return Open(p_screen, p_close_all: true, p_duration, p_delay);
		}

		public UIScreen Open(UIScreen p_screen, float p_duration)
		{
			return Open(p_screen, p_close_all: true, p_duration, 0f);
		}

		public UIScreen Open(UIScreen p_screen)
		{
			return Open(p_screen, p_close_all: true, defaultDuration, 0f);
		}

		public UIScreen Open(UIScreen p_screen, bool p_close_all, float p_duration)
		{
			return Open(p_screen, p_close_all, p_duration, 0f);
		}

		public UIScreen Open(UIScreen p_screen, bool p_close_all)
		{
			return Open(p_screen, p_close_all, defaultDuration, 0f);
		}

		public UIScreen Push(UIScreen p_screen, float p_duration, float p_delay)
		{
			return Open(p_screen, p_close_all: false, p_duration, p_delay);
		}

		public UIScreen Push(UIScreen p_screen, float p_duration)
		{
			return Push(p_screen, p_duration, 0f);
		}

		public UIScreen Push(UIScreen p_screen)
		{
			return Push(p_screen, defaultDuration, 0f);
		}

		public T Push<T>(string p_id, float p_duration, float p_delay) where T : UIScreen
		{
			T val = Get<T>(p_id);
			Push(val, p_duration, p_delay);
			return val;
		}

		public T Push<T>(string p_id, float p_duration) where T : UIScreen
		{
			return Push<T>(p_id, p_duration, 0f);
		}

		public T Push<T>(string p_id) where T : UIScreen
		{
			return Push<T>(p_id, defaultDuration, 0f);
		}

		public UIScreen Push(string p_id, float p_duration, float p_delay)
		{
			return Push<UIScreen>(p_id, p_duration, p_delay);
		}

		public UIScreen Push(string p_id, float p_duration)
		{
			return Push<UIScreen>(p_id, p_duration, 0f);
		}

		public UIScreen Push(string p_id)
		{
			return Push<UIScreen>(p_id, defaultDuration, 0f);
		}

		public void Close(UIScreen p_screen, float p_duration, float p_delay)
		{
			if ((bool)p_screen && !(p_screen.transform.parent != base.transform))
			{
				p_screen.Hide(p_duration, p_delay, Cubic.Out);
			}
		}

		public void Close(string p_id, float p_duration, float p_delay)
		{
			UIScreen uIScreen = Find<UIScreen>(p_id);
			if ((bool)uIScreen)
			{
				Close(uIScreen, p_duration, p_delay);
			}
		}

		public void Close(UIScreen p_screen, float p_duration)
		{
			Close(p_screen, p_duration, 0f);
		}

		public void Close(UIScreen p_screen)
		{
			Close(p_screen, defaultDuration, 0f);
		}

		public void Close(string p_id, float p_duration)
		{
			Close(p_id, p_duration, 0f);
		}

		public void Close(string p_id)
		{
			Close(p_id, defaultDuration, 0f);
		}

		public void Close(float p_duration, float p_delay)
		{
			Close(front, p_duration, p_delay);
		}

		public void Close(float p_duration)
		{
			Close(front, p_duration, 0f);
		}

		public void Close()
		{
			Close(front, defaultDuration, 0f);
		}

		public void Switch(UIScreen a, UIScreen b, bool p_sequential, float p_duration, float p_delay)
		{
			if (!(a == b))
			{
				float num = p_delay;
				if ((bool)a)
				{
					Close(a, p_duration, num);
				}
				if (p_sequential)
				{
					num += p_duration;
				}
				if ((bool)b)
				{
					Push(b, p_duration, num);
				}
			}
		}

		public void Switch(UIScreen a, UIScreen b, float p_duration, float p_delay)
		{
			Switch(a, b, p_sequential: false, p_duration, p_delay);
		}

		public void Switch(UIScreen a, UIScreen b, bool p_sequential, float p_duration)
		{
			Switch(a, b, p_sequential, p_duration, 0f);
		}

		public void Switch(UIScreen a, UIScreen b, bool p_sequential)
		{
			Switch(a, b, p_sequential, defaultDuration, 0f);
		}

		public void Switch(UIScreen a, UIScreen b)
		{
			Switch(a, b, p_sequential: false, defaultDuration, 0f);
		}

		public void ClearHistory()
		{
			history.Clear();
		}

		public void ClearHistory(UIScreen p_screen, bool p_all)
		{
			if (p_all)
			{
				history.Remove(p_screen);
				return;
			}
			for (int i = 0; i < history.Count; i++)
			{
				int index = history.Count - 1 - i;
				if (!(history[index] != p_screen))
				{
					history.RemoveAt(i);
					break;
				}
			}
		}

		public void ClearHistory(UIScreen p_screen)
		{
			ClearHistory(p_screen, p_all: true);
		}

		public void ClearHistory(string p_screenID)
		{
			for (int i = 0; i < history.Count; i++)
			{
				if (history[i].name == p_screenID)
				{
					history.RemoveAt(i--);
				}
			}
		}

		public UIScreen BackHistory(bool p_sequential, float p_duration, float p_delay)
		{
			UIScreen uIScreen = ((history.Count <= 0) ? null : history[history.Count - 1]);
			UIScreen uIScreen2 = ((history.Count <= 1) ? null : history[history.Count - 2]);
			if ((bool)uIScreen)
			{
				history.Remove(uIScreen);
			}
			Switch(uIScreen, uIScreen2, p_sequential, p_duration, p_delay);
			return uIScreen2;
		}

		public UIScreen BackHistory(float p_duration, float p_delay)
		{
			return BackHistory(p_sequential: false, p_duration, p_delay);
		}

		public UIScreen BackHistory(float p_duration)
		{
			return BackHistory(p_sequential: false, p_duration, 0f);
		}

		public UIScreen BackHistory()
		{
			return BackHistory(p_sequential: false, defaultDuration, 0f);
		}

		public UIScreen Back(bool p_sequential, float p_duration)
		{
			return BackHistory(p_sequential, p_duration, 0f);
		}

		public UIScreen BackHistory(bool p_sequential)
		{
			return BackHistory(p_sequential, defaultDuration, 0f);
		}

		public bool InHistory(string p_id)
		{
			for (int i = 0; i < history.Count; i++)
			{
				if (history[i].name == p_id)
				{
					return true;
				}
			}
			return false;
		}

		public void RemoveFromHistory(UIScreen p_screen, bool p_firstOnly = false)
		{
			for (int i = 0; i < history.Count; i++)
			{
				if (history[i] == p_screen)
				{
					history.RemoveAt(i);
					if (p_firstOnly)
					{
						break;
					}
				}
			}
		}

		public void Sort(UIScreen p_screen)
		{
			UIScreen uIScreen = p_screen;
			if (!uIScreen)
			{
				return;
			}
			if (uIScreen.transform.parent != base.transform)
			{
				uIScreen.transform.SetParent(base.transform, worldPositionStays: false);
			}
			List<UIScreen> list = new List<UIScreen>();
			for (int i = 0; i < base.transform.childCount; i++)
			{
				UIScreen component = base.transform.GetChild(i).GetComponent<UIScreen>();
				if ((bool)component)
				{
					list.Add(component);
				}
			}
			list.Sort(delegate(UIScreen a, UIScreen b)
			{
				if (a.order == b.order)
				{
					if (!(a == p_screen))
					{
						if (!(b == p_screen))
						{
							return 0;
						}
						return 1;
					}
					return -1;
				}
				return (a.order >= b.order) ? 1 : (-1);
			});
			for (int num = 0; num < list.Count; num++)
			{
				uIScreen.transform.SetSiblingIndex(num);
			}
		}
	}
}
