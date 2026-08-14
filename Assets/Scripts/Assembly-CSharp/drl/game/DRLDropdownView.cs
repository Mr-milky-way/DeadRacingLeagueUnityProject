using UnityEngine;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLDropdownView : DropdownView
	{
		private bool m_focused;

		public Component downNavigation;

		private bool m_opened;

		private bool is_down;

		private bool is_up;

		public UINavigation nav => AssertLocal<UINavigation>("nav");

		public override void OnFocus()
		{
			base.OnFocus();
			m_focused = true;
		}

		public override void OnUnfocus()
		{
			base.OnFocus();
			m_focused = false;
		}

		private void OnEnable()
		{
			base.dropdown.interactable = false;
		}

		protected override void Start()
		{
			base.Start();
		}

		protected override void OnOpen()
		{
			base.OnOpen();
			RefreshNavigation();
			Notify(notification + "@open");
		}

		protected override void OnClose()
		{
			base.OnClose();
			nav.down = downNavigation;
			Notify(notification + "@close");
			ReturnFocus();
		}

		private void RefreshNavigation()
		{
			if (nav == null || optionItems == null || optionItems.Count == 0)
			{
				return;
			}
			for (int i = 0; i < optionItems.Count && !(optionItems[i] == null); i++)
			{
				optionItems[i].GetComponent<Toggle>().navigation = new Navigation
				{
					mode = Navigation.Mode.None
				};
				UINavigation component = optionItems[i].GetComponent<UINavigation>();
				if (i == 0)
				{
					if (nav != null)
					{
						component.up = nav;
						nav.down = component;
					}
					if (optionItems.Count > 1)
					{
						component.down = optionItems[i + 1].GetComponent<UINavigation>();
					}
				}
				else
				{
					component.up = optionItems[i - 1].GetComponent<UINavigation>();
					if (i + 1 < optionItems.Count)
					{
						component.down = optionItems[i + 1].GetComponent<UINavigation>();
					}
				}
			}
		}

		protected override void OnState(string s)
		{
			base.OnState(s);
			if (s == "over" || s == "focus")
			{
				m_focused = true;
			}
			if (s == "out")
			{
				m_focused = false;
				this.TimerRunOnce(delegate
				{
					if (UINavigation.focus != null && !CheckIfChild(UINavigation.focus.transform, base.transform))
					{
						base.dropdown.Hide();
					}
					base.dropdown.interactable = false;
				}, Time.deltaTime + 0.1f);
			}
			if (s == "change" || (s == "select" && UINavigation.focus != null && CheckIfChild(UINavigation.focus.transform, base.transform)))
			{
				OnClose();
			}
		}

		private bool CheckIfChild(Transform p_child, Transform p_parent)
		{
			if (p_child == null)
			{
				return false;
			}
			if (p_child.parent == p_parent)
			{
				return true;
			}
			if (p_child.parent != null)
			{
				return CheckIfChild(p_child.parent, p_parent);
			}
			return false;
		}

		private void ReturnFocus()
		{
			this.TimerRunOnce(delegate
			{
				UINavigation.Focus(nav);
			}, 0.5f);
		}

		protected override void Update()
		{
			if (m_focused && !(UINavigation.focus == null) && (!(UINavigation.focus != null) || !(UINavigation.focus != nav)))
			{
				base.dropdown.interactable = true;
				base.Update();
				bool p_isdown = (RCI.HasNavigationController && RCI.GetAxisTrigger(RawAxis.RightStickY, isPositiveSign: false)) || RCI.GetButtonDown(ConsoleButtons.ActionBottomRow1) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
				CheckToToggle(p_isdown);
			}
		}

		private void CheckToToggle(bool p_isdown)
		{
			if (p_isdown && !(base.dropdown.transform.Find("Dropdown List") != null))
			{
				base.dropdown.Show();
				this.TimerRunOnce(delegate
				{
					OnOpen();
				}, 0.2f);
			}
		}
	}
}
