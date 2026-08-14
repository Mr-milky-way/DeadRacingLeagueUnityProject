using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLInputFieldView : InputFieldView
	{
		[SerializeField]
		private UINavigation m_nav;

		private Text m_inputText;

		private UINavigation m_input_text_nav;

		public bool unfocusOnSubmit = true;

		public bool unfocusOnSubmitEmpty;

		public bool unfocusOnArrowAndEmpty;

		public bool allcaps;

		public bool form;

		public bool allowValidation = true;

		internal bool m_input_focus;

		private bool m_accept_input;

		private bool m_remove_tabs;

		internal bool m_lock_focus;

		private string m_input_text;

		public UINavigation nav
		{
			get
			{
				if (!m_nav)
				{
					return m_nav = GetComponent<UINavigation>();
				}
				return m_nav;
			}
		}

		public Text inputText
		{
			get
			{
				if (!base.field)
				{
					return null;
				}
				if (!m_inputText)
				{
					return m_inputText = base.field.textComponent;
				}
				return m_inputText;
			}
		}

		public UINavigation inputTextNav
		{
			get
			{
				if (!inputText)
				{
					return null;
				}
				if (!m_input_text_nav)
				{
					return m_input_text_nav = inputText.GetComponent<UINavigation>();
				}
				return m_input_text_nav;
			}
		}

		public bool IsEditing => m_input_focus;

		protected override void Awake()
		{
			base.Awake();
			if ((bool)base.field)
			{
				base.field.interactable = false;
			}
		}

		public override void OnFocus()
		{
			base.OnFocus();
			_ = base.field.interactable;
		}

		public override void OnUnfocus()
		{
			OnDeselect(null);
			base.OnUnfocus();
		}

		protected override void OnChange(string v)
		{
			OnChangeApply(v);
		}

		private void OnChangeApply(string v)
		{
			if (allcaps)
			{
				v = v.ToUpper();
				base.field.text = v;
			}
			base.OnChange(v);
		}

		protected override void OnChangeEnd(string v)
		{
			if (m_remove_tabs)
			{
				char c = '\t';
				base.field.text = base.field.text.Replace("\t", "").Replace(c.ToString(), "");
				v = v.Replace("\t", "").Replace(c.ToString(), "");
			}
			OnChangeEndApply(v);
		}

		private void OnChangeEndApply(string v)
		{
			base.OnChangeEnd(v);
			if (m_accept_input)
			{
				Notify(notification + "@submit");
			}
			if (UINavigation.focus == null || UINavigation.focus == nav)
			{
				ReturnFocus();
			}
		}

		protected virtual void OnFieldFocus()
		{
			DRLUINavigationSystem.IsTyping = true;
			Notify(notification + "@start-edit");
		}

		public override void OnSelect(BaseEventData p_event_data)
		{
			base.OnSelect(p_event_data);
			DRLUINavigationSystem.IsTyping = true;
		}

		public override void OnDeselect(BaseEventData p_event_data)
		{
			base.OnDeselect(p_event_data);
			DRLUINavigationSystem.IsTyping = false;
			base.field.DeactivateInputField();
		}

		public void ClearInputText()
		{
			m_input_text = "";
			base.field.text = "";
			inputText.text = "";
		}

		protected void ValidateFocus()
		{
			m_input_focus = false;
			if ((bool)base.field)
			{
				DRLUINavigationSystem.IsTyping = false;
				base.field.interactable = false;
				base.field.DeactivateInputField();
			}
		}

		protected void LateUpdate()
		{
			if (form && Input.GetKeyDown(KeyCode.Tab))
			{
				this.TimerRunOnce(delegate
				{
					base.field.DeactivateInputField();
				}, 0.2f);
			}
			if (m_input_focus)
			{
				_ = UINavigation.focus == inputTextNav;
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				bool num = base.field.lineType == InputField.LineType.MultiLineNewline;
				bool flag4 = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow);
				bool buttonUp = RCI.GetButtonUp(ConsoleButtons.ActionBottomRow1);
				bool flag5 = Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return);
				bool flag6 = string.IsNullOrEmpty(base.field.text);
				bool flag7 = unfocusOnSubmitEmpty && flag6;
				flag3 = !num || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand);
				if (Input.GetKeyDown(KeyCode.Tab))
				{
					flag2 = true;
					m_remove_tabs = true;
				}
				if (Input.GetKeyDown(KeyCode.Escape))
				{
					flag2 = true;
					ClearInputText();
				}
				if (flag5)
				{
					flag2 = unfocusOnSubmit || flag7;
					flag = flag3;
				}
				if (buttonUp)
				{
					flag2 = true;
				}
				if (flag4)
				{
					flag2 = flag6 && unfocusOnArrowAndEmpty;
				}
				if (RCI.GetButton(ConsoleButtons.ActionBottomRow2))
				{
					flag2 = true;
					ClearInputText();
					flag = false;
				}
				if (flag2)
				{
					m_lock_focus = flag;
					m_accept_input = flag;
					ReturnFocus();
				}
			}
		}

		internal void ReturnFocus()
		{
			if (m_input_focus)
			{
				m_input_focus = false;
				Activity.RunOnce(base.field.DeactivateInputField, 2f / 15f);
				OnDeselect(null);
				if (UINavigation.focus == null || UINavigation.focus != nav)
				{
					UINavigation.focus = nav;
				}
			}
		}

		protected override void OnState(string p_state)
		{
			base.OnState(p_state);
			if (base.isActiveAndEnabled && base.enabled && p_state != null && p_state == "lclick")
			{
				if (m_lock_focus)
				{
					m_lock_focus = false;
				}
				else if ((bool)base.field && (bool)inputTextNav && !(UINavigation.focus == inputTextNav))
				{
					UINavigation.focus = inputTextNav;
					m_input_focus = true;
					m_accept_input = false;
					m_remove_tabs = false;
					base.field.interactable = true;
					base.field.ActivateInputField();
					base.field.selectionAnchorPosition = 0;
					base.field.selectionFocusPosition = base.field.text.Length;
					base.field.Select();
					OnFieldFocus();
				}
			}
		}

		public void Activate()
		{
			OnState("lclick");
		}
	}
}
