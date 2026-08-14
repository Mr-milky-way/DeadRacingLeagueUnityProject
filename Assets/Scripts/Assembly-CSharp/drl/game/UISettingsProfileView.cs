using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISettingsProfileView : UIScreenView
	{
		public Image photoCardColorField;

		public RawImage photoCardImageField;

		public Image photoBadgeRank;

		public List<FadeComponent> colorSwatches;

		public List<FadeComponent> colorOutlines;

		public FadeComponent colorPickerOutline;

		public UINavigation colorsNav;

		public UINavigation playerDataNav;

		public UINavigation xpManualDataNav;

		public Text profileNameField;

		public GameObject[] mandatoryRedOutlines;

		public GameObject[] mandatoryGrayOutlines;

		public Text flightTimeField;

		public DRLInputFieldView nameField;

		public DRLInputFieldView emailField;

		public DRLInputFieldView ageInputField;

		public DRLTextAssetStepperView genderField;

		public DRLInputFieldView countryField;

		public DRLTextAssetStepperView nonFpvDroneSkillField;

		public DRLTextAssetStepperView nonFpvDroneSkillYearsField;

		public DRLTextAssetStepperView fpvDroneSkillField;

		public DRLTextAssetStepperView fpvDroneSkillYearsField;

		public DRLTextAssetStepperView fpvDronePreferenceField;

		public RectTransform realLifeCompetitionFieldLabel;

		public DRLTextAssetStepperView realLifeCompetitionField;

		public DRLTextAssetStepperView droneBuiltOwnField;

		public DRLTextAssetStepperView drlWatchField;

		public RectTransform multiGPFieldLabel;

		public DRLTextAssetStepperView multiGPField;

		public RectTransform usMilitaryFieldLabel;

		public DRLTextAssetStepperView usMilitaryField;

		public RectTransform amaMemberFieldLabel;

		public DRLTextAssetStepperView amaMemberField;

		public List<UINavigation> navigationElements;

		public List<Sprite> rankBadgeIcons;

		public GameObject profileStats;

		public GameObject profileData;

		public GameObject backButton;

		public GameObject playerDataButton;

		public GameObject discardButton;

		public GameObject saveButton;

		public GameObject achievementsButton;

		public bool showMandatoryFields;

		public DRLCampaign data;

		[Space]
		[Header("Progression")]
		public RectTransform progressionRankContainer;

		public CanvasGroup progressionHeaderGroup;

		public ImageClip progressionRankField;

		public ImageClip progressionLevelRangeField;

		public Text progressionLevelField;

		public Text progressionXPField;

		public RectTransform progressionXPProgressBar;

		public Text progressionStreakField;

		public Rotator progressionStreakPropRotator;

		public Text progressionStreakProgressField;

		public RectTransform progressionStreakProgressBar;

		public List<ImageClip> progressionWeekRanks;

		public FadeComponent progressionWeekRanksFade;

		public ListComponent progressionWeekRankList;

		public Text progressionWeekTimeField;

		public UIStatusView progressionWeekRankStatus;

		public Text progressionTierRankField;

		public DateTime progressionWeekRankStart;

		public DateTime progressionWeekRankEnd;

		public TimeSpan progressionWeekRankTimeStatus;

		[HideInInspector]
		public int m_profileColorSelected;

		public Dictionary<Color, int> m_profileColorToIndex;

		[HideInInspector]
		public UIElementView lastUnfocusedColor;

		private int m_xp_current;

		private int m_xp_prev;

		private int m_xp_next;

		private int m_streak;

		private int m_streak_current;

		private int m_streak_goal;

		private Activity m_rank_time_status_loop;

		public float rankTimeStatusSpeed = 1f;

		private bool m_is_rank_finished;

		public Texture cardPhoto
		{
			set
			{
				photoCardImageField.texture = value;
				photoCardImageField.enabled = value;
			}
		}

		public Color cardColor
		{
			set
			{
				photoCardColorField.color = value;
			}
		}

		public string profileName
		{
			set
			{
				profileNameField.text = value;
			}
		}

		public int age
		{
			get
			{
				int result = -1;
				int.TryParse(ageInputField.field.text, out result);
				return result;
			}
			set
			{
				ageInputField.field.enabled = false;
				ageInputField.field.text = value.ToString();
				ageInputField.field.enabled = true;
			}
		}

		public float flightTime
		{
			get
			{
				int result = -1;
				if (int.TryParse(flightTimeField.text.Split(' ')[0], out result))
				{
					return (float)result * 60f;
				}
				return result;
			}
			set
			{
				int num = (int)(value / 60f);
				flightTimeField.text = num + " " + base.app.model.storage.locale.Get("settings.profile-screen-dev.hours", "Hours").ToUpper();
			}
		}

		protected void Awake()
		{
			SetDRLColors();
			SetProgressionEnabled(base.app.online);
			progressionLevelRangeField.frames = base.app.model.storage.state.player.progression.GetLevelRangeThumbSprites().ToArray();
			progressionRankField.frames = base.app.model.storage.state.player.progression.GetLeagueThumbSprites().ToArray();
			for (int i = 0; i < progressionWeekRanks.Count; i++)
			{
				if ((bool)progressionWeekRanks[i])
				{
					progressionWeekRanks[i].frames = progressionRankField.frames;
				}
			}
		}

		public void SetProgressionEnabled(bool p_flag)
		{
			progressionHeaderGroup.alpha = (p_flag ? 1f : 0f);
			progressionRankField.gameObject.SetActive(p_flag);
			progressionRankContainer.gameObject.SetActive(p_flag);
		}

		public void SetProgression(int p_league, int p_level, int p_xp_current, int p_xp_prev, int p_xp_next, int p_streak, float p_streak_current, int p_streak_goal)
		{
			float num = Mathf.Max(0f, p_xp_next - p_xp_prev);
			float x = Mathf.Clamp01((num <= 0f) ? 0f : ((float)(p_xp_current - p_xp_prev) / num));
			float num2 = ((p_streak_goal <= 0) ? 0f : Mathf.Clamp01(p_streak_current / (float)p_streak_goal));
			progressionRankField.frame = ((p_league >= 0) ? p_league : 0);
			progressionRankField.gameObject.SetActive(p_league >= 0);
			progressionLevelField.text = string.Format("{0} {1}", base.app.model.storage.locale.Get("settings.profile-screen.dev.progression-level", "LEVEL"), p_level);
			progressionXPField.text = string.Format("{0} {1}/{2}", base.app.model.storage.locale.Get("settings.profile-screen.dev.progression-xp", "XP"), p_xp_current, p_xp_next);
			progressionStreakField.text = string.Format("{0} {1}", base.app.model.storage.locale.Get("settings.profile-screen.dev.progression-streak", "STREAK"), p_streak);
			progressionTierRankField.text = base.app.model.storage.locale.Get("settings.profile-screen.dev.progression-tier-rank", "TIER RANK") ?? "";
			if ((bool)progressionStreakProgressField)
			{
				progressionStreakProgressField.text = string.Format("{0} {1}/{2}", base.app.model.storage.locale.Get("settings.profile-screen.dev.race", "RACE"), Mathf.Min(p_streak_current, p_streak_goal), p_streak_goal);
			}
			if ((bool)progressionXPProgressBar)
			{
				progressionXPProgressBar.localScale = new Vector3(x, 1f, 1f);
			}
			if ((bool)progressionStreakProgressBar)
			{
				progressionStreakProgressBar.localScale = new Vector3(num2, 1f, 1f);
			}
			if ((bool)progressionStreakPropRotator)
			{
				progressionStreakPropRotator.speed = Vector3.Lerp(Vector3.forward * -45f, Vector3.forward * -900f, Mathf.Pow(num2, 4f));
			}
			progressionLevelRangeField.frame = base.app.model.storage.state.player.progression.GetLevelRangeIndexByLevel(p_level);
			m_xp_current = p_xp_current;
			m_xp_prev = p_xp_prev;
			m_xp_next = p_xp_next;
			m_streak = p_streak;
			m_streak_current = (int)p_streak_current;
			m_streak_goal = p_streak_goal;
		}

		public void SetProgression(DRLProgressionStateData p_state)
		{
			int leagueIndexByGUID = base.app.model.storage.state.player.progression.GetLeagueIndexByGUID(p_state.league.guid);
			int level = p_state.level;
			int previousLevelXP = p_state.previousLevelXP;
			int xp = p_state.xp;
			int nextLevelXP = p_state.nextLevelXP;
			int streak = p_state.streak;
			int streakMapIndex = p_state.streakMapIndex;
			int streakMapCount = p_state.streakMapCount;
			SetProgression(leagueIndexByGUID, level, xp, previousLevelXP, nextLevelXP, streak, streakMapIndex, streakMapCount);
		}

		public void SetProgression()
		{
			if (base.validContext)
			{
				ProgressionStateModel progression = base.app.model.storage.state.player.progression;
				SetProgression(progression.state);
			}
		}

		public void RefreshProgression()
		{
			base.app.model.storage.state.player.progression.Refresh(delegate
			{
				if (base.validContext)
				{
					SetProgression();
				}
			});
		}

		public void SetProgressionRank(DRLProgressionRankResult p_ranking)
		{
			progressionWeekRankEnd = p_ranking.rankingEndDate;
			progressionWeekRankTimeStatus = progressionWeekRankEnd - DateTime.UtcNow;
			SetProgressionWeekLeague(p_ranking.league.guid);
			List<DRLProgressionRankData> rankingList = p_ranking.GetRankingList();
			progressionWeekRankList.Clear();
			for (int i = 0; i < rankingList.Count; i++)
			{
				DRLProgressionRankData p_data = rankingList[i];
				UIProgressionRankItemView uIProgressionRankItemView = progressionWeekRankList.Push<UIProgressionRankItemView>();
				uIProgressionRankItemView.Set(p_data);
				uIProgressionRankItemView.SetRank(p_ranking.league.guid);
			}
			colorsNav.right = xpManualDataNav;
			playerDataNav.left = colorsNav;
			xpManualDataNav.left = colorsNav;
			LayoutGroup component = progressionWeekRankList.GetComponent<LayoutGroup>();
			UINavigation.Link(component, colorsNav, xpManualDataNav);
			if (rankingList.Count > 0)
			{
				colorsNav.right = component;
				playerDataNav.left = component;
				xpManualDataNav.left = component;
			}
		}

		public void RefreshProgressionWeekRank()
		{
			progressionWeekRankList.Clear();
			progressionWeekTimeField.text = base.app.model.storage.locale.Get("settings.profile-screen.dev.progression-wait", "WAIT");
			progressionWeekRanksFade.FadeOut(0.3f);
			progressionWeekRankStatus.fade.FadeIn(0.3f);
			progressionWeekRankStatus.SetLoading(0f);
			base.app.model.service.GetProgressionWeekRank(delegate(DRLProgressionRankResult p_result)
			{
				if (p_result == null || p_result.ranking.Length == 0)
				{
					progressionWeekTimeField.text = "-- -- -- --";
					string warning = base.app.model.storage.locale.Get("settings.profile-screen-dev.progression-week-rank-status-warning", "COMPLETE A RACE TO JOIN YOUR RANKING");
					progressionWeekRankStatus.SetWarning(warning);
				}
				else
				{
					SetProgressionRank(p_result);
					RefreshRankTimeStatus();
					progressionWeekRanksFade.FadeIn(0.8f, 0.5f);
					progressionWeekRankStatus.fade.FadeOut(0.3f);
				}
			});
		}

		public void RefreshRankTimeStatus()
		{
			StopRankTimeRefresh();
			float t = 1f;
			m_rank_time_status_loop = Activity.Run((Func<bool>)delegate
			{
				t += Time.deltaTime;
				progressionWeekRankTimeStatus -= new TimeSpan(0, 0, 0, 0, (int)(Time.deltaTime * 1000f * 0.93f * rankTimeStatusSpeed));
				if (t < 1f)
				{
					return true;
				}
				t = 0f;
				int days = progressionWeekRankTimeStatus.Days;
				int hours = progressionWeekRankTimeStatus.Hours;
				int minutes = progressionWeekRankTimeStatus.Minutes;
				int seconds = progressionWeekRankTimeStatus.Seconds;
				string text = string.Format("{0}{1} ", days, base.app.model.storage.locale.Get("settings.profile-screen.progression-days", "D"));
				string text2 = hours.ToString("00") + base.app.model.storage.locale.Get("settings.profile-screen.progression-hours", "H") + " ";
				string text3 = minutes.ToString("00") + base.app.model.storage.locale.Get("settings.profile-screen.progression-minutes", "M") + " ";
				string text4 = seconds.ToString("00") + base.app.model.storage.locale.Get("settings.profile-screen.progression-seconds", "S");
				if (days <= 0)
				{
					text = "";
				}
				if (days <= 0 && hours <= 0)
				{
					text2 = "";
				}
				if (days <= 0 && hours <= 0 && minutes <= 0)
				{
					text3 = "";
				}
				if (days <= 0 && hours <= 0 && minutes <= 0 && seconds <= 0)
				{
					text4 = "";
				}
				string text5 = text + text2 + text3 + text4;
				bool flag = !string.IsNullOrEmpty(text5);
				if (!flag)
				{
					text5 = base.app.model.storage.locale.Get("settings.profile-screen.dev.progression-tier-rank-finished", "RANK FINISHED!") ?? "";
				}
				if (m_is_rank_finished && flag)
				{
					Notify(5f, "settings.profile.progression.rank.enable");
					m_is_rank_finished = false;
				}
				if (!m_is_rank_finished && !flag)
				{
					Notify(5f, "settings.profile.progression.rank.finish");
					m_is_rank_finished = true;
				}
				progressionWeekTimeField.text = text5;
				return true;
			}, 0f, false);
		}

		public void StopRankTimeRefresh()
		{
			if (m_rank_time_status_loop != null)
			{
				m_rank_time_status_loop.Stop();
			}
			m_rank_time_status_loop = null;
		}

		public void SetProgressionWeekLeague(int p_league)
		{
			int num = p_league - 1;
			int frame = p_league;
			int frame2 = p_league + 1;
			int num2 = 1;
			if (p_league <= 0)
			{
				num = 0;
				frame = num + 1;
				frame2 = num + 2;
				num2 = 0;
			}
			if (p_league >= progressionRankField.count - 1)
			{
				num = progressionRankField.count - 1 - 2;
				frame = num + 1;
				frame2 = num + 2;
				num2 = 2;
			}
			progressionWeekRanks[0].frame = num;
			progressionWeekRanks[1].frame = frame;
			progressionWeekRanks[2].frame = frame2;
			progressionWeekRanks[0].target.color = new Color(1f, 1f, 1f, (num2 == 0) ? 1f : 0.2f);
			progressionWeekRanks[1].target.color = new Color(1f, 1f, 1f, (num2 == 1) ? 1f : 0.2f);
			progressionWeekRanks[2].target.color = new Color(1f, 1f, 1f, (num2 == 2) ? 1f : 0.2f);
		}

		public void SetProgressionWeekLeague(string p_league_guid)
		{
			int leagueIndexByGUID = base.app.model.storage.state.player.progression.GetLeagueIndexByGUID(p_league_guid);
			SetProgressionWeekLeague(leagueIndexByGUID);
		}

		public void Set(DRLCampaign p_data)
		{
			data = p_data;
		}

		public void Set(CampaignRegisterInfo p_data)
		{
			CampaignRegisterInfo campaignRegisterInfo = ((p_data == null) ? new CampaignRegisterInfo() : p_data);
			age = Mathf.Max(10, campaignRegisterInfo.age);
			emailField.field.text = campaignRegisterInfo.email;
			nameField.field.text = campaignRegisterInfo.name;
			countryField.field.text = campaignRegisterInfo.country;
			if (!string.IsNullOrEmpty(campaignRegisterInfo.gender))
			{
				genderField.SetValue(campaignRegisterInfo.gender);
			}
			if (!string.IsNullOrEmpty(campaignRegisterInfo.affiliationWatchDRL))
			{
				drlWatchField.SetValue(campaignRegisterInfo.affiliationWatchDRL);
			}
			if (!string.IsNullOrEmpty(campaignRegisterInfo.experienceNonFPV))
			{
				nonFpvDroneSkillField.SetValue(campaignRegisterInfo.experienceNonFPV);
			}
			if (!string.IsNullOrEmpty(campaignRegisterInfo.experienceNonFPVYears))
			{
				nonFpvDroneSkillYearsField.SetValue(campaignRegisterInfo.experienceNonFPVYears);
			}
			if (!string.IsNullOrEmpty(campaignRegisterInfo.experienceFPV))
			{
				fpvDroneSkillField.SetValue(campaignRegisterInfo.experienceFPV);
			}
			if (!string.IsNullOrEmpty(campaignRegisterInfo.experienceFPVYears))
			{
				fpvDroneSkillYearsField.SetValue(campaignRegisterInfo.experienceFPVYears);
			}
			if (!string.IsNullOrEmpty(campaignRegisterInfo.experiencePreferenceFPV))
			{
				fpvDronePreferenceField.SetValue(campaignRegisterInfo.experiencePreferenceFPV);
			}
			if (!string.IsNullOrEmpty(campaignRegisterInfo.experienceRealLifeRacing))
			{
				realLifeCompetitionField.SetValue(campaignRegisterInfo.experienceRealLifeRacing);
			}
			if (!string.IsNullOrEmpty(campaignRegisterInfo.experienceBuiltOwnDrone))
			{
				droneBuiltOwnField.SetValue(campaignRegisterInfo.experienceBuiltOwnDrone);
			}
			if (!string.IsNullOrEmpty(campaignRegisterInfo.affiliationMultiGP))
			{
				multiGPField.SetValue(campaignRegisterInfo.affiliationMultiGP);
			}
			if (!string.IsNullOrEmpty(campaignRegisterInfo.affiliationMilitary))
			{
				usMilitaryField.SetValue(campaignRegisterInfo.affiliationMilitary);
			}
			if (!string.IsNullOrEmpty(campaignRegisterInfo.affiliationAMA))
			{
				amaMemberField.SetValue(campaignRegisterInfo.affiliationAMA);
			}
		}

		public void SetFromProfile(ProfileStateModel p_data)
		{
			profileName = p_data.username;
			cardColor = p_data.color;
			cardPhoto = p_data.photo;
			PlayerStateModel player = base.app.model.storage.state.player;
			if (player != null)
			{
				int userRank = player.userRank;
				if (userRank == 8)
				{
					if ((bool)photoBadgeRank)
					{
						photoBadgeRank.sprite = rankBadgeIcons[userRank - 1];
						photoBadgeRank.gameObject.SetActive(photoBadgeRank.sprite != null);
					}
				}
				else if ((bool)photoBadgeRank)
				{
					photoBadgeRank.gameObject.SetActive(value: false);
				}
			}
			if (m_profileColorToIndex.ContainsKey(p_data.color))
			{
				m_profileColorSelected = m_profileColorToIndex[p_data.color];
				colorOutlines[m_profileColorSelected].Fade(1f);
				colorSwatches[m_profileColorSelected].Fade(1f);
			}
			if (p_data.age > 0)
			{
				age = p_data.age;
			}
			else
			{
				int result = -1;
				int.TryParse(ageInputField.field.text, out result);
				if (result <= 0)
				{
					age = 10;
				}
			}
			if (!string.IsNullOrEmpty(p_data.email))
			{
				emailField.field.text = p_data.email;
			}
			if (!string.IsNullOrEmpty(p_data.fullName))
			{
				nameField.field.text = p_data.fullName;
			}
			if (!string.IsNullOrEmpty(p_data.country))
			{
				countryField.field.text = p_data.country;
			}
			if (!string.IsNullOrEmpty(p_data.gender))
			{
				genderField.SetValue(p_data.gender);
			}
			if (!string.IsNullOrEmpty(p_data.watchDRL))
			{
				drlWatchField.SetValue(p_data.watchDRL);
			}
			if (!string.IsNullOrEmpty(p_data.experienceNonFPV))
			{
				nonFpvDroneSkillField.SetValue(p_data.experienceNonFPV);
			}
			if (!string.IsNullOrEmpty(p_data.experienceNonFPVYears))
			{
				nonFpvDroneSkillYearsField.SetValue(p_data.experienceNonFPVYears);
			}
			if (!string.IsNullOrEmpty(p_data.experienceFPV))
			{
				fpvDroneSkillField.SetValue(p_data.experienceFPV);
			}
			if (!string.IsNullOrEmpty(p_data.experienceFPVYears))
			{
				fpvDroneSkillYearsField.SetValue(p_data.experienceFPVYears);
			}
			if (!string.IsNullOrEmpty(p_data.experiencePreferenceFPV))
			{
				fpvDronePreferenceField.SetValue(p_data.experiencePreferenceFPV);
			}
			if (!string.IsNullOrEmpty(p_data.experienceRealLifeRacing))
			{
				realLifeCompetitionField.SetValue(p_data.experienceRealLifeRacing);
			}
			if (!string.IsNullOrEmpty(p_data.experienceBuiltOwnDrone))
			{
				droneBuiltOwnField.SetValue(p_data.experienceBuiltOwnDrone);
			}
			if (!string.IsNullOrEmpty(p_data.affiliationMultiGP))
			{
				multiGPField.SetValue(p_data.affiliationMultiGP);
			}
			if (!string.IsNullOrEmpty(p_data.affiliationMilitary))
			{
				usMilitaryField.SetValue(p_data.affiliationMilitary);
			}
			if (!string.IsNullOrEmpty(p_data.affiliationAMA))
			{
				amaMemberField.SetValue(p_data.affiliationAMA);
			}
			flightTime = p_data.flightTime;
		}

		public void SetFocus(Component p_target)
		{
			Transform transform = (p_target ? p_target.transform : null);
			for (int i = 0; i < colorSwatches.Count; i++)
			{
				FadeComponent fadeComponent = colorSwatches[i];
				bool flag = (bool)transform && fadeComponent.transform == transform;
				fadeComponent.Fade(flag ? 1f : 0.5f);
			}
		}

		public void SetDRLColors()
		{
			m_profileColorToIndex = new Dictionary<Color, int>();
			Color[] profileColors = DRLColor.profileColors;
			int num = Mathf.Min(profileColors.Length, colorSwatches.Count);
			for (int i = 0; i < num; i++)
			{
				Transform transform = colorSwatches[i].transform.Find("image");
				if ((bool)transform)
				{
					Image component = transform.GetComponent<Image>();
					component.color = profileColors[i + 1];
					m_profileColorToIndex.Add(component.color, i);
				}
			}
		}

		public void SelectColor(Component p_target, List<FadeComponent> p_list, List<FadeComponent> p_outlines, ref int p_index)
		{
			Transform transform = (p_target ? p_target.transform : null);
			for (int i = 0; i < p_list.Count; i++)
			{
				FadeComponent fadeComponent = p_list[i];
				FadeComponent fadeComponent2 = p_outlines[i];
				if ((bool)transform && fadeComponent.transform == transform)
				{
					p_outlines[p_index].Fade(0f);
					p_index = i;
					fadeComponent2.Fade(1f);
				}
			}
		}

		public void SetColorFocus(Component p_target, List<FadeComponent> p_list, List<FadeComponent> p_outlines, ref int p_index)
		{
			Transform transform = (p_target ? p_target.transform : null);
			for (int i = 0; i < p_list.Count; i++)
			{
				FadeComponent fadeComponent = p_list[i];
				FadeComponent fadeComponent2 = p_outlines[i];
				bool flag = (bool)transform && fadeComponent.transform == transform;
				fadeComponent.Fade((flag || p_index == i) ? 1f : 0.5f);
				fadeComponent2.Fade((flag || p_index == i) ? 1f : 0f);
			}
		}

		public void UnfocusColor(Component p_target, List<FadeComponent> p_list, List<FadeComponent> p_outlines, ref int p_index)
		{
			Transform transform = (p_target ? p_target.transform : null);
			for (int i = 0; i < p_list.Count; i++)
			{
				FadeComponent fadeComponent = p_list[i];
				FadeComponent fadeComponent2 = p_outlines[i];
				if ((bool)transform && fadeComponent.transform == transform)
				{
					fadeComponent.Fade((p_index != i) ? 0.5f : 1f);
					fadeComponent2.Fade((p_index != i) ? 0f : 1f);
				}
			}
		}

		public void SetColorPickerFocus()
		{
			colorPickerOutline.Fade(1f);
		}

		public void ClearColorPickerFocus()
		{
			colorPickerOutline.Fade(0f);
		}

		public void ToggleMandatoryField(int p_index, bool p_show)
		{
			mandatoryRedOutlines[p_index].SetActive(p_show);
			mandatoryGrayOutlines[p_index].SetActive(!p_show);
		}
	}
}
