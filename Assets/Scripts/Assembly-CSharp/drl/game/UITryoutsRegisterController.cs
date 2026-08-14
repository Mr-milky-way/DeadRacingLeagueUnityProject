using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITryoutsRegisterController : Controller<DRLApp>
	{
		public bool isLoadingRegister;

		public UITryoutsRegisterView view => AssertLocal<UITryoutsRegisterView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					DRLCampaign data = view.data;
					view.Set(data);
					RefreshVisibilityOfItems();
					CampaignRegisterInfo registerInfo = base.app.model.storage.state.player.results.campaign.GetRegisterInfo(data);
					view.Set(registerInfo);
					view.ShowSendingIndicator(p_show: false);
					CheckForErrors(view.Get());
					view.quebecDisclaimerLabel.SetActive(value: false);
				}
				break;
			case "campaign.register.form.event@click":
				OnFormNotification(p_target, p_is_change: false);
				break;
			case "campaign.register.form.event@change":
				OnFormNotification(p_target, p_is_change: true);
				break;
			case "campaign.register.form.event@open":
				if (p_target.name == "country-dropdown")
				{
					view.scroll.scrollMouseWheel = false;
				}
				break;
			case "campaign.register.form.event@close":
				if (p_target.name == "country-dropdown")
				{
					view.scroll.scrollMouseWheel = true;
				}
				break;
			case "campaign.register.form.event@submit":
			{
				string text2 = p_target.name;
				if (text2 != null && text2 == "profile-age-field")
				{
					string text3 = (p_target as DRLInputFieldView).field.text;
					int result = 0;
					if (int.TryParse(text3, out result))
					{
						result = Mathf.Clamp(result, 13, 100);
					}
					view.ageInputField.field.enabled = false;
					view.ageInputField.field.text = result.ToString();
					view.ageInputField.field.enabled = true;
				}
				break;
			}
			case "storage.state@write":
				if (isLoadingRegister)
				{
					isLoadingRegister = false;
					if (base.app.model.storage.state.player.results.campaign.GetRegisterInfo(view.data) == null)
					{
						Debug.LogWarning("UITryoutsRegisterController> State still not has register info.");
						break;
					}
					Notify("analytics.tryouts.registered");
					base.app.view.audio.PlayUIGenericSuccess();
					base.app.view.ui.screens.Return();
				}
				break;
			case "ui.screen.nav-right@click":
			{
				string text = (p_target as UIElementView).name;
				if (text != null && text == "submit" && !isLoadingRegister)
				{
					CampaignRegisterInfo campaignRegisterInfo = view.Get();
					UINavigation uINavigation = CheckForErrors(campaignRegisterInfo, useAnimAndDelay: true);
					if (!(uINavigation != null))
					{
						isLoadingRegister = true;
						view.ShowSendingIndicator(p_show: true);
						base.app.model.storage.state.player.results.campaign.SetRegisterInfo(view.data, campaignRegisterInfo);
					}
					else
					{
						UINavigation.focus = uINavigation;
						base.app.view.audio.PlayUIGenericError();
					}
				}
				break;
			}
			case "ui.screen.return@click":
				if (!isLoadingRegister)
				{
					base.app.view.ui.screens.Return();
				}
				break;
			}
		}

		private UINavigation CheckForErrors(CampaignRegisterInfo register, bool useAnimAndDelay = false)
		{
			view.ClearErrors();
			List<string> list = ValidateRegister(register);
			_ = list.Count;
			UINavigation uINavigation = null;
			float p_time = (useAnimAndDelay ? 0.3f : 0f);
			for (int i = 0; i < list.Count; i++)
			{
				float p_delay = (useAnimAndDelay ? (0.5f + (float)i * 0.2f) : 0f);
				switch (list[i])
				{
				case "profile-name":
				{
					UIFieldErrorIndicator component3 = view.nameField.GetComponent<UIFieldErrorIndicator>();
					if ((bool)component3 && !component3.IsOn())
					{
						component3.Show(p_delay, p_time);
					}
					if (uINavigation == null)
					{
						uINavigation = view.nameField.GetComponent<UINavigation>();
					}
					break;
				}
				case "profile-email":
				{
					UIFieldErrorIndicator component5 = view.emailField.GetComponent<UIFieldErrorIndicator>();
					if ((bool)component5 && !component5.IsOn())
					{
						component5.Show(p_delay, p_time);
					}
					if (uINavigation == null)
					{
						uINavigation = view.emailField.GetComponent<UINavigation>();
					}
					break;
				}
				case "profile-age":
				{
					UIFieldErrorIndicator component2 = view.ageInputField.GetComponent<UIFieldErrorIndicator>();
					if ((bool)component2 && !component2.IsOn())
					{
						component2.Show(p_delay, p_time);
					}
					if (uINavigation == null)
					{
						uINavigation = view.ageInputField.GetComponent<UINavigation>();
					}
					break;
				}
				case "profile-area":
				{
					UIFieldErrorIndicator component4 = view.areaCodeInputField.GetComponent<UIFieldErrorIndicator>();
					if ((bool)component4 && !component4.IsOn())
					{
						component4.Show(p_delay, p_time);
					}
					if (uINavigation == null)
					{
						uINavigation = view.areaCodeInputField.GetComponent<UINavigation>();
					}
					break;
				}
				case "profile-phone":
				{
					UIFieldErrorIndicator component = view.phoneNumberInputField.GetComponent<UIFieldErrorIndicator>();
					if ((bool)component && !component.IsOn())
					{
						component.Show(p_delay, p_time);
					}
					if (uINavigation == null)
					{
						uINavigation = view.phoneNumberInputField.GetComponent<UINavigation>();
					}
					break;
				}
				}
			}
			return uINavigation;
		}

		protected List<string> ValidateRegister(CampaignRegisterInfo p_data)
		{
			CampaignRegisterInfo campaignRegisterInfo = ((p_data == null) ? new CampaignRegisterInfo() : p_data);
			List<string> list = new List<string>();
			string pattern = "[a-z0-9]+[\\+_a-z0-9\\.-]*[a-z0-9]+@[a-z0-9-]+(\\.[a-z0-9-]+)*(\\.[a-z]{2,4})";
			object[] array = new object[10]
			{
				"profile-name",
				!string.IsNullOrEmpty(campaignRegisterInfo.name),
				"profile-email",
				Regex.Match(campaignRegisterInfo.email, pattern).Success,
				"profile-age",
				campaignRegisterInfo.age >= 16,
				"profile-area",
				!string.IsNullOrEmpty(campaignRegisterInfo.area),
				"profile-phone",
				!string.IsNullOrEmpty(campaignRegisterInfo.phone) && campaignRegisterInfo.phone.Length >= 6
			};
			for (int i = 1; i < array.Length; i += 2)
			{
				string item = (string)array[i - 1];
				if (!(bool)array[i])
				{
					list.Add(item);
				}
			}
			return list;
		}

		private void RefreshVisibilityOfItems()
		{
			string text = view.americanCitizenField.value.ToLower();
			bool p_enable = text.Contains("yes");
			EnableFieldWithFade(view.usMilitaryFieldLabel.gameObject, p_enable);
			EnableFieldWithFade(view.usMilitaryField.gameObject, p_enable);
			view.usMilitaryField.enabled = p_enable;
			EnableFieldWithFade(view.amaMemberFieldLabel.gameObject, p_enable: true);
			EnableFieldWithFade(view.amaMemberField.gameObject, p_enable: true);
			view.amaMemberField.enabled = true;
			text = view.nonFpvDroneSkillField.value.ToLower();
			bool p_enable2 = text.Contains("basic") || text.Contains("advanced");
			EnableFieldWithFade(view.nonFpvDroneSkillYearsField.gameObject, p_enable2);
			view.nonFpvDroneSkillYearsField.enabled = p_enable2;
			text = view.fpvDroneSkillField.value.ToLower();
			p_enable = text.Contains("basic") || text.Contains("advanced");
			EnableFieldWithFade(view.fpvDroneSkillYearsField.gameObject, p_enable);
			view.fpvDroneSkillYearsField.enabled = p_enable;
			EnableFieldWithFade(view.fpvDronePreferenceField.gameObject, p_enable);
			view.fpvDronePreferenceField.enabled = p_enable;
			text = view.fpvDronePreferenceField.value.ToLower();
			p_enable = text.Contains("racing") || text.Contains("both");
			EnableFieldWithFade(view.realLifeCompetitionFieldLabel.gameObject, p_enable);
			EnableFieldWithFade(view.realLifeCompetitionField.gameObject, p_enable);
			view.realLifeCompetitionField.enabled = p_enable;
			string text2 = view.fpvDroneSkillField.value.ToLower();
			string text3 = view.fpvDronePreferenceField.value.ToLower();
			bool p_enable3 = false;
			if ((text2.Contains("basic") || text2.Contains("advanced")) && (text3.Contains("racing") || text3.Contains("both")))
			{
				p_enable3 = true;
			}
			EnableFieldWithFade(view.multiGPFieldLabel.gameObject, p_enable3);
			EnableFieldWithFade(view.multiGPField.gameObject, p_enable3);
			view.multiGPField.enabled = p_enable3;
		}

		private void EnableFieldWithFade(GameObject p_field, bool p_enable)
		{
			if ((bool)p_field)
			{
				FadeComponent component = p_field.GetComponent<FadeComponent>();
				if ((bool)component)
				{
					component.Fade(p_enable ? 1f : 0.2f, 0f);
				}
			}
		}

		private bool IsFieldEnabled(GameObject p_field)
		{
			if (!p_field)
			{
				return false;
			}
			FadeComponent component = p_field.GetComponent<FadeComponent>();
			if (!component)
			{
				return true;
			}
			return component.alpha > 0.9f;
		}

		protected void OnFormNotification(Object p_target, bool p_is_change)
		{
			bool flag = p_is_change;
			string text = p_target.name;
			if (text == null)
			{
				return;
			}
			switch (text)
			{
			case "non-fpv-drone-skill":
			case "fpv-drone-skill":
			case "fpv-drone-preference":
			case "us-citizen":
				RefreshVisibilityOfItems();
				break;
			case "profile-age-field":
			case "profile-name":
			case "profile-email":
			case "profile-age":
			case "profile-phone-area":
			case "profile-phone-number":
				if (flag)
				{
					CheckForErrors(view.Get());
				}
				break;
			case "country-dropdown":
				if (flag)
				{
					CheckForErrors(view.Get());
					DRLDropdownView dRLDropdownView = p_target as DRLDropdownView;
					if (!(dRLDropdownView == null))
					{
						view.quebecDisclaimerLabel.SetActive(dRLDropdownView.Value().text.ToLower() == "canada");
					}
				}
				break;
			}
		}

		private void Update()
		{
			if (!Input.GetKeyDown(KeyCode.Tab))
			{
				return;
			}
			int idx = -1;
			for (int i = 0; i < view.navigationElements.Count; i++)
			{
				if (UINavigation.focus == view.navigationElements[i] || (bool)Hierarchy.FindReverse<UINavigation>(UINavigation.focus.transform))
				{
					idx = i;
					break;
				}
			}
			if (idx < 0 || idx + 1 >= view.navigationElements.Count)
			{
				return;
			}
			this.TimerRunOnce(delegate
			{
				UINavigation.focus = view.navigationElements[idx + 1];
				DRLInputFieldView component = view.navigationElements[idx + 1].GetComponent<DRLInputFieldView>();
				if (component != null)
				{
					component.field.Select();
					component.field.ActivateInputField();
				}
			}, 0.3f);
		}
	}
}
