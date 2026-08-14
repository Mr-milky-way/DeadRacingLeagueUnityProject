using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	[RequireComponent(typeof(FadeComponent))]
	public class DialogComponent : View<DRLApp>
	{
		public FadeComponent fade;

		public GraphicRaycaster raycaster;

		[Header("Dialog Layout Variations:")]
		public Color defaultColor;

		public Color warningColor;

		public Color errorColor;

		public List<Image> colorElements;

		[Header("Dialog options and info:")]
		public Text title;

		public Text infoText;

		public RawImage infoIcon;

		public RectTransform dialogContainer;

		public GameObject infoIconContainer;

		public GameObject optionsContainer;

		public List<GameObject> options;

		public List<Text> optionFields;

		public List<HorizontalLayoutGroup> optionLayouts;

		public List<DRLGamepadHotkey> optionHotkeys;

		public Texture2D warningIcon;

		private int m_chosenOption = -1;

		private string m_activeId = "";

		private UINavigation m_lastNavigation;

		[Header("Dialog templates:")]
		public List<DialogTemplate> templates;

		private Activity m_dialog_poll;

		private Action<string, int> m_dialog_callback;

		public bool isVisible => fade.alpha > 0.2f;

		[ContextMenu("Debug Disconnect")]
		private void DebugDisconnect()
		{
			Open(DialogTemplateType.ServerDisconnect, "server-disconnect");
		}

		[ContextMenu("Debug Dialog Resize")]
		private void DebugDialogResize()
		{
			Open(DialogType.Info, "TEST SIZE", "Test message.", new string[2] { "CHECKOUT", "CANCEL" }, null, "", null, 16, 1000f);
		}

		public void Init()
		{
			if (!isVisible)
			{
				return;
			}
			StartDialogPoll();
			foreach (DRLGamepadHotkey optionHotkey in optionHotkeys)
			{
				if (optionHotkey.gameObject.activeInHierarchy)
				{
					optionHotkey.Init();
				}
			}
		}

		public void Open(DialogType p_type, string p_title, string p_message, string[] p_options = null, Texture2D p_icon = null, string p_id = "", Action<string, int> p_callback = null, int p_fontSize = 16, float p_dialogWidth = 500f)
		{
			if (string.IsNullOrEmpty(p_id))
			{
				p_id = "dialog-" + (Time.time * 1000f).ToString("0");
			}
			fade.FadeIn(0.1f);
			ResetLayout();
			SetLayout(p_type, p_title, p_message, p_options, p_icon, p_fontSize, p_dialogWidth);
			m_lastNavigation = UINavigation.focus;
			m_activeId = p_id;
			if (p_options != null && p_options.Length != 0)
			{
				optionsContainer.SetActive(value: true);
				UINavigation.Focus(options[0].transform);
				raycaster.enabled = true;
				m_dialog_callback = p_callback;
				StartDialogPoll();
			}
			else
			{
				p_callback?.Invoke(p_id, -1);
			}
		}

		protected void StartDialogPoll()
		{
			if (m_dialog_poll != null)
			{
				m_dialog_poll.Stop();
			}
			m_dialog_poll = null;
			m_dialog_poll = ((Component)this).TimerRun((Func<bool>)delegate
			{
				if (m_chosenOption == -1)
				{
					return true;
				}
				if (m_dialog_callback != null)
				{
					m_dialog_callback(m_activeId, m_chosenOption);
				}
				m_dialog_callback = null;
				Close();
				return false;
			}, 0f);
		}

		public void Open(DialogTemplateType p_template, string p_id = "", Action<string, int> p_callback = null)
		{
			DialogTemplate dialogTemplate = null;
			if (string.IsNullOrEmpty(p_id))
			{
				p_id = "dialog-" + (Time.time * 1000f).ToString("0");
			}
			foreach (DialogTemplate template in templates)
			{
				if (template.template == p_template)
				{
					dialogTemplate = template;
					break;
				}
			}
			if (dialogTemplate == null && p_callback != null)
			{
				p_callback(p_id, -1);
				UnityEngine.Debug.LogWarning("Dialog > No template found for " + p_template);
			}
			else
			{
				Texture2D icon = dialogTemplate.icon;
				Open(dialogTemplate.type, dialogTemplate.title, dialogTemplate.message, dialogTemplate.options, icon, p_id, p_callback);
			}
		}

		public void Close()
		{
			bool flag = false;
			fade.FadeOut(0.1f);
			this.TimerRunOnce(delegate
			{
				ResetLayout();
				if (m_lastNavigation != null)
				{
					UINavigation.Focus(m_lastNavigation);
				}
			}, flag ? 0.45f : 0f);
			m_chosenOption = -1;
			m_activeId = "";
			raycaster.enabled = false;
		}

		public void Close(string p_id)
		{
			if (!string.IsNullOrEmpty(p_id) && !(m_activeId != p_id))
			{
				Close();
			}
		}

		private void SetLayout(DialogType p_type, string p_title, string p_message, string[] p_options, Texture2D p_icon, int p_fontSize, float p_dialogWidth)
		{
			Color color = defaultColor;
			switch (p_type)
			{
			case DialogType.Warning:
				color = warningColor;
				break;
			case DialogType.Error:
				color = errorColor;
				break;
			}
			foreach (Image colorElement in colorElements)
			{
				colorElement.color = color;
			}
			_ = new StackTrace().GetFrame(1).GetMethod().Name;
			title.fontSize = p_fontSize + 2;
			infoText.fontSize = p_fontSize;
			dialogContainer.sizeDelta = new Vector2(p_dialogWidth, dialogContainer.sizeDelta.y);
			if (p_title.Contains("@"))
			{
				string[] array = p_title.Split('@');
				if (array[0] != string.Empty && array[1] != string.Empty)
				{
					title.text = base.app.model.storage.locale.Get(array[0], array[1]).ToUpper();
				}
				else
				{
					title.text = p_title.ToUpper();
				}
			}
			else
			{
				title.text = p_title;
			}
			if (p_message.Contains("@"))
			{
				string[] array2 = p_message.Split('@');
				if (array2[0] != string.Empty && array2[1] != string.Empty)
				{
					infoText.text = base.app.model.storage.locale.Get(array2[0], array2[1]).ToUpper();
				}
				else
				{
					infoText.text = p_title.ToUpper();
				}
			}
			else
			{
				infoText.text = p_message;
			}
			for (int i = 0; i < options.Count; i++)
			{
				if (p_options == null || i >= p_options.Length || string.IsNullOrEmpty(p_options[i]))
				{
					optionFields[i].text = "";
					options[i].SetActive(value: false);
					continue;
				}
				if (p_options[i].Contains("@"))
				{
					string[] array3 = p_options[i].Split('@');
					if (array3[0] != string.Empty && array3[1] != string.Empty)
					{
						optionFields[i].text = base.app.model.storage.locale.Get(array3[0], array3[1]).ToUpper();
					}
					else
					{
						optionFields[i].text = p_options[i].ToUpper();
					}
				}
				else
				{
					optionFields[i].text = p_options[i];
				}
				options[i].SetActive(value: true);
				optionLayouts[i].enabled = false;
				StartCoroutine(RefreshOptionLayout(i));
			}
			infoIconContainer.SetActive(p_icon != null);
			if (p_icon != null)
			{
				infoIcon.texture = p_icon;
			}
		}

		private void ResetLayout()
		{
			foreach (Image colorElement in colorElements)
			{
				colorElement.color = defaultColor;
			}
			title.text = "";
			infoText.text = "";
			infoText.fontSize = 16;
			title.fontSize = 18;
			dialogContainer.sizeDelta = new Vector2(500f, dialogContainer.sizeDelta.y);
			infoIcon.texture = null;
			infoIconContainer.SetActive(value: false);
			for (int i = 0; i < options.Count; i++)
			{
				optionFields[i].text = "";
				options[i].SetActive(value: false);
			}
			optionsContainer.SetActive(value: false);
		}

		private IEnumerator RefreshOptionLayout(int i)
		{
			yield return new WaitForSeconds(0.2f);
			optionLayouts[i].enabled = true;
			RefreshHotkeys();
		}

		public void SetDialogOption(int p_option)
		{
			UnityEngine.Debug.Log($"DialogComponent> SetDialogOption / option[{p_option}]");
			m_chosenOption = p_option;
			StartDialogPoll();
		}

		public void RefreshHotkeys()
		{
			bool active = RCI.NavigationIsGamepad();
			foreach (DRLGamepadHotkey optionHotkey in optionHotkeys)
			{
				optionHotkey.gameObject.SetActive(active);
			}
		}

		public void OnPersistency()
		{
			if ((bool)base.app.view.ui)
			{
				base.app.view.ui.dialog = this;
			}
		}
	}
}
