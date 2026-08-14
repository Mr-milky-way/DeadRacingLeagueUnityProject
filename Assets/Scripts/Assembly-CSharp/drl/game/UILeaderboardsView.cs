using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UILeaderboardsView : UIScreenView
	{
		[Header("controller icons")]
		public List<Sprite> controllerTypeIcons_Standalone;

		public List<Sprite> controllerTypeIcons_XBox;

		public List<Sprite> controllerTypeIcons_PS;

		[Header("platform icons")]
		public List<Sprite> platformTypeIcons_Standalone;

		public List<Sprite> platformTypeIcons_XBox;

		public List<Sprite> platformTypeIcons_PS;

		public ListComponent raceListField;

		public FadeComponent raceListFade;

		public DRLPagePickerView racePageField;

		public UILeaderboardItemView campaignItemField;

		public FadeComponent campaignRacesListFade;

		public ListComponent campaignRacesListField;

		public DRLPagePickerView campaignRacePageField;

		public DRLStepperView gameTypeStepper;

		public DRLIconStepperView platformStepper;

		public DRLIconStepperView controllerTypeStepper;

		public DRLStepperView droneClassStepper;

		public DRLStepperView physicsStepper;

		public DRLStepperView campaignStepper;

		public DRLStepperView circuitsStepper;

		public UINavigation regionNav;

		public List<RectTransform> regionItems;

		public DRLStepperView questStepper;

		public DRLStepperView missionStepper;

		public List<GameObject> feedbacks;

		public FadeComponent feedbackFade;

		public Text progressField;

		public DRLInputFieldView replayURLInputField;

		public UINavigation mapSelectionNav;

		public UINavigation mapNav;

		public UINavigation exitButtonNav;

		public UINavigation backButtonNav;

		public UINavigation userButtonNav;

		public UIElementView userButtonView;

		public UINavigation showAllButton;

		public Text mapButtonText;

		public Text mapField;

		public RawImage mapThumb;

		public GameObject menu;

		public GameObject topContainer;

		public Text topContainerText;

		public DRLInputFieldView drlInputFieldView;

		public GameFlag gameTypeFlag;

		public bool isCampaignRaceMode;

		[NonSerialized]
		public bool drlOnly;

		public UILeaderboardFeedbackType status;

		private string mLoadingWithDotsStr;

		private string mLoadingStr;

		public DRLMap map;

		public DRLMapTrack track;

		public MapData customMap;

		public bool isCustomMap;

		public DRLMission mission;

		public DRLQuest quest;

		public DRLCampaign campaign;

		public string regionId;

		private AsyncRequest m_map_thumb_loader;

		private static object[] m_platform_stepper_values = new object[5] { null, "Steam", "Xbox", "PS4", null };

		private static object[] m_controller_stepper_values = new object[5] { null, "Taranis", "XBox", "PS4", null };

		public UINavigation campaignItemNav => campaignItemField.GetComponentInChildren<UINavigation>();

		public bool loading => status == UILeaderboardFeedbackType.Loading;

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
				if (string.IsNullOrEmpty(mLoadingWithDotsStr))
				{
					mLoadingWithDotsStr = base.app.model.storage.locale.Get("ui.common.loading-w-dots", "LOADING...");
				}
				return mLoadingWithDotsStr;
			}
		}

		private string loadingLocalized
		{
			get
			{
				if (string.IsNullOrEmpty(mLoadingStr))
				{
					mLoadingStr = base.app.model.storage.locale.Get("leaderboard.progress-loading", "LOADING <color=red>/</color>");
				}
				return mLoadingStr;
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
				else
				{
					droneClassStepper.index = value - 1;
				}
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

		public DRLCircuitData circuit { get; set; }

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
			platformStepper.gameObject.SetActive(value: false);
			controllerTypeStepper.gameObject.SetActive(value: false);
			droneClassStepper.gameObject.SetActive(value: false);
			questStepper.gameObject.SetActive(value: false);
			missionStepper.gameObject.SetActive(value: false);
			regionNav.gameObject.SetActive(value: false);
			regionNav.GetComponent<FadeComponent>().alpha = 0.1f;
			campaignStepper.gameObject.SetActive(value: false);
			mapNav.gameObject.SetActive(value: false);
			menu.SetActive(value: true);
			bool active = false;
			if ((bool)base.app.model.game)
			{
				active = base.app.model.game.type == GameFlag.Replay;
			}
			exitButtonNav.gameObject.SetActive(active);
			switch (gameTypeFlag)
			{
			case GameFlag.Race:
				controllerTypeStepper.gameObject.SetActive(value: true);
				droneClassStepper.gameObject.SetActive(!drlOnly);
				if (!physicsStepper)
				{
					physicsStepper = droneClassStepper;
				}
				physicsStepper.gameObject.SetActive(!drlOnly);
				mapNav.gameObject.SetActive(value: true);
				break;
			case GameFlag.Mission:
				questStepper.gameObject.SetActive(value: true);
				missionStepper.gameObject.SetActive(value: true);
				break;
			case GameFlag.Campaign:
				droneClassStepper.gameObject.SetActive(value: false);
				gameTypeStepper.gameObject.SetActive(value: false);
				if ((bool)physicsStepper)
				{
					physicsStepper.gameObject.SetActive(value: false);
				}
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

		public void SetDRLOnly(bool p_flag)
		{
			drlOnly = p_flag;
			droneClassStepper.gameObject.SetActive(!drlOnly);
			if (!physicsStepper)
			{
				physicsStepper = droneClassStepper;
			}
			physicsStepper.gameObject.SetActive(!drlOnly);
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
				mapButtonText.text = "CHOOSE";
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
				mapButtonText.text = "CHOOSE";
			}
		}

		public void ResetMap()
		{
			if (base.app.arguments.game.type == GameFlag.Collectable && (map == null || track == null))
			{
				List<MapData> list = base.app.model.storage.maps.FetchSDMaps();
				if (list.Count <= 0)
				{
					Debug.LogWarning("UILeaderboardsView> ResetMap / No Collectable Map Found");
					return;
				}
				SetCustomMap(list[0]);
				circuit = null;
				return;
			}
			if (map == null || track == null)
			{
				List<DRLMap> raceMaps = base.app.model.storage.GetRaceMaps();
				List<DRLMapTrack> mapTracks = base.app.model.storage.GetMapTracks(raceMaps[0], gameTypeFlag);
				map = raceMaps[0];
				track = mapTracks[0];
			}
			SetMap();
			circuit = null;
		}

		public void SetCircuit()
		{
			CircuitStateModel circuits = base.app.model.storage.state.player.circuits;
			if (circuit == null)
			{
				if (circuits == null || circuits.circuits == null || circuits.circuits.Length == 0)
				{
					return;
				}
				circuit = circuits.circuits[0];
			}
			if (circuit == null)
			{
				return;
			}
			mapField.text = circuit.name.ToUpper();
			mapButtonText.text = "CHOOSE";
			Web.Get("circuit-tex", circuit.imageURL, delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
			{
				if (p_progress >= 1f && !(p_result == null))
				{
					SetMapThumb(p_result);
				}
			});
		}

		public void SetCampaign(DRLCampaign p_campaign)
		{
			if (!(p_campaign == null))
			{
				mapField.text = p_campaign.title.Replace("\n", " ").ToUpper();
				SetMapThumb(p_campaign.image);
			}
		}

		public void SetTryouts(DRLCampaign p_campaign, string p_username)
		{
			if (!(p_campaign == null))
			{
				mapField.text = p_campaign.title.Replace("\n", " ").ToUpper();
				SetMapThumb(p_campaign.image);
				base.screen.title = p_username.ToUpper();
				gameTypeFlag = GameFlag.Campaign;
				gameTypeStepper.gameObject.SetActive(value: false);
				droneClassStepper.gameObject.SetActive(value: false);
				physicsStepper.gameObject.SetActive(value: false);
				questStepper.gameObject.SetActive(value: false);
				missionStepper.gameObject.SetActive(value: false);
				regionNav.gameObject.SetActive(value: false);
				campaignStepper.gameObject.SetActive(value: false);
				mapSelectionNav.gameObject.SetActive(value: false);
				exitButtonNav.gameObject.SetActive(value: false);
				mapNav.gameObject.SetActive(value: false);
				mapSelectionNav.gameObject.SetActive(value: true);
			}
		}

		public void SetCustomMap(MapData p_map)
		{
			if (p_map == null)
			{
				return;
			}
			customMap = p_map;
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

		public void SetMapThumb(Texture p_thumb)
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

		public void SetFeedback(UILeaderboardFeedbackType p_type, bool p_hide_list = true, float p_delay = 0f)
		{
			float feedback_alpha = ((p_type == UILeaderboardFeedbackType.None) ? (-0.1f) : 1f);
			float content_alpha = ((p_type == UILeaderboardFeedbackType.None) ? 1f : (p_hide_list ? (-0.1f) : 1f));
			FadeComponent content_fade = (isCampaignRaceMode ? campaignRacesListFade : raceListFade);
			status = p_type;
			if (status == UILeaderboardFeedbackType.Loading)
			{
				progress = 0f;
			}
			Action action = delegate
			{
				feedbackFade.Fade(feedback_alpha, 0.3f, 0.05f, Cubic.Out);
				content_fade.Fade(content_alpha, 0.3f, 0f, Cubic.Out);
				if (p_type != UILeaderboardFeedbackType.None)
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

		public string GetPlatformString()
		{
			if (platformStepper.index < 0 || platformStepper.index > 3)
			{
				Debug.LogWarning($"UILeaderboardsView> GetPlatformString / out-of-range value in platformStepper: {platformStepper.index}");
			}
			int num = Mathf.Clamp(platformStepper.index, 0, m_platform_stepper_values.Length - 1);
			return (string)m_platform_stepper_values[num];
		}

		public string GetControllerTypeString()
		{
			if (controllerTypeStepper.index < 0 || controllerTypeStepper.index > 3)
			{
				Debug.LogWarning($"UILeaderboardsView> GetControllerTypeString / out-of-range value in controllerTypeStepper: {controllerTypeStepper.index}");
			}
			int num = Mathf.Clamp(controllerTypeStepper.index, 0, m_controller_stepper_values.Length - 1);
			return (string)m_controller_stepper_values[num];
		}
	}
}
