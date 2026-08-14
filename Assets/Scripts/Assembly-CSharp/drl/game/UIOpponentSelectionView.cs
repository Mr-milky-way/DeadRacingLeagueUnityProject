using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIOpponentSelectionView : UIScreenView
	{
		[HideInInspector]
		public DroneRigData droneRigData;

		[HideInInspector]
		public DRLMap map;

		[HideInInspector]
		public DRLMapTrack track;

		[HideInInspector]
		public MapData customMap;

		[HideInInspector]
		public bool isCustomMap;

		[HideInInspector]
		public DRLMission mission;

		[HideInInspector]
		public DRLQuest quest;

		[HideInInspector]
		public DRLCampaign campaign;

		public List<Sprite> controllerTypeIcons_Standalone;

		public List<Sprite> controllerTypeIcons_XBox;

		public List<Sprite> controllerTypeIcons_PS;

		public Text selectionCount;

		public ListComponent raceListField;

		public FadeComponent raceListFade;

		public DRLPagePickerView racePageField;

		public UIOpponentSelectionItemView campaignItemField;

		public FadeComponent campaignRacesListFade;

		public ListComponent campaignRacesListField;

		public DRLPagePickerView campaignRacePageField;

		public DRLStepperView gameTypeStepper;

		public DRLStepperView droneClassStepper;

		public DRLIconStepperView controllerTypeStepper;

		public DRLStepperView physicsStepper;

		public DRLStepperView campaignStepper;

		public UINavigation regionNav;

		public List<RectTransform> regionItems;

		public DRLStepperView questStepper;

		public DRLStepperView missionStepper;

		public List<GameObject> feedbacks;

		public FadeComponent feedbackFade;

		public Text progressField;

		public UINavigation mapSelectionNav;

		public UINavigation mapNav;

		public UINavigation exitButtonNav;

		public UINavigation userButtonNav;

		public UINavigation startNav;

		public UIElementView userButtonView;

		public Text mapField;

		public RawImage mapThumb;

		public GameObject menu;

		public GameObject topContainer;

		public Text topContainerText;

		public GameFlag gameTypeFlag;

		[HideInInspector]
		public bool isCampaignRaceMode;

		public UIOpponentSelectionFeedbackType status;

		private string m_loadingWithDotsString;

		private string m_loadingString;

		public readonly int SpecificDroneClassIndex = 7;

		public string regionId;

		private AsyncRequest m_map_thumb_loader;

		public UINavigation campaignItemNav => campaignItemField.GetComponentInChildren<UINavigation>();

		public bool loading => status == UIOpponentSelectionFeedbackType.Loading;

		public float progress
		{
			set
			{
				progressField.text = ((value <= 0f) ? loadingWithDotsLocalized : (loadingLocalized + " " + Mathf.FloorToInt(Mathf.Clamp01(value) * 100f) + "%"));
			}
		}

		private string loadingWithDotsLocalized
		{
			get
			{
				if (string.IsNullOrEmpty(m_loadingWithDotsString))
				{
					m_loadingWithDotsString = base.app.model.storage.locale.Get("ui.common.loading-w-dots", "LOADING...");
				}
				return m_loadingWithDotsString;
			}
		}

		private string loadingLocalized
		{
			get
			{
				if (string.IsNullOrEmpty(m_loadingString))
				{
					m_loadingString = base.app.model.storage.locale.Get("leaderboard.progress-loading", "LOADING <color=red>/</color>");
				}
				return m_loadingString;
			}
		}

		public int droneClass
		{
			get
			{
				if (droneClassStepper.index > 0)
				{
					if (droneClassStepper.index != 1)
					{
						return droneClassStepper.index + 1;
					}
					return 1;
				}
				return -1;
			}
			set
			{
				if (value == 0)
				{
					droneClassStepper.index = 1;
				}
				else if (value < 3)
				{
					droneClassStepper.index = 0;
				}
				else if (value == SpecificDroneClassIndex + 1)
				{
					droneClassStepper.index = SpecificDroneClassIndex;
				}
				else
				{
					droneClassStepper.index = value - 1;
				}
			}
		}

		public int controllerType
		{
			get
			{
				return controllerTypeStepper.index;
			}
			set
			{
				controllerTypeStepper.index = value;
			}
		}

		public int physics
		{
			get
			{
				return physicsStepper.index - 1;
			}
			set
			{
				physicsStepper.index = value + 1;
			}
		}

		public void SetGameType(string p_type)
		{
			switch (p_type.ToLower())
			{
			case "race":
				gameTypeFlag = GameFlag.Race;
				break;
			case "campaign":
				gameTypeFlag = GameFlag.Campaign;
				break;
			case "mission":
				gameTypeFlag = GameFlag.Mission;
				break;
			}
			SetGameType(gameTypeFlag);
		}

		public void SetGameType(GameFlag p_type)
		{
			gameTypeFlag = p_type;
			droneClassStepper.gameObject.SetActive(value: false);
			questStepper.gameObject.SetActive(value: false);
			missionStepper.gameObject.SetActive(value: false);
			regionNav.gameObject.SetActive(value: false);
			regionNav.GetComponent<FadeComponent>().alpha = 0.1f;
			campaignStepper.gameObject.SetActive(value: false);
			menu.SetActive(value: true);
			bool active = false;
			if ((bool)base.app.model.game)
			{
				active = base.app.model.game.type == GameFlag.Replay;
			}
			exitButtonNav.gameObject.SetActive(active);
			startNav.gameObject.SetActive(value: true);
			startNav.GetComponent<UIElementView>().interactable = false;
			switch (gameTypeFlag)
			{
			case GameFlag.Race:
				droneClassStepper.gameObject.SetActive(value: true);
				break;
			case GameFlag.Mission:
				questStepper.gameObject.SetActive(value: true);
				missionStepper.gameObject.SetActive(value: true);
				break;
			case GameFlag.Campaign:
				droneClassStepper.gameObject.SetActive(value: false);
				gameTypeStepper.gameObject.SetActive(value: false);
				menu.SetActive(value: false);
				break;
			case GameFlag.Freestyle:
				break;
			}
		}

		public void SetTopVisible(bool p_flag)
		{
			if ((bool)topContainer)
			{
				topContainer.SetActive(p_flag);
			}
		}

		public void SetMyPositionEnabled(bool p_flag)
		{
			if ((bool)userButtonView)
			{
				userButtonView.interactable = p_flag;
			}
		}

		public void SetRegion(string p_id)
		{
			for (int i = 0; i < regionItems.Count; i++)
			{
				RectTransform rectTransform = regionItems[i];
				rectTransform.Find("outline").GetComponent<Image>().enabled = !(p_id == "") && rectTransform.name.Contains(p_id);
			}
			regionId = p_id;
		}

		public void SetCampaignRaceMode(bool p_flag)
		{
			isCampaignRaceMode = p_flag;
			raceListFade.Fade(p_flag ? (-0.1f) : 1f, 0.3f);
			campaignRacesListFade.Fade(p_flag ? 1f : (-0.1f), 0.3f);
			if (!p_flag)
			{
				campaignItemField.data = null;
			}
			DRLPagePickerView obj = (p_flag ? campaignRacePageField : racePageField);
			FadeComponent fade = obj.fade;
			if (fade.alpha < 0f)
			{
				fade.alpha = 0f;
			}
			if (obj.total > 1)
			{
				fade.FadeIn(0.3f);
			}
			else
			{
				fade.FadeOut(0.3f);
			}
		}

		public void PopulateCampaigns(int p_index)
		{
			List<string> campaignNames = base.app.model.storage.GetCampaignNames();
			for (int i = 0; i < campaignNames.Count; i++)
			{
				campaignNames[i] = campaignNames[i].ToUpper().Replace("\n", " ");
			}
			DRLStepperView dRLStepperView = campaignStepper;
			dRLStepperView.min = 0;
			dRLStepperView.max = campaignNames.Count - 1;
			dRLStepperView.labels = campaignNames.ToArray();
			if (p_index >= 0)
			{
				campaign = null;
			}
			List<DRLCampaign> campaigns = base.app.model.storage.GetCampaigns();
			dRLStepperView.index = Mathf.Clamp(p_index, dRLStepperView.min, dRLStepperView.max);
			if ((bool)campaign)
			{
				dRLStepperView.index = campaigns.IndexOf(campaign);
			}
			else
			{
				campaign = ((campaigns.Count <= 0) ? null : campaigns[dRLStepperView.index]);
			}
			if ((bool)campaign)
			{
				regionNav.enabled = false;
				regionNav.gameObject.SetActive(value: false);
			}
			dRLStepperView.Refresh();
		}

		public void SetMap(DRLMap p_map, DRLMapTrack p_track)
		{
			if (!(p_map == null) && !(p_track == null))
			{
				map = p_map;
				track = p_track;
				customMap = null;
				isCustomMap = false;
				mapField.text = p_map.title.Replace("\n", " ").ToUpper() + "  <color=#FF0000FF>/</color>  " + p_track.title.ToUpper();
				SetMapThumb(map.preview);
			}
		}

		public void SetMap()
		{
			if (!(map == null) && !(track == null))
			{
				customMap = null;
				isCustomMap = false;
				mapField.text = map.title.Replace("\n", " ").ToUpper() + "  <color=#FF0000FF>/</color>  " + track.title.ToUpper();
				SetMapThumb(map.preview);
			}
		}

		public void SetCampaign(DRLCampaign p_campaign)
		{
			if (!(p_campaign == null))
			{
				mapField.text = p_campaign.title.Replace("\n", " ").ToUpper();
				SetMapThumb(p_campaign.image);
			}
		}

		public void SetCustomMap(MapData p_map_data)
		{
			if (p_map_data == null)
			{
				return;
			}
			customMap = p_map_data;
			map = base.app.model.storage.library.FindByGUID<DRLMap>(customMap.mapId);
			track = null;
			isCustomMap = true;
			if (map == null)
			{
				mapField.text = customMap.mapTitle.Replace("\n", " ").ToUpper();
			}
			else
			{
				mapField.text = map.title.Replace("\n", " ").ToUpper() + "  <color=#FF0000FF>/</color>  " + customMap.mapTitle.Replace("\n", " ").ToUpper();
			}
			if (string.IsNullOrEmpty(customMap.mapThumbURL))
			{
				SetMapThumb(map.preview);
				return;
			}
			m_map_thumb_loader = Web.Get(customMap.GetThumbURL(MapData.ThumbSize.Small), delegate(Texture2D p_res, float p_progress, WebAsyncRequest p_req)
			{
				if (this != null && p_res != null)
				{
					SetMapThumb(p_res);
				}
			});
		}

		private void SetMapThumb(Texture p_thumb)
		{
			if ((bool)mapThumb)
			{
				mapThumb.texture = p_thumb;
			}
		}

		public void ClearPages()
		{
			FadeComponent fade = racePageField.fade;
			if ((bool)fade)
			{
				fade.FadeOut(0.3f);
			}
			RunOnce(delegate
			{
				racePageField.listField.Clear();
			}, 0.35f);
		}

		public void Clear()
		{
			raceListField.Clear();
		}

		public void SetFeedback(UIOpponentSelectionFeedbackType p_type, bool p_hide_list, float p_delay)
		{
			float feedback_alpha = ((p_type == UIOpponentSelectionFeedbackType.None) ? (-0.1f) : 1f);
			float content_alpha = ((p_type == UIOpponentSelectionFeedbackType.None) ? 1f : (p_hide_list ? (-0.1f) : 1f));
			FadeComponent content_fade = (isCampaignRaceMode ? campaignRacesListFade : raceListFade);
			status = p_type;
			if (status == UIOpponentSelectionFeedbackType.Loading)
			{
				progress = 0f;
			}
			Action action = delegate
			{
				feedbackFade.Fade(feedback_alpha, 0.3f, 0.05f, Cubic.Out);
				content_fade.Fade(content_alpha, 0.3f, 0f, Cubic.Out);
				if (p_type != UIOpponentSelectionFeedbackType.None)
				{
					int num = (int)p_type;
					for (int i = 0; i < feedbacks.Count; i++)
					{
						feedbacks[i].SetActive(i == num);
					}
				}
			};
			if (p_delay <= 0f)
			{
				action();
			}
			else
			{
				RunOnce(p_delay, action);
			}
		}

		public void SetFeedback(UIOpponentSelectionFeedbackType p_type, bool p_hide_list)
		{
			SetFeedback(p_type, p_hide_list, 0f);
		}

		public void SetFeedback(UIOpponentSelectionFeedbackType p_type)
		{
			SetFeedback(p_type, p_hide_list: true, 0f);
		}

		public void SetSelectionCount(int p_selected_players_count, int p_max_players = 5)
		{
			selectionCount.text = $"{p_selected_players_count}/{p_max_players}";
		}
	}
}
