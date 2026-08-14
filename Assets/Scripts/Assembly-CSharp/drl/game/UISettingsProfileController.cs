using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISettingsProfileController : Controller<DRLApp>
	{
		public UISettingsProfileView view => AssertLocal<UISettingsProfileView>("view");

		public StateModel model => base.app.model.storage.state;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "ui.screen.return@click")
			{
				UIScreen current = base.app.view.ui.screens.current;
				if ((bool)current && !(current.name != "progression-manual-screen"))
				{
					base.app.view.ui.screens.Return();
				}
			}
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
					CampaignRegisterInfo registerInfo = base.app.model.storage.state.player.results.campaign.GetRegisterInfo(data);
					view.Set(registerInfo);
					view.SetFromProfile(model.player.profile);
					SetProfileStats();
					if (view.showMandatoryFields)
					{
						OpenTournamentForm();
					}
					RefreshVisibilityOfItems();
					CheckMandatories();
					view.RefreshProgressionWeekRank();
					view.RefreshProgression();
					if (base.app.online)
					{
						view.achievementsButton.SetActive(value: true);
					}
					else
					{
						view.achievementsButton.SetActive(value: false);
					}
				}
				break;
			case "ui.screen@close":
				view.StopRankTimeRefresh();
				break;
			case "settings.profile.color@click":
			{
				UINavigation.focus = view.colorsNav;
				view.scroll.enabled = true;
				UIElementView uIElementView = p_target as UIElementView;
				int num = uIElementView.transform.GetSiblingIndex() - 1;
				Color color = DRLColor.profileColors[num + 1];
				model.player.profile.color = color;
				view.cardColor = model.player.profile.color;
				Notify("settings.profile.color@changed", color);
				view.SelectColor(uIElementView, view.colorSwatches, view.colorOutlines, ref view.m_profileColorSelected);
				break;
			}
			case "settings.profile.color@focus":
			{
				UIElementView p_target2 = p_target as UIElementView;
				view.SetColorFocus(p_target2, view.colorSwatches, view.colorOutlines, ref view.m_profileColorSelected);
				break;
			}
			case "settings.profile.color@unfocus":
				view.lastUnfocusedColor = p_target as UIElementView;
				view.UnfocusColor(view.lastUnfocusedColor, view.colorSwatches, view.colorOutlines, ref view.m_profileColorSelected);
				break;
			case "settings.profile.color-picker@click":
				UINavigation.focus = view.colorSwatches[0].GetComponent<UINavigation>();
				view.scroll.enabled = false;
				break;
			case "settings.profile.color-picker@focus":
				if (view.scroll.mode == NavigationModeType.Controller)
				{
					view.SetColorPickerFocus();
					break;
				}
				view.ClearColorPickerFocus();
				view.SetColorFocus(view.lastUnfocusedColor, view.colorSwatches, view.colorOutlines, ref view.m_profileColorSelected);
				break;
			case "settings.profile.color-picker@unfocus":
				if (view.scroll.mode == NavigationModeType.Mouse)
				{
					view.UnfocusColor(view.lastUnfocusedColor, view.colorSwatches, view.colorOutlines, ref view.m_profileColorSelected);
				}
				else
				{
					view.ClearColorPickerFocus();
				}
				break;
			case "ui.screen.return@click":
				view.showMandatoryFields = false;
				base.app.view.ui.screens.Return();
				break;
			case "settings.profile.player-data@click":
				view.SetFromProfile(model.player.profile);
				view.backButton.SetActive(value: false);
				view.playerDataButton.SetActive(value: false);
				view.discardButton.SetActive(value: true);
				view.saveButton.SetActive(value: true);
				view.profileData.SetActive(value: true);
				view.profileStats.SetActive(value: false);
				UINavigation.Focus(view.discardButton.GetComponent<UINavigation>());
				break;
			case "settings.profile.progression-manual@click":
				base.app.view.ui.screens.Open("progression-manual-screen");
				break;
			case "settings.profile.achievements@click":
				base.app.view.ui.screens.Open("achievements-screen");
				break;
			case "settings.profile.progression.rank.finish":
				view.RefreshProgressionWeekRank();
				Activity.RunOnce(view.RefreshProgression, 120f);
				break;
			case "settings.profile.discard-data@click":
				view.backButton.SetActive(value: true);
				view.playerDataButton.SetActive(value: true);
				view.discardButton.SetActive(value: false);
				view.saveButton.SetActive(value: false);
				view.profileData.SetActive(value: false);
				view.profileStats.SetActive(value: true);
				UINavigation.Focus(view.playerDataButton.GetComponent<UINavigation>());
				break;
			case "settings.profile.save-data@click":
				SaveProfileData();
				if (view.showMandatoryFields)
				{
					base.app.view.ui.screens.Return();
					view.showMandatoryFields = false;
					break;
				}
				view.backButton.SetActive(value: true);
				view.playerDataButton.SetActive(value: true);
				view.discardButton.SetActive(value: false);
				view.saveButton.SetActive(value: false);
				view.profileData.SetActive(value: false);
				view.profileStats.SetActive(value: true);
				UINavigation.Focus(view.playerDataButton.GetComponent<UINavigation>());
				break;
			case "settings.profile.form.event@click":
				OnFormNotification(p_target, p_is_change: false);
				break;
			case "settings.profile.form.event@change":
				OnFormNotification(p_target, p_is_change: true);
				break;
			}
		}

		private void RefreshVisibilityOfItem(string p_fieldName)
		{
			switch (p_fieldName)
			{
			case "non-fpv-drone-skill":
			{
				int index3 = view.nonFpvDroneSkillField.index;
				bool flag = index3 == 2 || index3 == 3;
				EnableFieldWithFade(view.nonFpvDroneSkillYearsField.gameObject, flag);
				view.nonFpvDroneSkillYearsField.enabled = flag;
				if (!flag)
				{
					view.nonFpvDroneSkillYearsField.index = 5;
					view.nonFpvDroneSkillYearsField.Refresh();
				}
				break;
			}
			case "fpv-drone-skill":
			{
				int index4 = view.fpvDroneSkillField.index;
				int index5 = view.fpvDronePreferenceField.index;
				bool flag2 = index4 == 2 || index4 == 3;
				EnableFieldWithFade(view.fpvDroneSkillYearsField.gameObject, flag2);
				view.fpvDroneSkillYearsField.enabled = flag2;
				EnableFieldWithFade(view.fpvDronePreferenceField.gameObject, flag2);
				view.fpvDronePreferenceField.enabled = flag2;
				if (!flag2)
				{
					view.fpvDroneSkillYearsField.index = 5;
					view.fpvDroneSkillYearsField.Refresh();
					view.fpvDronePreferenceField.index = 3;
					view.fpvDronePreferenceField.Refresh();
				}
				bool p_enable3 = false;
				if ((index4 == 2 || index4 == 3) && (index5 == 1 || index5 == 2))
				{
					p_enable3 = true;
				}
				EnableFieldWithFade(view.multiGPFieldLabel.gameObject, p_enable3);
				EnableFieldWithFade(view.multiGPField.gameObject, p_enable3);
				view.multiGPField.enabled = p_enable3;
				break;
			}
			case "fpv-drone-preference":
			{
				int index = view.fpvDronePreferenceField.index;
				int index2 = view.fpvDroneSkillField.index;
				bool p_enable = index == 1 || index == 2;
				EnableFieldWithFade(view.realLifeCompetitionFieldLabel.gameObject, p_enable);
				EnableFieldWithFade(view.realLifeCompetitionField.gameObject, p_enable);
				view.realLifeCompetitionField.enabled = p_enable;
				bool p_enable2 = false;
				if ((index2 == 2 || index2 == 3) && (index == 1 || index == 2))
				{
					p_enable2 = true;
				}
				EnableFieldWithFade(view.multiGPFieldLabel.gameObject, p_enable2);
				EnableFieldWithFade(view.multiGPField.gameObject, p_enable2);
				view.multiGPField.enabled = p_enable2;
				break;
			}
			}
		}

		private void RefreshVisibilityOfItems()
		{
			bool p_enable = true;
			EnableFieldWithFade(view.usMilitaryFieldLabel.gameObject, p_enable);
			EnableFieldWithFade(view.usMilitaryField.gameObject, p_enable);
			view.usMilitaryField.enabled = p_enable;
			EnableFieldWithFade(view.amaMemberFieldLabel.gameObject, p_enable);
			EnableFieldWithFade(view.amaMemberField.gameObject, p_enable);
			view.amaMemberField.enabled = p_enable;
			RefreshVisibilityOfItem("non-fpv-drone-skill");
			RefreshVisibilityOfItem("fpv-drone-skill");
			RefreshVisibilityOfItem("fpv-drone-preference");
		}

		private void CheckMandatories()
		{
			if (view.showMandatoryFields)
			{
				view.ToggleMandatoryField(0, !IsValidData(view.nameField.field.text));
				view.ToggleMandatoryField(1, !IsValidData(view.emailField.field.text));
				view.ToggleMandatoryField(2, !IsValidData(view.ageInputField.inputText.text));
				view.ToggleMandatoryField(3, !IsValidData(view.countryField.text));
				view.ToggleMandatoryField(4, !IsValidData(view.genderField.value));
				view.ToggleMandatoryField(5, !IsValidData(view.drlWatchField.value));
				view.ToggleMandatoryField(6, !IsValidData(view.nonFpvDroneSkillField.value));
				view.ToggleMandatoryField(7, !IsValidData(view.nonFpvDroneSkillYearsField.value));
				view.ToggleMandatoryField(8, !IsValidData(view.fpvDroneSkillField.value));
				view.ToggleMandatoryField(9, !IsValidData(view.fpvDroneSkillYearsField.value));
				view.ToggleMandatoryField(10, !IsValidData(view.fpvDronePreferenceField.value));
			}
		}

		private void CheckMandatory(string p_fieldName)
		{
			if (p_fieldName != null)
			{
				switch (p_fieldName)
				{
				case "profile-name":
					view.ToggleMandatoryField(0, !IsValidData(view.nameField.field.text));
					break;
				case "profile-email":
					view.ToggleMandatoryField(1, !IsValidData(view.emailField.field.text));
					break;
				case "profile-age-field":
					view.ToggleMandatoryField(2, !IsValidData(view.ageInputField.inputText.text));
					break;
				case "profile-country-input":
					view.ToggleMandatoryField(3, !IsValidData(view.countryField.text));
					break;
				case "profile-gender":
					view.ToggleMandatoryField(4, !IsValidData(view.genderField.value));
					break;
				case "drl-watch":
					view.ToggleMandatoryField(5, !IsValidData(view.drlWatchField.value));
					break;
				case "non-fpv-drone-skill":
					view.ToggleMandatoryField(6, !IsValidData(view.nonFpvDroneSkillField.value));
					break;
				case "non-fpv-drone-years":
					view.ToggleMandatoryField(7, !IsValidData(view.nonFpvDroneSkillYearsField.value));
					break;
				case "fpv-drone-skill":
					view.ToggleMandatoryField(8, !IsValidData(view.fpvDroneSkillField.value));
					break;
				case "fpv-drone-years":
					view.ToggleMandatoryField(9, !IsValidData(view.fpvDroneSkillYearsField.value));
					break;
				case "fpv-drone-preference":
					view.ToggleMandatoryField(10, !IsValidData(view.fpvDronePreferenceField.value));
					break;
				}
			}
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

		private void SetProfileStats()
		{
			view.backButton.SetActive(value: true);
			view.playerDataButton.SetActive(value: true);
			view.discardButton.SetActive(value: false);
			view.saveButton.SetActive(value: false);
			view.profileData.SetActive(value: false);
			view.profileStats.SetActive(value: true);
			UINavigation.Focus(view.playerDataButton.GetComponent<UINavigation>());
		}

		private void OpenTournamentForm()
		{
			view.SetFromProfile(model.player.profile);
			view.backButton.SetActive(value: true);
			view.playerDataButton.SetActive(value: false);
			view.discardButton.SetActive(value: false);
			view.saveButton.SetActive(value: true);
			view.profileData.SetActive(value: true);
			view.profileStats.SetActive(value: false);
			UINavigation.Focus(view.discardButton.GetComponent<UINavigation>());
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

		private void SaveProfileData()
		{
			ProfileStateModel profile = model.player.profile;
			int num = 0;
			float num2 = 7f;
			if (!string.IsNullOrEmpty(view.nameField.inputText.text))
			{
				profile.fullName = view.nameField.inputText.text;
				num++;
			}
			if (!string.IsNullOrEmpty(view.ageInputField.inputText.text))
			{
				int result = -1;
				int.TryParse(view.ageInputField.inputText.text, out result);
				int age = Mathf.Max(10, result);
				profile.age = age;
				num++;
			}
			if (!string.IsNullOrEmpty(view.emailField.field.text))
			{
				profile.email = view.emailField.inputText.text;
				num++;
			}
			if (!string.IsNullOrEmpty(view.countryField.field.text))
			{
				profile.country = view.countryField.field.text;
				num++;
			}
			profile.gender = view.genderField.value;
			int index = view.nonFpvDroneSkillField.index;
			bool flag = index == 2 || index == 3;
			if (flag)
			{
				num2 += 1f;
			}
			index = view.fpvDroneSkillField.index;
			bool flag2 = index == 2 || index == 3;
			if (flag2)
			{
				num2 += 2f;
			}
			if (IsValidData(view.nonFpvDroneSkillField.value))
			{
				profile.experienceNonFPV = view.nonFpvDroneSkillField.value;
				num++;
			}
			if (IsValidData(view.nonFpvDroneSkillYearsField.value))
			{
				profile.experienceNonFPVYears = view.nonFpvDroneSkillYearsField.value;
				if (flag)
				{
					num++;
				}
			}
			if (IsValidData(view.fpvDroneSkillField.value))
			{
				profile.experienceFPV = view.fpvDroneSkillField.value;
				num++;
			}
			if (IsValidData(view.fpvDroneSkillYearsField.value))
			{
				profile.experienceFPVYears = view.fpvDroneSkillYearsField.value;
				if (flag2)
				{
					num++;
				}
			}
			if (IsValidData(view.drlWatchField.value))
			{
				profile.watchDRL = view.drlWatchField.value;
				num++;
			}
			if (IsValidData(view.fpvDronePreferenceField.value))
			{
				profile.experiencePreferenceFPV = view.fpvDronePreferenceField.value;
				if (flag2)
				{
					num++;
				}
			}
			if (IsValidData(view.realLifeCompetitionField.value))
			{
				profile.experienceRealLifeRacing = view.realLifeCompetitionField.value;
			}
			if (IsValidData(view.droneBuiltOwnField.value))
			{
				profile.experienceBuiltOwnDrone = view.droneBuiltOwnField.value;
			}
			if (IsValidData(view.multiGPField.value))
			{
				profile.affiliationMultiGP = view.multiGPField.value;
			}
			if (IsValidData(view.usMilitaryField.value))
			{
				profile.affiliationMilitary = view.usMilitaryField.value;
			}
			if (IsValidData(view.amaMemberField.value))
			{
				profile.affiliationAMA = view.amaMemberField.value;
			}
			float dataCompletion = (float)num / num2;
			profile.dataCompletion = dataCompletion;
		}

		private bool IsValidData(string p_str)
		{
			if (string.IsNullOrEmpty(p_str))
			{
				return false;
			}
			if (p_str.ToLower().Contains("<") && p_str.ToLower().Contains(">"))
			{
				return false;
			}
			return true;
		}

		protected void OnFormNotification(Object p_target, bool p_is_change)
		{
			string p_fieldName = p_target.name;
			if (view.showMandatoryFields)
			{
				CheckMandatory(p_fieldName);
			}
			RefreshVisibilityOfItem(p_fieldName);
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
			if (idx >= 0 && idx + 1 < view.navigationElements.Count)
			{
				this.TimerRunOnce(delegate
				{
					UINavigation.focus = view.navigationElements[idx + 1];
				}, 0.3f);
			}
		}
	}
}
