using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using drl.game;

namespace thelab.core
{
	public class UINavigation : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		private static Activity m_unfocusDelayTimer;

		private static Activity m_pointerExitDelayTimer;

		private static UINavigation m_lastFocus;

		private static Activity m_focusDisableTimer;

		private static bool focusDisabled;

		private static UINavigation m_focus;

		private static bool m_stateLock;

		private static UINavigation m_search;

		private IFocusHandler[] m_handlers_cache;

		public UINavigation callee;

		public bool disableNavigation;

		[SerializeField]
		private Component m_left;

		[SerializeField]
		private Component m_right;

		[SerializeField]
		private Component m_top;

		[SerializeField]
		private Component m_down;

		public bool calleeRight;

		public bool calleeLeft;

		public bool calleeUp;

		public bool calleeDown;

		private Activity m_click_sequence;

		public static UINavigation focus
		{
			get
			{
				return m_focus;
			}
			set
			{
				if (m_focus == value || focusDisabled || ((bool)value && !value.isActiveAndEnabled))
				{
					return;
				}
				UINavigation uINavigation = m_focus;
				m_focus = value;
				if (value != null)
				{
					StopUnfocus();
					m_lastFocus = value;
				}
				IFocusHandler[] array;
				if ((bool)uINavigation)
				{
					GameObject target = uINavigation.gameObject;
					PointerEventData eventData = new PointerEventData(EventSystem.current);
					ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerExitHandler);
					uINavigation.OnUnfocus();
					array = uINavigation.handlers_cache;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].OnUnfocus();
					}
				}
				UINavigation uINavigation2 = uINavigation;
				if (!m_focus || m_stateLock)
				{
					return;
				}
				m_stateLock = true;
				m_focus.callee = uINavigation2;
				GameObject target2 = m_focus.gameObject;
				PointerEventData eventData2 = new PointerEventData(EventSystem.current);
				ExecuteEvents.Execute(target2, eventData2, ExecuteEvents.pointerEnterHandler);
				m_focus.OnFocus();
				array = m_focus.handlers_cache;
				if (array != null)
				{
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j] != null)
						{
							array[j].OnFocus();
						}
					}
				}
				m_stateLock = false;
			}
		}

		internal IFocusHandler[] handlers_cache
		{
			get
			{
				if (m_handlers_cache == null)
				{
					RefreshHandlersCache();
				}
				return m_handlers_cache;
			}
		}

		public Component left
		{
			get
			{
				if ((bool)callee && calleeLeft)
				{
					return callee;
				}
				return m_left;
			}
			set
			{
				if (!(m_left == value))
				{
					m_left = value;
				}
			}
		}

		public Component right
		{
			get
			{
				if ((bool)callee && calleeRight)
				{
					return callee;
				}
				return m_right;
			}
			set
			{
				if (!(m_right == value))
				{
					m_right = value;
				}
			}
		}

		public Component up
		{
			get
			{
				if ((bool)callee && calleeUp)
				{
					return callee;
				}
				return m_top;
			}
			set
			{
				if (!(m_top == value))
				{
					m_top = value;
				}
			}
		}

		public Component down
		{
			get
			{
				if ((bool)callee && calleeDown)
				{
					return callee;
				}
				return m_down;
			}
			set
			{
				if (!(m_down == value))
				{
					m_down = value;
				}
			}
		}

		public static bool IsFocusDisabled()
		{
			return focusDisabled;
		}

		public static void DisableFocusForTime(float time)
		{
			focusDisabled = true;
			if (m_focusDisableTimer != null)
			{
				m_focusDisableTimer.Stop();
			}
			m_focusDisableTimer = Activity.RunOnce(delegate
			{
				focusDisabled = false;
			}, time);
		}

		public static void RestoreLastFocus()
		{
			StopUnfocus();
			if (focus == null && m_lastFocus != null)
			{
				focus = m_lastFocus;
			}
		}

		public static void ClearFocus(bool p_useDelay = false)
		{
			if (!Cursor.visible || !(focus != null))
			{
				return;
			}
			StopUnfocus();
			if (p_useDelay)
			{
				m_unfocusDelayTimer = Activity.RunOnce(delegate
				{
					m_lastFocus = focus;
					focus = null;
				}, 0.1f);
			}
			else
			{
				m_lastFocus = focus;
				focus = null;
			}
		}

		public static void StopUnfocus()
		{
			if (m_pointerExitDelayTimer != null)
			{
				m_pointerExitDelayTimer.Stop();
			}
			if (m_unfocusDelayTimer != null)
			{
				m_unfocusDelayTimer.Stop();
			}
		}

		public static void Focus(Component p_target)
		{
			if ((bool)p_target)
			{
				UINavigation uINavigation = null;
				if (p_target is UINavigation)
				{
					uINavigation = (UINavigation)p_target;
				}
				else
				{
					m_search = null;
					Hierarchy.Traverse(p_target.transform, (Predicate<UINavigation>)FindFirstActive, false, false);
					uINavigation = m_search;
				}
				if ((bool)uINavigation && uINavigation.gameObject.activeInHierarchy)
				{
					uINavigation.Focus();
				}
			}
		}

		public static void Link(LayoutGroup p_layout, Component p_left = null, Component p_right = null, Component p_up = null, Component p_down = null, bool allow_disabled = false)
		{
			if (!p_layout)
			{
				return;
			}
			int p_stride = 0;
			bool p_vertical = p_layout is VerticalLayoutGroup;
			if (p_layout is GridLayoutGroup)
			{
				GridLayoutGroup obj = p_layout as GridLayoutGroup;
				p_stride = obj.constraintCount;
				p_vertical = obj.startAxis == GridLayoutGroup.Axis.Vertical;
			}
			List<UINavigation> list = new List<UINavigation>();
			int childCount = p_layout.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				UINavigation component = p_layout.transform.GetChild(i).GetComponent<UINavigation>();
				if ((bool)component && (component.isActiveAndEnabled || allow_disabled))
				{
					list.Add(component);
				}
			}
			Link(list.ToArray(), p_stride, p_vertical, p_left, p_right, p_up, p_down);
		}

		public static void LinkGrids(List<GridLayoutGroup> p_grids, Component p_left = null, Component p_right = null)
		{
			if (p_grids == null || p_grids.Count == 0)
			{
				return;
			}
			int constraintCount = p_grids[0].constraintCount;
			for (int i = 0; i < p_grids.Count - 1; i++)
			{
				int num = Mathf.Min(p_grids[i].transform.childCount, constraintCount);
				if (p_left != null && i == 0 && p_grids[0].transform.childCount > 0)
				{
					UINavigation component = p_grids[i].transform.GetChild(0).GetComponent<UINavigation>();
					if (!component)
					{
						break;
					}
					component.left = p_left;
					UINavigation component2 = p_left.GetComponent<UINavigation>();
					if (component2.isActiveAndEnabled)
					{
						component2.right = component;
					}
					if (p_grids[i].transform.childCount > 1)
					{
						for (int j = 1; j < p_grids[i].transform.childCount && j % num != 0; j++)
						{
							p_grids[i].transform.GetChild(j).GetComponent<UINavigation>().left = p_left;
						}
					}
				}
				if (i + 1 > p_grids.Count - 1 && p_right != null)
				{
					int num2 = p_grids[i].transform.childCount - 1 % num;
					UINavigation component3 = p_grids[i].transform.GetChild(p_grids[i].transform.childCount - 1).GetComponent<UINavigation>();
					if (!component3)
					{
						break;
					}
					component3.right = p_right;
					UINavigation component4 = p_right.GetComponent<UINavigation>();
					if ((bool)component4)
					{
						component4.left = component3;
					}
					if (p_grids[i].transform.childCount > 1)
					{
						int num3 = p_grids[i].transform.childCount - 2;
						while (num3 > -1 && num3 % num != num2)
						{
							p_grids[i].transform.GetChild(num3).GetComponent<UINavigation>().right = p_right;
							num3--;
						}
					}
					break;
				}
				for (int k = 0; k < num; k++)
				{
					UINavigation component5 = p_grids[i].transform.GetChild(p_grids[i].transform.childCount - 1 - k).GetComponent<UINavigation>();
					int num4 = (p_grids[i].transform.childCount - 1 - k) % num;
					int num5 = -1;
					num5 = ((num4 < p_grids[i + 1].transform.childCount) ? num4 : (p_grids[i + 1].transform.childCount - 1));
					if (num5 >= 0)
					{
						UINavigation component6 = p_grids[i + 1].transform.GetChild(num5).GetComponent<UINavigation>();
						if ((bool)component5 && (bool)component6 && component5.isActiveAndEnabled && component6.isActiveAndEnabled)
						{
							component5.right = component6;
							component6.left = component5;
						}
					}
				}
			}
		}

		public static void Link(UINavigation[] p_list, int p_stride = 0, bool p_vertical = false, Component p_left = null, Component p_right = null, Component p_up = null, Component p_down = null)
		{
			int num = p_list.Length;
			if (num <= 0)
			{
				return;
			}
			p_stride = ((p_stride <= 0) ? num : p_stride);
			int num2 = Mathf.Min(num, p_stride);
			float f = (p_vertical ? ((float)num / (float)num2) : ((float)num2));
			float f2 = (p_vertical ? ((float)num2) : ((float)num / (float)num2));
			int num3 = Mathf.CeilToInt(f);
			int num4 = Mathf.CeilToInt(f2);
			num2 = p_stride;
			string text = "v[" + p_vertical + "] wh[" + num3 + "," + num4 + "] s[" + num2 + "]\n";
			for (int i = 0; i < num; i++)
			{
				int num5 = (p_vertical ? (i / num2) : (i % num2));
				int num6 = (p_vertical ? (i % num2) : (i / num2));
				UINavigation uINavigation = p_list[i];
				text = text + uINavigation?.ToString() + " i[" + i + "][" + num5 + "," + num6 + "]\n";
				int num7;
				int num8;
				int num9;
				int num10;
				if (p_vertical)
				{
					num7 = num6 - 1 + num5 * num2;
					num8 = num6 + 1 + num5 * num2;
					num9 = num6 + (num5 - 1) * num2;
					num10 = num6 + (num5 + 1) * num2;
				}
				else
				{
					num7 = num5 + (num6 - 1) * num2;
					num8 = num5 + (num6 + 1) * num2;
					num9 = num5 - 1 + num6 * num2;
					num10 = num5 + 1 + num6 * num2;
				}
				if (num6 <= 0)
				{
					num7 = -1;
				}
				if (num6 >= num4 - 1)
				{
					num8 = -1;
				}
				if (num5 <= 0)
				{
					num9 = -1;
				}
				if (num5 >= num3 - 1)
				{
					num10 = -1;
				}
				if (num7 >= num)
				{
					num7 = -1;
				}
				if (num8 >= num)
				{
					num8 = -1;
				}
				if (num9 >= num)
				{
					num9 = -1;
				}
				if (num10 >= num)
				{
					num10 = -1;
				}
				text = text + "u[" + num7 + "] d[" + num8 + "] l[" + num9 + "] r[" + num10 + "]\n";
				Component component = ((num7 < 0) ? p_up : p_list[num7]);
				Component component2 = ((num8 < 0) ? p_down : p_list[num8]);
				Component component3 = ((num9 < 0) ? p_left : p_list[num9]);
				Component component4 = ((num10 < 0) ? p_right : p_list[num10]);
				uINavigation.up = component;
				uINavigation.down = component2;
				uINavigation.left = component3;
				uINavigation.right = component4;
				UINavigation uINavigation2 = null;
				uINavigation2 = null;
				if (p_up is UINavigation)
				{
					uINavigation2 = p_up as UINavigation;
				}
				if (num7 < 0 && (bool)uINavigation2 && !uINavigation2.down)
				{
					uINavigation2.down = uINavigation;
				}
				uINavigation2 = null;
				if (p_down is UINavigation)
				{
					uINavigation2 = p_down as UINavigation;
				}
				if (num8 < 0 && (bool)uINavigation2 && !uINavigation2.up)
				{
					uINavigation2.up = uINavigation;
				}
				uINavigation2 = null;
				if (p_left is UINavigation)
				{
					uINavigation2 = p_left as UINavigation;
				}
				if (num9 < 0 && (bool)uINavigation2 && !uINavigation2.right)
				{
					uINavigation2.right = uINavigation;
				}
				uINavigation2 = null;
				if (p_right is UINavigation)
				{
					uINavigation2 = p_right as UINavigation;
				}
				if (num10 < 0 && (bool)uINavigation2 && !uINavigation2.left)
				{
					uINavigation2.left = uINavigation;
				}
			}
		}

		private static bool FindFirstActive(UINavigation p_item)
		{
			if ((bool)m_search)
			{
				return false;
			}
			if (!p_item)
			{
				return true;
			}
			if (!p_item.isActiveAndEnabled)
			{
				return true;
			}
			m_search = p_item;
			return false;
		}

		private void Start()
		{
			RefreshHandlersCache();
		}

		public void Focus()
		{
			if (!disableNavigation)
			{
				focus = this;
			}
		}

		public void FocusLeft()
		{
			Focus(left);
		}

		public void FocusRight()
		{
			Focus(right);
		}

		public void FocusUp()
		{
			CheckFooterUp();
			Focus(up);
		}

		public void FocusDown()
		{
			if ((!down || IsFooterNav(down)) && UIFooterView.isVisible && !IsFooterNav() && !IsLobbyNav() && UIFooterView.buttonNavs.Count > 0)
			{
				down = GetClosestFooterNav();
				if (down == null)
				{
					down = UIFooterView.buttonNavs[0];
				}
				DRLUINavigationSystem.lastNavigationDown = this;
				UIFooterView.SetNavigationTop(this);
			}
			Focus(down);
		}

		private void CheckFooterUp()
		{
			if (!IsFooterNav() || IsLobbyNav())
			{
				return;
			}
			if (up != null && up is UINavigation && up.gameObject.activeInHierarchy)
			{
				UIScreen uIScreen = Hierarchy.FindReverse<UIScreen>(up.transform);
				if (uIScreen != null)
				{
					Scene sceneByName = SceneManager.GetSceneByName("main");
					if (!sceneByName.IsValid())
					{
						sceneByName = SceneManager.GetSceneByName("game");
					}
					if (!sceneByName.IsValid())
					{
						return;
					}
					UIScreenManagerView uIScreenManagerView = Hierarchy.Find<UIScreenManagerView>(sceneByName.GetRootGameObjects()[0].transform);
					if (uIScreenManagerView == null || uIScreenManagerView.current == null)
					{
						return;
					}
					if (uIScreen.name != uIScreenManagerView.current.name)
					{
						up = null;
					}
				}
			}
			if (DRLUINavigationSystem.lastNavigationDown != null && DRLUINavigationSystem.lastNavigationDown.gameObject.activeInHierarchy)
			{
				up = DRLUINavigationSystem.lastNavigationDown;
				UIFooterView.SetNavigationTop(up);
			}
			if (!(up == null) && up.gameObject.activeInHierarchy)
			{
				return;
			}
			Scene sceneByName2 = SceneManager.GetSceneByName("main");
			if (!sceneByName2.IsValid())
			{
				sceneByName2 = SceneManager.GetSceneByName("game");
			}
			if (sceneByName2.IsValid())
			{
				UIScreenManagerView uIScreenManagerView2 = Hierarchy.Find<UIScreenManagerView>(sceneByName2.GetRootGameObjects()[0].transform);
				if (!(uIScreenManagerView2 == null) && !(uIScreenManagerView2.current == null))
				{
					Focus(uIScreenManagerView2.current.transform);
				}
			}
		}

		private bool IsFooterNav(Component p_target = null)
		{
			if (p_target == null)
			{
				p_target = this;
			}
			List<UINavigation> buttonNavs = UIFooterView.buttonNavs;
			_ = UIFooterView.lobbyNavs;
			for (int i = 0; i < buttonNavs.Count; i++)
			{
				if (buttonNavs[i] == p_target)
				{
					return true;
				}
			}
			return false;
		}

		private bool IsLobbyNav(Component p_target = null)
		{
			if (p_target == null)
			{
				p_target = this;
			}
			List<UINavigation> lobbyNavs = UIFooterView.lobbyNavs;
			for (int i = 0; i < lobbyNavs.Count; i++)
			{
				if (lobbyNavs[i] == p_target)
				{
					return true;
				}
			}
			return false;
		}

		private UINavigation GetClosestFooterNav()
		{
			UINavigation result = null;
			if (!UIFooterView.isVisible || UIFooterView.buttonNavs.Count == 0)
			{
				return null;
			}
			float num = float.MaxValue;
			foreach (UINavigation buttonNav in UIFooterView.buttonNavs)
			{
				if ((bool)buttonNav && buttonNav.gameObject.activeInHierarchy && buttonNav.enabled)
				{
					float num2 = Mathf.Abs(buttonNav.transform.position.x - base.transform.position.x);
					if (num2 < num)
					{
						result = buttonNav;
						num = num2;
					}
				}
			}
			return result;
		}

		public void Click()
		{
			EventSystem esys = EventSystem.current;
			if (!esys)
			{
				return;
			}
			PointerEventData pointer = new PointerEventData(esys);
			int k = 0;
			if (m_click_sequence != null)
			{
				m_click_sequence.Stop();
			}
			m_click_sequence = null;
			m_click_sequence = ((Component)this).ActivityRun((Func<bool>)delegate
			{
				if (base.gameObject == null)
				{
					return false;
				}
				if (!esys)
				{
					return false;
				}
				switch (k++)
				{
				case 0:
					ExecuteEvents.Execute(base.gameObject, pointer, ExecuteEvents.pointerDownHandler);
					break;
				case 2:
					ExecuteEvents.Execute(base.gameObject, pointer, ExecuteEvents.pointerUpHandler);
					ExecuteEvents.Execute(base.gameObject, pointer, ExecuteEvents.pointerClickHandler);
					break;
				default:
					return k < 3;
				}
				return true;
			}, 0f);
		}

		private void RefreshHandlersCache()
		{
			if (m_handlers_cache == null)
			{
				IFocusHandler[] components = GetComponents<IFocusHandler>();
				m_handlers_cache = components;
			}
		}

		public virtual void OnFocus()
		{
		}

		public virtual void OnUnfocus()
		{
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			StopUnfocus();
			if (!(focus == this) && Cursor.visible)
			{
				focus = this;
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (m_pointerExitDelayTimer != null)
			{
				m_pointerExitDelayTimer.Stop();
			}
			m_pointerExitDelayTimer = Activity.RunOnce(delegate
			{
				ClearFocus();
			}, 0.1f);
		}

		public UINavigation GetLeft()
		{
			if (!left)
			{
				return null;
			}
			if (!(left is UINavigation))
			{
				return null;
			}
			return left as UINavigation;
		}

		public UINavigation GetRight()
		{
			if (!right)
			{
				return null;
			}
			if (!(right is UINavigation))
			{
				return null;
			}
			return right as UINavigation;
		}

		public UINavigation GetUp()
		{
			if (!up)
			{
				return null;
			}
			if (!(up is UINavigation))
			{
				return null;
			}
			return up as UINavigation;
		}

		public UINavigation GetDown()
		{
			if (!down)
			{
				return null;
			}
			if (!(down is UINavigation))
			{
				return null;
			}
			return down as UINavigation;
		}
	}
}
