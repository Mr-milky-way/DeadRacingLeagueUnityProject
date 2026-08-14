using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISettingsGameView : UIScreenView
	{
		public bool notificationLock;

		[Header("Volume")]
		public DRLSliderView volumeMainSlider;

		public DRLSliderView volumeMusicSlider;

		public DRLSliderView volumeSFXSlider;

		public DRLToggleView volumeUIToggle;

		[Space]
		public DRLSliderView cameraTiltSlider;

		public DRLSliderView cameraFovSlider;

		public DRLStepperView languageStepper;

		public DRLToggleView raceGuideToggle;

		public DRLToggleView raceStatsToggle;

		public DRLToggleView raceFastResetToggle;

		public DRLToggleView radioNoiseToggle;

		public DRLToggleView raceAutoStandingsToggle;

		public DRLToggleView gateMarkersToggle;

		public DRLToggleView fpsWarningToggle;

		public DRLToggleView controllerOverlayToggle;

		public DRLToggleView trailsToggle;

		public DRLSliderView trailsDurationSlider;

		public DRLToggleView lensDistortionToggle;

		public DRLToggleView propsVisibility;

		public DRLToggleView armAndTurtle;

		public DRLToggleView tuningPromodeToggle;

		public DRLStepperView propwashStepper;

		public DRLStepperView menuNotificationsStepper;

		public DRLStepperView gameNotificationsStepper;

		public DRLToggleView crosshairToggle;

		public DRLToggleView hotkeysToggle;

		public DRLToggleView inGameChatToggle;

		public DRLToggleView crossplayToggle;

		public UINavigation raceLineColorsNav;

		public List<FadeComponent> raceLineColorSwatches;

		public List<FadeComponent> raceLineColorOutlines;

		public FadeComponent raceLineColorPickerOutline;

		public UINavigation checkPointColorsNav;

		public List<FadeComponent> checkPointColorSwatches;

		public List<FadeComponent> checkPointColorOutlines;

		public FadeComponent checkPointColorPickerOutline;

		public DRLToggleView damageIndicatorToggle;

		public int raceLineColorSelectedIndex = 4;

		public Dictionary<Color, int> raceLineColorToIndex;

		public int raceMarkerColorSelectedIndex;

		public Dictionary<Color, int> raceMarkerColorToIndex;

		[HideInInspector]
		public UIElementView lastUnfocusedColor;

		[SerializeField]
		private DRLMap map;

		[SerializeField]
		private DRLMapTrack track;

		[SerializeField]
		private MapData customMap;

		public bool isCustomMap;

		public DRLStepperView circuitsStepper;

		public float volumeMain
		{
			get
			{
				return volumeMainSlider.slider.normalizedValue;
			}
			set
			{
				volumeMainSlider.slider.normalizedValue = value;
			}
		}

		public float volumeMusic
		{
			get
			{
				return volumeMusicSlider.slider.normalizedValue;
			}
			set
			{
				volumeMusicSlider.slider.normalizedValue = value;
			}
		}

		public float volumeSFX
		{
			get
			{
				return volumeSFXSlider.slider.normalizedValue;
			}
			set
			{
				volumeSFXSlider.slider.normalizedValue = value;
			}
		}

		public bool volumeUIActive
		{
			get
			{
				if ((bool)volumeUIToggle.toggle)
				{
					return volumeUIToggle.toggle.isOn;
				}
				return false;
			}
			set
			{
				if ((bool)volumeUIToggle && (bool)volumeUIToggle.toggle)
				{
					volumeUIToggle.toggle.isOn = value;
				}
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

		public bool raceGuideActive
		{
			get
			{
				return raceGuideToggle.toggle.isOn;
			}
			set
			{
				raceGuideToggle.toggle.isOn = value;
			}
		}

		public bool raceStatsActive
		{
			get
			{
				return raceStatsToggle.toggle.isOn;
			}
			set
			{
				raceStatsToggle.toggle.isOn = value;
			}
		}

		public bool raceFastResetActive
		{
			get
			{
				return raceFastResetToggle.toggle.isOn;
			}
			set
			{
				raceFastResetToggle.toggle.isOn = value;
			}
		}

		public bool radioNoiseActive
		{
			get
			{
				return radioNoiseToggle.toggle.isOn;
			}
			set
			{
				radioNoiseToggle.toggle.isOn = value;
			}
		}

		public bool raceAutostandingsActive
		{
			get
			{
				return raceAutoStandingsToggle.toggle.isOn;
			}
			set
			{
				raceAutoStandingsToggle.toggle.isOn = value;
			}
		}

		public bool gateMarkersActive
		{
			get
			{
				return gateMarkersToggle.toggle.isOn;
			}
			set
			{
				gateMarkersToggle.toggle.isOn = value;
			}
		}

		public bool fpsWarningActive
		{
			get
			{
				if ((bool)fpsWarningToggle.toggle)
				{
					return fpsWarningToggle.toggle.isOn;
				}
				return false;
			}
			set
			{
				if ((bool)fpsWarningToggle.toggle)
				{
					fpsWarningToggle.toggle.isOn = value;
				}
			}
		}

		public bool crossplay
		{
			get
			{
				if (!crossplayToggle.toggle)
				{
					return false;
				}
				return crossplayToggle.toggle.isOn;
			}
			set
			{
				if ((bool)crossplayToggle && (bool)crossplayToggle.toggle)
				{
					crossplayToggle.toggle.isOn = value;
				}
			}
		}

		public bool controllerOverlayActive
		{
			get
			{
				return controllerOverlayToggle.toggle.isOn;
			}
			set
			{
				controllerOverlayToggle.toggle.isOn = value;
			}
		}

		public bool trailsActive
		{
			get
			{
				if (!trailsToggle.toggle)
				{
					return true;
				}
				return trailsToggle.toggle.isOn;
			}
			set
			{
				if ((bool)trailsToggle.toggle)
				{
					trailsToggle.toggle.isOn = value;
				}
			}
		}

		public int propwashStrength
		{
			get
			{
				if (!propwashStepper)
				{
					return 0;
				}
				return propwashStepper.index;
			}
			set
			{
				if ((bool)propwashStepper)
				{
					propwashStepper.index = value;
					propwashStepper.Refresh();
				}
			}
		}

		public bool lensDistortion
		{
			get
			{
				return lensDistortionToggle.toggle.isOn;
			}
			set
			{
				lensDistortionToggle.toggle.isOn = value;
			}
		}

		public bool propsVisible
		{
			get
			{
				return propsVisibility.toggle.isOn;
			}
			set
			{
				propsVisibility.toggle.isOn = value;
			}
		}

		public bool armAndTurtleMode
		{
			get
			{
				if (!armAndTurtle.toggle)
				{
					return false;
				}
				return armAndTurtle.toggle.isOn;
			}
			set
			{
				if ((bool)armAndTurtle.toggle)
				{
					armAndTurtle.toggle.isOn = value;
				}
			}
		}

		public float trailsDurationSeconds
		{
			get
			{
				if (!trailsDurationSlider.slider)
				{
					return trailsDurationSlider.slider.value;
				}
				return 0.2f;
			}
			set
			{
				if ((bool)trailsDurationSlider.slider)
				{
					trailsDurationSlider.slider.value = value;
				}
			}
		}

		public bool tuningPromode
		{
			get
			{
				if (!tuningPromodeToggle.toggle)
				{
					return true;
				}
				return tuningPromodeToggle.toggle.isOn;
			}
			set
			{
				if ((bool)tuningPromodeToggle.toggle)
				{
					tuningPromodeToggle.toggle.isOn = value;
				}
			}
		}

		public bool crosshairVisible
		{
			get
			{
				return crosshairToggle.toggle.isOn;
			}
			set
			{
				crosshairToggle.toggle.isOn = value;
			}
		}

		public bool inGameChatVisible
		{
			get
			{
				if (!inGameChatToggle.toggle)
				{
					return false;
				}
				return inGameChatToggle.toggle.isOn;
			}
			set
			{
				if ((bool)inGameChatToggle.toggle)
				{
					inGameChatToggle.toggle.isOn = value;
				}
			}
		}

		public bool hotkeysEnabled
		{
			get
			{
				if (!hotkeysToggle.toggle)
				{
					return false;
				}
				return hotkeysToggle.toggle.isOn;
			}
			set
			{
				if ((bool)hotkeysToggle.toggle)
				{
					hotkeysToggle.toggle.isOn = value;
				}
			}
		}

		public bool damageIndicator
		{
			get
			{
				if (!damageIndicatorToggle.toggle)
				{
					return false;
				}
				return damageIndicatorToggle.toggle.isOn;
			}
			set
			{
				if ((bool)damageIndicatorToggle && (bool)damageIndicatorToggle.toggle)
				{
					damageIndicatorToggle.toggle.isOn = value;
				}
			}
		}

		public DRLMap Map
		{
			get
			{
				return map;
			}
			set
			{
				map = value;
			}
		}

		public DRLMapTrack Track
		{
			get
			{
				return track;
			}
			set
			{
				track = value;
			}
		}

		public MapData CustomMap
		{
			get
			{
				return customMap;
			}
			set
			{
				customMap = value;
			}
		}

		public void RefreshStates()
		{
			notificationLock = true;
			PlayerStateModel player = base.app.model.storage.state.player;
			AudioStateModel audio = player.settings.audio;
			GameStateModel game = player.settings.game;
			ProfileStateModel profile = player.profile;
			volumeMain = audio.volumeMain;
			volumeMusic = audio.volumeMusic;
			volumeSFX = audio.volumeSFX;
			volumeUIActive = audio.audioUIEnabled;
			languageStepper.index = (int)player.preferedLanguage;
			languageStepper.Refresh();
			raceGuideActive = game.raceGuide;
			raceStatsActive = game.raceStats;
			raceAutostandingsActive = game.raceAutoStandings;
			raceFastResetActive = game.raceFastReset;
			radioNoiseActive = game.radioNoise;
			gateMarkersActive = game.gateMarkers;
			fpsWarningActive = game.fpsWarning;
			controllerOverlayActive = game.controllerOverlay;
			trailsActive = game.trails;
			trailsDurationSeconds = game.trailsDuration;
			tuningPromode = game.tuningPromode;
			lensDistortion = game.lensDistortion;
			propsVisible = game.propsVisible;
			crosshairVisible = game.crosshair;
			inGameChatVisible = game.chat;
			damageIndicator = game.damage;
			hotkeysEnabled = game.hotkeys;
			crossplay = game.crossplay;
			crossplayToggle?.gameObject.SetActive(value: false);
			propwashStepper.index = game.propwash;
			propwashStepper.Refresh();
			menuNotificationsStepper.index = ((profile.notificationStateMenu != NotificationState.Everyone) ? ((profile.notificationStateMenu == NotificationState.Friends) ? 1 : 2) : 0);
			gameNotificationsStepper.index = ((profile.notificationStateInGame != NotificationState.Everyone) ? ((profile.notificationStateInGame == NotificationState.Friends) ? 1 : 2) : 0);
			menuNotificationsStepper.Refresh();
			gameNotificationsStepper.Refresh();
			notificationLock = false;
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

		public void GetProfile(FCProfileData p_data)
		{
			float fov = CameraLens.H2VFov(cameraFov);
			p_data.tilt = cameraTilt;
			p_data.fov = fov;
		}

		public void OnFOVChange(float value = 0f)
		{
			if (base.app.model.storage.state.player.settings.game.lensDistortion)
			{
				_ = FCProfileData.lensDistortionFOVOffset;
			}
			float num = CameraLens.H2Lens(CameraLens.V2HFov(CameraLens.H2VFov(cameraFov)));
			cameraFovSlider.unit = "º <color=#f00>/</color> " + Math.Round(num, 2) + "mm";
			cameraFovSlider.UpdateField();
		}

		public void OnPropsChange()
		{
			if (!(base.app.model.game == null))
			{
				Drone playerDrone = base.app.model.game.playerDrone;
				if (!(playerDrone == null))
				{
					playerDrone.renderer.propsVisible = base.app.model.storage.state.player.settings.game.propsVisible;
				}
			}
		}

		public void OnGameChatChange()
		{
			if (base.app.inGame)
			{
				Notify("chat.toggle");
			}
		}

		public void OnDamageIndicatorChange(bool p_flag)
		{
			if (base.app.inGame)
			{
				base.app.view.ui.game.hud.damage.Show(p_flag || (base.app.inTournament && base.app.tournament.drlPilotMode));
			}
		}

		public void OnArmAndTurtle()
		{
			if (!(base.app.model.game == null))
			{
				Drone playerDrone = base.app.model.game.playerDrone;
				if (!(playerDrone == null))
				{
					playerDrone.fc.turtle = false;
				}
			}
		}

		public void SetMap(DRLMap p_map, DRLMapTrack p_track)
		{
			if (!(p_map == null) && !(p_track == null))
			{
				Map = p_map;
				Track = p_track;
				CustomMap = null;
				isCustomMap = false;
			}
		}

		public void SetMap()
		{
			if (!(Map == null) && !(Track == null))
			{
				CustomMap = null;
				isCustomMap = false;
			}
		}

		public void SetCustomMap(MapData p_map)
		{
			if (p_map != null)
			{
				CustomMap = p_map;
				Map = base.app.model.storage.library.FindByGUID<DRLMap>(CustomMap.mapId);
				Track = null;
				isCustomMap = true;
			}
		}

		public void SetRaceLineColors()
		{
			raceLineColorToIndex = new Dictionary<Color, int>();
			Color[] raceLineColors = DRLColor.raceLineColors;
			int num = Mathf.Min(raceLineColors.Length, raceLineColorSwatches.Count);
			for (int i = 0; i < num; i++)
			{
				Transform transform = raceLineColorSwatches[i].transform.Find("image");
				if ((bool)transform)
				{
					transform.GetComponent<Image>().color = raceLineColors[i];
					raceLineColorToIndex.Add(raceLineColors[i], i);
				}
			}
		}

		public void SetRaceMarkerColors()
		{
			raceMarkerColorToIndex = new Dictionary<Color, int>();
			Color[] checkPointColors = DRLColor.checkPointColors;
			int num = Mathf.Min(checkPointColors.Length, checkPointColorSwatches.Count);
			for (int i = 0; i < num; i++)
			{
				Transform transform = checkPointColorSwatches[i].transform.Find("image");
				if ((bool)transform)
				{
					transform.GetComponent<Image>().color = checkPointColors[i];
					raceMarkerColorToIndex.Add(checkPointColors[i], i);
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

		public void SetColorFocusToSelected()
		{
			Color color = raceLineColorOutlines[raceLineColorSelectedIndex].GetComponentInChildren<Image>().color;
			color.a = 1f;
			raceLineColorOutlines[raceLineColorSelectedIndex].GetComponentInChildren<Image>().color = color;
			color = checkPointColorOutlines[raceMarkerColorSelectedIndex].GetComponentInChildren<Image>().color;
			color.a = 1f;
			checkPointColorOutlines[raceMarkerColorSelectedIndex].GetComponentInChildren<Image>().color = color;
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

		public void UnfocusAllColors(List<FadeComponent> p_list, List<FadeComponent> p_outlines)
		{
			for (int i = 0; i < p_list.Count; i++)
			{
				FadeComponent fadeComponent = p_list[i];
				FadeComponent fadeComponent2 = p_outlines[i];
				fadeComponent.Fade(0.5f);
				fadeComponent2.Fade(0f);
			}
		}

		public void FadeInAllColors(List<FadeComponent> p_list)
		{
			for (int i = 0; i < p_list.Count; i++)
			{
				p_list[i].Fade(1f);
			}
		}

		public void SetColorPickerFocus()
		{
			raceLineColorPickerOutline.Fade(1f);
		}

		public void ClearColorPickerFocus()
		{
			raceLineColorPickerOutline.Fade(0f);
		}
	}
}
