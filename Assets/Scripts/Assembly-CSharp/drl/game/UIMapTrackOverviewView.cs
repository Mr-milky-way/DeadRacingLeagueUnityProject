using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class UIMapTrackOverviewView : UIScreenView
	{
		public UICardButtonMap mapCard;

		public UICardButtonMapTrack trackCard;

		public UICardButtonDroneRig droneCard;

		public GameObject notRecommendedWarning;

		public UICardView beginnerCard;

		public UICardView intermediateCard;

		public UICardView proCard;

		public GameObject hardcoreButtonOff;

		public GameObject hardcoreButtonOn;

		public FadeComponent hardcoreToggleFade;

		public Text proDamageText;

		public GameObject solidBackgroundProMode;

		public GameObject redBackgroundProMode;

		public UILeaderboardCardView leaderCard;

		public UILeaderboardCardView playerCard;

		public DRLStepperView opponentModeStepper;

		public DRLStepperView opponentDifficultyStepper;

		public DRLToggleView droneCrashToggle;

		public DRLSliderView cameraTiltSlider;

		public DRLSliderView cameraFovSlider;

		public UIStatusView status;

		public FCMode fcMode;

		public DRLMap map;

		public DRLMapTrack track;

		public DRLCampaign campaign;

		public MapData customData;

		public bool usingCommunityMap;

		public bool isCustomMap => customData != null;

		public OpponentModeType opponentMode
		{
			get
			{
				return (OpponentModeType)opponentModeStepper.index;
			}
			set
			{
				opponentModeStepper.index = (int)value;
				opponentModeStepper.Refresh();
			}
		}

		public DifficultyType opponentDifficulty
		{
			get
			{
				return (DifficultyType)opponentDifficultyStepper.index;
			}
			set
			{
				opponentDifficultyStepper.index = (int)value;
				opponentDifficultyStepper.Refresh();
			}
		}

		public bool droneCrashEnabled
		{
			get
			{
				return droneCrashToggle.toggle.isOn;
			}
			set
			{
				droneCrashToggle.toggle.isOn = value;
			}
		}

		public float cameraFov
		{
			get
			{
				return Mathf.Round(cameraFovSlider.slider.value);
			}
			set
			{
				cameraFovSlider.slider.value = value;
				OnFOVChange(cameraFov);
			}
		}

		public float cameraTilt
		{
			get
			{
				return Mathf.Round(cameraTiltSlider.slider.value);
			}
			set
			{
				cameraTiltSlider.slider.value = value;
			}
		}

		public void GetProfile(FCProfileData p_data)
		{
			float fov = CameraLens.H2VFov(cameraFov);
			p_data.tilt = cameraTilt;
			p_data.fov = fov;
		}

		protected void Awake()
		{
		}

		public void Set(DRLMap p_data)
		{
			map = p_data;
			if ((bool)map && (bool)mapCard)
			{
				customData = null;
				mapCard.Set(map);
				usingCommunityMap = false;
			}
		}

		public void Set(DRLMapTrack p_data)
		{
			track = p_data;
			if ((bool)track)
			{
				trackCard.Set(track);
				mapCard.SetTrack(p_data);
			}
		}

		public void Set(DRLCommunityMapData p_map)
		{
			if (p_map != null)
			{
				MapData mapData = new MapData();
				mapData.Load(p_map.ToJson());
				Set(mapData);
			}
		}

		public void Set(MapData p_map)
		{
			customData = p_map;
			usingCommunityMap = true;
			DRLMap dRLMap = base.app.model.storage.library.FindByGUID<DRLMap>(p_map.mapId);
			DRLMapTrack dRLMapTrack = base.app.model.storage.GetMapTracks(dRLMap, GameFlag.Freestyle)[0];
			trackCard.Set(p_map, dRLMap);
			map = dRLMap;
			track = dRLMapTrack;
			mapCard.preview = map.preview;
			mapCard.image = map.image;
			DRLMap p_baseMap = dRLMap;
			if (p_map.mapCategoryFlag == GameFlag.MapMultiGP)
			{
				p_baseMap = base.app.model.storage.library.FindByGUID<DRLMap>("MP-7ea");
			}
			mapCard.SetTrack(p_map, p_baseMap);
		}

		public void SetRatingOverall(float p_rating, float p_delay = 0f, float p_item_delay = 0.25f)
		{
			mapCard.SetRating(p_rating, p_delay, p_item_delay);
		}

		public void SetRatingsAvailable(bool p_available)
		{
			if (mapCard.stars != null)
			{
				mapCard.stars.fade.alpha = (p_available ? 1f : 0f);
				mapCard.stars.Clear();
				mapCard.stars.SetProgress(0f);
			}
		}

		public void Set(GameFlag p_flag)
		{
			bool opponentModeFlag = false;
			bool tuningFlag = true;
			bool droneSelectionFlag = true;
			droneCard.model = base.app.model.storage.state.player.garage;
			notRecommendedWarning.SetActive(value: false);
			DroneRigData droneRigData = droneCard.model.currentRigData;
			if (campaign != null && campaign.drone != null)
			{
				droneRigData = campaign.drone;
				droneCard.Set(droneRigData, delegate
				{
					if (base.app != null && base.app.controller != null)
					{
						base.app.controller.RefreshFooterDrone();
					}
				});
				base.app.model.storage.state.player.garage.currentRigData = droneRigData;
				droneSelectionFlag = false;
			}
			else if (track != null && track.promoDrones != null && track.promoDrones.Length != 0)
			{
				List<DroneRigData> list = new List<DroneRigData>(track.promoDrones);
				if (!list.Contains(droneRigData))
				{
					if (track.promoDronesOnly)
					{
						droneRigData = track.promoDrones[0];
					}
					else
					{
						if (!base.app.model.storage.state.player.garage.RigExists(droneRigData))
						{
							droneRigData = base.app.model.storage.state.player.garage.defaultRig;
						}
						if (!list.Contains(droneRigData))
						{
							notRecommendedWarning.SetActive(value: true);
						}
					}
				}
				droneCard.Set(droneRigData, delegate
				{
					if (base.app != null && base.app.controller != null)
					{
						base.app.controller.RefreshFooterDrone();
					}
				});
				base.app.model.storage.state.player.garage.currentRigData = droneRigData;
				droneSelectionFlag = track.promoDrones.Length != 1 || !track.promoDronesOnly;
			}
			else if (map != null && map.promoDrones != null && map.promoDrones.Length != 0)
			{
				List<DroneRigData> list2 = new List<DroneRigData>(map.promoDrones);
				if (!list2.Contains(droneRigData))
				{
					if (map.promoDronesOnly)
					{
						droneRigData = map.promoDrones[0];
					}
					else
					{
						if (!base.app.model.storage.state.player.garage.RigExists(droneRigData))
						{
							droneRigData = base.app.model.storage.state.player.garage.defaultRig;
						}
						if (!list2.Contains(droneRigData))
						{
							notRecommendedWarning.SetActive(value: true);
						}
					}
				}
				droneCard.Set(droneRigData, delegate
				{
					if (base.app != null && base.app.controller != null)
					{
						base.app.controller.RefreshFooterDrone();
					}
				});
				base.app.model.storage.state.player.garage.currentRigData = droneRigData;
				droneSelectionFlag = map.promoDrones.Length != 1 || !map.promoDronesOnly;
			}
			else if (track != null && track.droneSizes != null && track.droneSizes.Length != 0)
			{
				List<int> list3 = new List<int>(track.droneSizes);
				if (list3.Contains(droneRigData.diameter))
				{
					droneCard.Set(droneRigData);
				}
				else
				{
					for (int num = 0; num < droneCard.model.originalRigs.Count; num++)
					{
						droneRigData = droneCard.model.originalRigs[num];
						if (list3.Contains(droneRigData.diameter))
						{
							droneCard.Set(droneRigData);
							base.app.model.storage.state.player.garage.currentRigData = droneRigData;
							break;
						}
					}
				}
			}
			else if (map != null && map.droneSizes != null && map.droneSizes.Length != 0)
			{
				List<int> list4 = new List<int>(map.droneSizes);
				if (list4.Contains(droneRigData.diameter))
				{
					droneCard.Set(droneRigData);
				}
				else
				{
					for (int num2 = 0; num2 < droneCard.model.originalRigs.Count; num2++)
					{
						droneRigData = droneCard.model.originalRigs[num2];
						if (list4.Contains(droneRigData.diameter))
						{
							droneCard.Set(droneRigData);
							base.app.model.storage.state.player.garage.currentRigData = droneRigData;
							break;
						}
					}
				}
			}
			else
			{
				if (!base.app.model.storage.state.player.garage.RigExists(droneRigData))
				{
					base.app.model.storage.state.player.garage.currentRigData = null;
					droneRigData = base.app.model.storage.state.player.garage.currentRigData;
				}
				if (!base.app.model.storage.state.player.garage.RigExists(droneRigData))
				{
					droneRigData = base.app.model.storage.state.player.garage.defaultRig;
					base.app.model.storage.state.player.garage.currentRigData = droneRigData;
				}
				droneCard.Set(droneRigData);
			}
			switch (p_flag)
			{
			case GameFlag.FreeCamera:
				tuningFlag = true;
				SetDroneCrashFlag(p_flag: false);
				opponentMode = OpponentModeType.Off;
				opponentDifficulty = DifficultyType.Easy;
				break;
			case GameFlag.Race:
				opponentModeFlag = true;
				SetDroneCrashFlag(p_flag: true);
				opponentDifficulty = DifficultyType.Easy;
				break;
			case GameFlag.Freestyle:
				SetDroneCrashFlag(p_flag: true);
				opponentMode = OpponentModeType.Off;
				opponentDifficulty = DifficultyType.Easy;
				break;
			case GameFlag.Campaign:
				opponentModeFlag = false;
				SetDroneCrashFlag(p_flag: false);
				opponentMode = OpponentModeType.Off;
				opponentDifficulty = DifficultyType.Easy;
				break;
			}
			if (!base.app.model.storage.state.license.exists)
			{
				opponentModeFlag = false;
			}
			leaderCard.GetComponent<CanvasGroup>().alpha = 0.2f;
			SetTuningFlag(tuningFlag);
			SetOpponentModeFlag(opponentModeFlag);
			SetDroneCrashFlag(p_flag: false);
			SetOpponentDifficultyFlag(p_flag: false);
			SetDroneSelectionFlag(droneSelectionFlag);
		}

		public void SetDroneSelectionFlag(bool p_flag)
		{
			CanvasGroup component = droneCard.GetComponent<CanvasGroup>();
			UINavigation component2 = droneCard.GetComponent<UINavigation>();
			component.alpha = (p_flag ? 1f : 1f);
			bool blocksRaycasts = (component.interactable = p_flag);
			component.blocksRaycasts = blocksRaycasts;
			component2.enabled = p_flag;
		}

		public void SetOpponentModeFlag(bool p_flag)
		{
			CanvasGroup component = opponentModeStepper.GetComponent<CanvasGroup>();
			UINavigation component2 = opponentModeStepper.GetComponent<UINavigation>();
			component.alpha = (p_flag ? 1f : 0.2f);
			bool blocksRaycasts = (component.interactable = p_flag);
			component.blocksRaycasts = blocksRaycasts;
			component2.enabled = p_flag;
		}

		public void SetOpponentDifficultyFlag(bool p_flag)
		{
			CanvasGroup component = opponentDifficultyStepper.GetComponent<CanvasGroup>();
			UINavigation component2 = opponentDifficultyStepper.GetComponent<UINavigation>();
			component.alpha = (p_flag ? 1f : 0.2f);
			bool blocksRaycasts = (component.interactable = p_flag);
			component.blocksRaycasts = blocksRaycasts;
			component2.enabled = p_flag;
			UINavigation component3 = opponentModeStepper.GetComponent<UINavigation>();
			UINavigation component4 = cameraTiltSlider.GetComponent<UINavigation>();
			UINavigation down = (p_flag ? component2 : component4);
			component3.down = down;
		}

		public void SetTuningFlag(bool p_flag)
		{
			UICardView uICardView = beginnerCard;
			CanvasGroup component = uICardView.GetComponent<CanvasGroup>();
			UINavigation component2 = uICardView.GetComponent<UINavigation>();
			component.alpha = ((!p_flag) ? 0.2f : (uICardView.selected ? 1f : 0.5f));
			component2.enabled = p_flag;
			uICardView = intermediateCard;
			CanvasGroup component3 = uICardView.GetComponent<CanvasGroup>();
			UINavigation component4 = uICardView.GetComponent<UINavigation>();
			component3.alpha = ((!p_flag) ? 0.2f : (uICardView.selected ? 1f : 0.5f));
			component4.enabled = p_flag;
			UINavigation uINavigation = (p_flag ? component2 : base.rightNavigation);
			UINavigation uINavigation2 = (p_flag ? component4 : base.rightNavigation);
			uICardView = proCard;
			CanvasGroup component5 = uICardView.GetComponent<CanvasGroup>();
			UINavigation component6 = uICardView.GetComponent<UINavigation>();
			component5.alpha = ((!p_flag) ? 0.2f : (uICardView.selected ? 1f : 0.5f));
			component6.enabled = p_flag;
			component6 = leaderCard.GetComponent<UINavigation>();
			component6.right = uINavigation;
			uINavigation.left = component6;
			if ((bool)playerCard)
			{
				component6 = playerCard.GetComponent<UINavigation>();
				component6.right = uINavigation2;
				uINavigation2.left = component6;
			}
		}

		public void SetDroneCrashFlag(bool p_flag)
		{
			UINavigation component = droneCrashToggle.GetComponent<UINavigation>();
			CanvasGroup component2 = droneCrashToggle.GetComponent<CanvasGroup>();
			component2.alpha = (p_flag ? 1f : 0.2f);
			bool blocksRaycasts = (component2.interactable = p_flag);
			component2.blocksRaycasts = blocksRaycasts;
			component.enabled = p_flag;
		}

		public void OnFOVChange(float p_hfov)
		{
			float num = CameraLens.H2Lens(CameraLens.V2HFov(CameraLens.H2VFov(cameraFov)));
			cameraFovSlider.unit = "º <color=#f00>/</color> " + Math.Round(num, 2) + "mm";
			cameraFovSlider.UpdateField();
		}

		public void SetProfile(FCProfileData p_data, float p_duration = 0f)
		{
			float p_to = 115f;
			float num = 83f;
			float p_to2 = 30f;
			if (p_data != null)
			{
				num = Mathf.Clamp(p_data.fov, CameraLens.H2VFov(cameraFovSlider.slider.minValue), CameraLens.H2VFov(cameraFovSlider.slider.maxValue));
				p_to = Mathf.Clamp(CameraLens.V2HFov(num), cameraFovSlider.slider.minValue, cameraFovSlider.slider.maxValue);
				p_to2 = p_data.tilt;
			}
			if (base.app.controller.game != null)
			{
				base.app.controller.game.model.camera.fov = num;
				p_to = CameraLens.V2HFov(num);
			}
			Tween.Add(this, "cameraTilt", p_to2, p_duration, Cubic.Out);
			Tween.Add(this, "cameraFov", p_to, p_duration, Cubic.Out);
		}

		public void SetFCMode(FCMode p_mode, bool p_toggleHardcore = true)
		{
			beginnerCard.selected = false;
			intermediateCard.selected = false;
			proCard.selected = false;
			switch (p_mode)
			{
			case FCMode.Beginner:
				beginnerCard.selected = true;
				break;
			case FCMode.Intermediate:
				intermediateCard.selected = true;
				break;
			case FCMode.Pro:
				proCard.selected = true;
				break;
			case FCMode.DRLPilot:
				proCard.selected = true;
				break;
			}
			FCMode activeFCMode = base.app.model.storage.state.player.activeFCMode;
			if (p_toggleHardcore && p_mode == FCMode.Pro && activeFCMode == FCMode.Pro)
			{
				p_mode = FCMode.DRLPilot;
			}
			base.app.model.storage.state.player.garage.IsOfficial();
			if (!base.app.model.storage.state.player.garage.CanUseDamage() && p_mode == FCMode.DRLPilot)
			{
				p_mode = FCMode.Pro;
			}
			SetDRLPilotMode(p_mode == FCMode.DRLPilot);
			base.app.model.storage.state.player.activeFCMode = p_mode;
			fcMode = p_mode;
			UICardView uICardView = beginnerCard;
			CanvasGroup component = beginnerCard.GetComponent<CanvasGroup>();
			Tween.Kill(component, "alpha");
			Tween.Add(component, "alpha", uICardView.selected ? 1f : 0.1f, 0.4f, 0f, Cubic.Out);
			uICardView = intermediateCard;
			CanvasGroup component2 = intermediateCard.GetComponent<CanvasGroup>();
			Tween.Kill(component2, "alpha");
			Tween.Add(component2, "alpha", uICardView.selected ? 1f : 0.1f, 0.4f, 0f, Cubic.Out);
			uICardView = proCard;
			CanvasGroup component3 = proCard.GetComponent<CanvasGroup>();
			Tween.Kill(component3, "alpha");
			Tween.Add(component3, "alpha", uICardView.selected ? 1f : 0.1f, 0.4f, 0f, Cubic.Out);
		}

		public void SetDRLPilotMode(bool p_flag)
		{
			solidBackgroundProMode.SetActive(!p_flag);
			redBackgroundProMode.SetActive(p_flag);
			hardcoreButtonOff.SetActive(!p_flag);
			hardcoreButtonOn.SetActive(p_flag);
			proDamageText.text = (p_flag ? "DAMAGE ON" : "DAMAGE OFF");
			HighlightHardcoreText();
		}

		public void HighlightHardcoreText()
		{
			bool num = base.app.model.storage.state.player.garage.IsOfficial();
			float num2 = 0.4f;
			num2 = (num ? 1f : 0.4f);
			hardcoreToggleFade.Fade(num2, 0.1f);
		}

		public bool IsGoldbergDrone()
		{
			return base.app.model.storage.state.player.garage.currentRigData.name == "Goldberg";
		}

		public void SetProModeOnly()
		{
			beginnerCard.interactable = false;
			intermediateCard.interactable = false;
			proCard.interactable = true;
			proCard.selected = true;
			SetFCMode(FCMode.Pro);
		}

		public void SetAllModes()
		{
			beginnerCard.interactable = true;
			intermediateCard.interactable = true;
			proCard.interactable = true;
			proCard.selected = true;
		}
	}
}
