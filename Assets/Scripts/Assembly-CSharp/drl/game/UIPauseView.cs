using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIPauseView : UIScreenView
	{
		public DRLSliderView cameraTiltSlider;

		public DRLSliderView cameraFovSlider;

		public DRLStepperView cameraModeStepper;

		public DRLStepperView droneRating;

		public DRLStepperView mapRating;

		public FadeComponent[] droneRatingStarFades;

		public FadeComponent[] mapRatingStarFades;

		public UICardView beginnerCard;

		public UICardView intermediateCard;

		public UICardView proCard;

		public GameObject hardcoreButtonOff;

		public GameObject hardcoreButtonOn;

		public FadeComponent hardcoreToggleFade;

		public Text proDamageText;

		public GameObject solidBackgroundProMode;

		public GameObject redBackgroundProMode;

		public Text changeGameTitle;

		public CanvasGroup droneGroup;

		public UIElementView changeGameCard;

		public UIElementView dashboardCard;

		public UIElementView restartCard;

		public UIElementView tuningCard;

		public UIElementView droneEditCard;

		public UIElementView droneChangeCard;

		public CanvasGroup droneFlightModes;

		public UIElementView droneRatingCard;

		public UIElementView mapRatingCard;

		public UIElementView roomPlayerState;

		public UIElementView roomAccess;

		public GameObject exitWarningContainer;

		public CanvasGroup tiltSliderGroup;

		public CanvasGroup fovSliderGroup;

		public GameObject forfeitPS4;

		public Image forfeitButtonIcon;

		public Sprite psButtonX;

		public Sprite psButtonO;

		public Image exitCrossIcon;

		public Sprite psExitAsiaSprite;

		public List<Text> exitCardLabels;

		public UINavigation[] droneNavs;

		public List<GameObject> singlePlayerCards;

		public Text playerRoomStateField;

		public List<Image> playerRoomStateIcons;

		public GameObject hotkeysPC;

		public GameObject hotkeysXbox;

		public GameObject hotkeysPS;

		public GameObject shortcutPhysics;

		public Image controllerSensitivityIcon;

		public Image controllerSetupIcon;

		public Sprite controllerSensitivitySprite;

		public Sprite controllerSetupSprite;

		public Texture2D exitDialogIcon;

		public bool ignoreReturn;

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

		protected void Awake()
		{
		}

		public void SetGame(GameFlag p_flag, bool p_is_online, bool p_from_editor, Drone p_drone)
		{
			bool interactable = false;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool active = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			bool inTournament = base.app.inTournament;
			bool flag7 = (base.app.inTournament && base.app.tournament.drlPilotMode) || (base.app.model.network.room != null && base.app.model.network.room.DRLPilotMode);
			bool tryouts = base.app.arguments.game.tryouts;
			bool inOnboarding = base.app.inOnboarding;
			bool inCircuits = base.app.inCircuits;
			if (!base.validContext)
			{
				return;
			}
			if (base.app.model.storage != null)
			{
				changeGameTitle.text = base.app.model.storage.locale.Get("pause.cards.change-game.change-map", "CHANGE\nMAP");
			}
			switch (p_flag)
			{
			case GameFlag.Campaign:
				interactable = false;
				flag2 = true;
				flag3 = true;
				active = true;
				break;
			case GameFlag.MapEditor:
				interactable = false;
				flag = false;
				flag2 = false;
				break;
			case GameFlag.FreeCamera:
				interactable = !p_is_online;
				flag = !p_is_online;
				break;
			case GameFlag.Freestyle:
			case GameFlag.Race:
				interactable = !p_is_online;
				flag = !p_is_online && !inTournament && !tryouts && !inOnboarding && !inCircuits;
				flag2 = !inOnboarding;
				flag3 = !inOnboarding && !flag7;
				flag5 = (flag6 = !p_is_online && !tryouts);
				if (inTournament && (base.app.arguments.game.tournamentData.droneClass == 1 || flag7))
				{
					flag5 = false;
				}
				flag4 = p_flag == GameFlag.Freestyle && !p_is_online && !inOnboarding;
				flag6 = !inTournament && !inOnboarding;
				break;
			case GameFlag.Sandbox:
				interactable = true;
				flag = false;
				flag2 = true;
				flag3 = !flag7;
				flag4 = (flag5 = true);
				flag6 = true;
				break;
			case GameFlag.Collectable:
				interactable = true;
				flag2 = true;
				flag = true;
				flag5 = true;
				flag3 = !inOnboarding && !flag7;
				break;
			case GameFlag.Mission:
				if (base.app.model.storage != null)
				{
					changeGameTitle.text = base.app.model.storage.locale.Get("pause.cards.change-map.label", "CHANGE\nMISSION");
				}
				interactable = true;
				flag = !inOnboarding;
				if (base.app.arguments.game.quest != null && base.app.arguments.game.quest.tags.Contains(GameFlag.DMVQuest))
				{
					flag2 = !inOnboarding;
					flag3 = false;
					if (base.app.model.storage != null)
					{
						changeGameTitle.text = base.app.model.storage.locale.Get("pause.cards.change-lesson.label", "CHANGE\nLESSON");
					}
					if (base.app.arguments.game.quest.testMission != null && base.app.arguments.game.quest.testMission.tag == "DiagnosticTest")
					{
						interactable = false;
						flag = false;
					}
				}
				if (base.app.arguments.game.mission != null && base.app.arguments.game.mission.tag == "Intro")
				{
					flag = false;
					interactable = false;
				}
				break;
			}
			if (p_from_editor)
			{
				flag = false;
			}
			exitCardLabels[0].enabled = !p_from_editor;
			exitCardLabels[1].enabled = p_from_editor;
			roomPlayerState.gameObject.SetActive(p_is_online);
			roomAccess.gameObject.SetActive(p_is_online);
			for (int i = 0; i < singlePlayerCards.Count; i++)
			{
				singlePlayerCards[i].SetActive(!p_is_online);
			}
			CanvasGroup canvasGroup = droneGroup;
			canvasGroup.alpha = (flag2 ? 1f : 0.2f);
			CanvasGroup canvasGroup2 = canvasGroup;
			bool blocksRaycasts = (canvasGroup.interactable = flag2);
			canvasGroup2.blocksRaycasts = blocksRaycasts;
			if (droneGroup != null)
			{
				for (int j = 0; j < canvasGroup.transform.childCount; j++)
				{
					UIElementView component = canvasGroup.transform.GetChild(j).GetComponent<UIElementView>();
					if ((bool)component)
					{
						component.enabled = flag2;
					}
				}
			}
			canvasGroup = droneFlightModes;
			canvasGroup.alpha = (flag3 ? 1f : 0.5f);
			CanvasGroup canvasGroup3 = canvasGroup;
			blocksRaycasts = (canvasGroup.interactable = flag3);
			canvasGroup3.blocksRaycasts = blocksRaycasts;
			if (droneFlightModes != null)
			{
				for (int k = 0; k < canvasGroup.transform.childCount; k++)
				{
					UIElementView component2 = canvasGroup.transform.GetChild(k).GetComponent<UIElementView>();
					if ((bool)component2)
					{
						component2.enabled = flag3;
					}
				}
			}
			bool flag10 = p_drone == null || p_drone.rig == null || p_drone.rig.isLocked || inOnboarding;
			bool flag11 = p_drone == null || p_drone.physics == null || p_drone.physics.isLocked || inOnboarding;
			bool flag12 = false;
			if (base.app.scene != null && base.app.scene.track != null && base.app.scene.track.promoDrones != null && base.app.scene.track.promoDrones.Length == 1 && base.app.scene.track.promoDronesOnly)
			{
				flag12 = true;
			}
			if (base.app.scene != null && base.app.scene.map != null && base.app.scene.map.promoDrones != null && base.app.scene.map.promoDrones.Length == 1 && base.app.scene.map.promoDronesOnly)
			{
				flag12 = true;
			}
			if (base.app.arguments.game.campaign != null && base.app.arguments.game.campaign.drone != null)
			{
				flag12 = true;
				flag10 = true;
				flag11 = true;
			}
			changeGameCard.interactable = flag && !inOnboarding;
			restartCard.interactable = interactable;
			tuningCard.interactable = flag2 && !inOnboarding;
			droneEditCard.interactable = flag4 && !flag10 && !inOnboarding;
			droneChangeCard.interactable = flag5 && !flag12 && !inOnboarding;
			dashboardCard.interactable = flag6 && !flag11;
			base.app.view.ui.footer.droneButton.interactable = droneChangeCard.interactable;
			exitWarningContainer.SetActive(active);
			RefreshNavigationTooltips();
		}

		public void SetSpectator(bool p_flag)
		{
			playerRoomStateIcons[0].gameObject.SetActive(!p_flag);
			playerRoomStateIcons[1].gameObject.SetActive(p_flag);
			playerRoomStateField.text = (p_flag ? base.app.model.storage.locale.Get("multiplayer.multiplayer-lobby-screen.quick-race.label", "RACE") : base.app.model.storage.locale.Get("multiplayer.multiplayer-room-screen.spectate.button", "SPECTATE"));
		}

		public void SetRoomStateChange(bool p_allow)
		{
			if ((bool)roomPlayerState)
			{
				roomPlayerState.interactable = p_allow;
			}
		}

		public void SetRoomAccess(bool p_allow)
		{
			if ((bool)roomAccess)
			{
				roomAccess.interactable = p_allow;
			}
		}

		public void SetCameraMode(GameCameraMode p_type)
		{
			cameraModeStepper.index = (int)p_type;
			cameraModeStepper.Refresh();
		}

		public void OnFOVChange(float p_hfov)
		{
			if (base.app.model.storage.state.player.settings.game.lensDistortion)
			{
				_ = FCProfileData.lensDistortionFOVOffset;
			}
			float num = CameraLens.H2Lens(CameraLens.V2HFov(CameraLens.H2VFov(cameraFov)));
			cameraFovSlider.unit = "º <color=#f00>/</color> " + Math.Round(num, 2) + "mm";
			cameraFovSlider.UpdateField();
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
			bool flag = base.app.model.storage.state.player.garage.IsOfficial();
			if ((!(base.app.model.storage.state.player.garage.CanUseDamage() && flag) || (base.app.inTournament && !base.app.tournament.drlPilotMode)) && p_mode == FCMode.DRLPilot)
			{
				p_mode = FCMode.Pro;
			}
			SetDRLPilotMode(p_mode == FCMode.DRLPilot);
			base.app.model.storage.state.player.activeFCMode = p_mode;
			UICardView uICardView = beginnerCard;
			CanvasGroup component = beginnerCard.GetComponent<CanvasGroup>();
			Tween.Kill(component);
			Tween.Add(component, "alpha", uICardView.selected ? 1f : 0.1f, 0.4f, 0f, Cubic.Out);
			uICardView = intermediateCard;
			CanvasGroup component2 = intermediateCard.GetComponent<CanvasGroup>();
			Tween.Kill(component2);
			Tween.Add(component2, "alpha", uICardView.selected ? 1f : 0.1f, 0.4f, 0f, Cubic.Out);
			uICardView = proCard;
			CanvasGroup component3 = proCard.GetComponent<CanvasGroup>();
			Tween.Kill(component3);
			Tween.Add(component3, "alpha", uICardView.selected ? 1f : 0.1f, 0.4f, 0f, Cubic.Out);
		}

		public void SetDrone(Drone p_drone)
		{
			if (p_drone == null)
			{
				return;
			}
			DroneFlightController fc = p_drone.fc;
			if (!(fc == null) && !(p_drone.body == null) && !(p_drone.body.frame == null) && !(p_drone.body.frame.camera == null))
			{
				cameraTilt = p_drone.body.frame.camera.tilt;
				FCMode fCMode = FCMode.None;
				HighlightHardcoreText();
				switch (fc.mode)
				{
				case FlightControllerMode.Beginner:
				case FlightControllerMode.DJI:
				case FlightControllerMode.Training:
					fCMode = FCMode.Beginner;
					break;
				case FlightControllerMode.Intermediate:
				case FlightControllerMode.Level:
					fCMode = FCMode.Intermediate;
					break;
				case FlightControllerMode.Acro:
				case FlightControllerMode.Pro:
					fCMode = FCMode.Pro;
					break;
				}
				if (base.app.model.storage.state.player.activeFCMode == FCMode.DRLPilot)
				{
					fCMode = FCMode.DRLPilot;
				}
				bool num = base.app.model.storage.state.player.garage.IsOfficial(p_drone.rig);
				bool flag = base.app.model.storage.state.player.garage.CanUseDamage();
				if (!(num || flag) && base.app.model.storage.state.player.activeFCMode == FCMode.DRLPilot)
				{
					fCMode = FCMode.Pro;
				}
				SetFCMode(fCMode, fCMode == FCMode.DRLPilot);
				if (base.app.model.network.room != null)
				{
					bool dRLPilotMode = base.app.model.network.room.DRLPilotMode;
					proCard.interactable = !dRLPilotMode;
					beginnerCard.interactable = !dRLPilotMode;
					intermediateCard.interactable = !dRLPilotMode;
				}
			}
		}

		public void RefreshNavigationTooltips()
		{
			DefaultControllerType defaultControllerType = RCI.GetDefaultControllerType(DefaultControllerType.XBox);
			bool flag = defaultControllerType == DefaultControllerType.XBox && RCI.GetActiveJoystick() != null;
			bool flag2 = defaultControllerType == DefaultControllerType.PS && RCI.GetActiveJoystick() != null;
			hotkeysXbox.SetActive(flag);
			hotkeysPS.SetActive(flag2);
			hotkeysPC.SetActive(!flag2 && !flag);
		}

		public void ClearMapRating(float p_duration)
		{
			for (int i = 0; i < mapRatingStarFades.Length; i++)
			{
				mapRatingStarFades[i].Fade(0.1f, p_duration);
			}
		}

		public void ClearDroneRating(float p_duration)
		{
			for (int i = 0; i < droneRatingStarFades.Length; i++)
			{
				droneRatingStarFades[i].Fade(0.1f, p_duration);
			}
		}

		public void FadeInMapRating(float p_duration, int p_rating)
		{
			if (p_rating > mapRatingStarFades.Length)
			{
				p_rating = mapRatingStarFades.Length;
			}
			mapRating.index = p_rating;
			for (int i = 0; i < mapRating.index; i++)
			{
				mapRatingStarFades[i].Fade(1f, p_duration + (float)i * 0.5f);
			}
		}

		public void FadeInDroneRating(float p_duration, int p_rating)
		{
			if (p_rating > droneRatingStarFades.Length)
			{
				p_rating = droneRatingStarFades.Length;
			}
			droneRating.index = p_rating;
			for (int i = 0; i < droneRating.index; i++)
			{
				droneRatingStarFades[i].Fade(1f, p_duration + (float)i * 0.5f);
			}
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
			bool flag = base.app.model.storage.state.player.garage.CanUseDamage();
			bool num2 = !num || !flag || (base.app.inTournament && !base.app.tournament.drlPilotMode);
			float num3 = 0.4f;
			num3 = (num2 ? 0.4f : 1f);
			hardcoreToggleFade.Fade(num3, 0.1f);
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
