using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISettingsGameController : Controller<DRLApp>
	{
		private Activity m_notify_timer;

		private MonoActivity m_delay_save;

		public UISettingsGameView view => AssertLocal<UISettingsGameView>("view");

		public StateModel model => base.app.model.storage.state;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "maps.selection-complete":
				if (!base.app.controller.AssertMapSelection(p_target, this, p_need_return: true))
				{
					break;
				}
				_ = (string)p_data[0];
				_ = (string)p_data[1];
				_ = (string)p_data[2];
				if ((bool)p_data[3])
				{
					if (!(p_data[6] is MapData))
					{
						break;
					}
					MapData customMap = (MapData)p_data[6];
					view.SetCustomMap(customMap);
				}
				else
				{
					if (!(p_data[4] is DRLMap) || !(p_data[5] is DRLMapTrack))
					{
						Debug.LogError("UISettingsGameController> MapSelectionComplete received invalid DRLMap or DRLMapTrack");
						break;
					}
					DRLMap dRLMap = (DRLMap)p_data[4];
					DRLMapTrack dRLMapTrack = (DRLMapTrack)p_data[5];
					view.Map = dRLMap;
					view.Track = dRLMapTrack;
					view.SetMap(dRLMap, dRLMapTrack);
				}
				Notify("ui.reset.track-leaderboard@click");
				break;
			case "ui.reset.track-leaderboard@click":
			{
				string mapID = view.Map?.guid;
				string trackID = view.Track?.guid;
				string customMapID = view.CustomMap?.guid;
				bool isCustomMap = view.isCustomMap;
				string[] options = new string[2] { "YES", "NO" };
				Texture2D icon = null;
				string id = "";
				string title = " THE SELECTED TRACK";
				if (isCustomMap && base.app.model.storage.maps.FindByGUID(customMapID) != null)
				{
					title = base.app.model.storage.maps.FindByGUID(customMapID).mapTitle.ToUpper();
				}
				if (view.Track != null && view.Track.title != null)
				{
					title = view.Track.title.ToUpper();
				}
				this.TimerRunOnce(delegate
				{
					base.app.view.ui.dialog.Open(DialogType.Info, "RESET LEADERBOARD TIMES", "DO YOU WANT TO RESET YOUR LEADERBOARD TIME FOR " + title, options, icon, id, delegate(string p_id, int p_option)
					{
						if (p_option == 1)
						{
							base.app.model.service.ResetTrackLeaderboardUser(mapID, trackID, customMapID, isCustomMap);
						}
					});
				}, 0.65f);
				break;
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
					view.armAndTurtle.gameObject.SetActive(value: false);
					view.armAndTurtleMode = false;
					base.app.model.storage.state.player.settings.game.armAndTurtle = false;
					view.SetRaceLineColors();
					view.raceLineColorSelectedIndex = base.app.model.storage.state.player.data.Get<int>("settings-game-race-line-color");
					view.SetRaceMarkerColors();
					view.raceMarkerColorSelectedIndex = base.app.model.storage.state.player.data.Get<int>("settings-game-check-point-color");
					view.SetColorFocusToSelected();
					FCProfileData active = base.app.model.storage.state.player.settings.tuning.GetActive();
					if (active == null)
					{
						Debug.LogWarning("UISettingsController> Invalid Profile\n" + base.app.model.storage.state.player.data.Get<string>("settings-fc-profiles"));
						break;
					}
					view.cameraFovSlider.slider.minValue = base.app.model.storage.state.player.settings.tuning.cameraMinFOV;
					view.cameraFovSlider.slider.maxValue = base.app.model.storage.state.player.settings.tuning.cameraMaxFOV;
					view.cameraFovSlider.slider.onValueChanged.AddListener(view.OnFOVChange);
					view.SetProfile(active);
					view.RefreshStates();
				}
				break;
			case "settings.game.form.event@change":
				OnFormNotification(p_target, p_is_change: true);
				break;
			case "settings.game.form.event@click":
				OnFormNotification(p_target, p_is_change: false);
				break;
			case "ui.screen.return@click":
				Debug.Log("UISettingsGameController> ScreenReturnClick");
				view.cameraFovSlider.slider.onValueChanged.RemoveListener(view.OnFOVChange);
				base.app.view.ui.screens.Return();
				break;
			case "ui.reset.leaderboards@click":
				base.app.view.ui.dialog.Open(DialogTemplateType.ResetLeaderboards, "reset-leaderboards", delegate(string p_id, int p_option)
				{
					if (p_option == 1)
					{
						this.TimerRunOnce(delegate
						{
							base.app.model.service.ResetLeaderboardUser(base.app.model.storage.state.player.playerData.playerId);
						}, 0.6f);
					}
				});
				break;
			case "leaderboards.choose-reset-map@click":
			{
				Debug.Log("UISettingsGameController-> Choose Reset Map Clicked");
				UIMapsCategoryView uIMapsCategoryView = base.app.view.ui.screens.Open<UIMapsCategoryView>("maps-category-screen");
				uIMapsCategoryView.screen.title = base.app.model.storage.locale.Get("maps.choose-map", "Choose Map");
				uIMapsCategoryView.caller = this;
				base.app.arguments.game.type = GameFlag.Race;
				break;
			}
			case "leaderboards.reset.filter.form.event@click":
				Debug.Log("UISettingsGameController-> Choose Reset Map Clicked");
				break;
			case "settings.race-line-color.color@click":
			{
				UINavigation.focus = view.raceLineColorsNav;
				UIElementView uIElementView2 = p_target as UIElementView;
				int siblingIndex2 = uIElementView2.transform.GetSiblingIndex();
				base.app.model.storage.state.player.data.SetInt("settings-game-race-line-color", siblingIndex2);
				base.app.model.storage.state.player.settings.game.raceLineColor = siblingIndex2;
				view.raceLineColorSelectedIndex = siblingIndex2;
				Color color2 = DRLColor.raceLineColors[siblingIndex2];
				Notify("settings.race-line-color.color@changed", color2);
				view.SelectColor(uIElementView2, view.raceLineColorSwatches, view.raceLineColorOutlines, ref view.raceLineColorSelectedIndex);
				view.UnfocusAllColors(view.raceLineColorSwatches, view.raceLineColorOutlines);
				view.SetColorFocus(uIElementView2, view.raceLineColorSwatches, view.raceLineColorOutlines, ref view.raceLineColorSelectedIndex);
				OnFormNotification(p_target, p_is_change: true);
				view.FadeInAllColors(view.raceLineColorSwatches);
				break;
			}
			case "settings.race-line-color.color@focus":
			{
				UIElementView p_target3 = p_target as UIElementView;
				view.SetColorFocus(p_target3, view.raceLineColorSwatches, view.raceLineColorOutlines, ref view.raceLineColorSelectedIndex);
				break;
			}
			case "settings.race-line-color.color@unfocus":
				view.lastUnfocusedColor = p_target as UIElementView;
				view.UnfocusColor(view.lastUnfocusedColor, view.raceLineColorSwatches, view.raceLineColorOutlines, ref view.raceLineColorSelectedIndex);
				break;
			case "settings.race-line-color.color-picker@click":
				UINavigation.Focus(view.raceLineColorSwatches[0].GetComponent<UINavigation>());
				view.scroll.enabled = false;
				break;
			case "settings.check-point-color.color@click":
			{
				UINavigation.Focus(view.checkPointColorsNav);
				UIElementView uIElementView = p_target as UIElementView;
				int siblingIndex = uIElementView.transform.GetSiblingIndex();
				base.app.model.storage.state.player.data.SetInt("settings-game-check-point-color", siblingIndex);
				base.app.model.storage.state.player.settings.game.raceMarkerColor = siblingIndex;
				view.raceMarkerColorSelectedIndex = siblingIndex;
				Color color = DRLColor.checkPointColors[siblingIndex];
				Notify("settings.check-point-color.color@changed", color);
				view.SelectColor(uIElementView, view.checkPointColorSwatches, view.checkPointColorOutlines, ref view.raceMarkerColorSelectedIndex);
				view.UnfocusAllColors(view.checkPointColorSwatches, view.checkPointColorOutlines);
				view.SetColorFocus(uIElementView, view.checkPointColorSwatches, view.checkPointColorOutlines, ref view.raceMarkerColorSelectedIndex);
				OnFormNotification(p_target, p_is_change: true);
				view.FadeInAllColors(view.checkPointColorSwatches);
				break;
			}
			case "settings.check-point-color.color@focus":
			{
				UIElementView p_target2 = p_target as UIElementView;
				view.SetColorFocus(p_target2, view.checkPointColorSwatches, view.checkPointColorOutlines, ref view.raceMarkerColorSelectedIndex);
				break;
			}
			case "settings.check-point-color.color@unfocus":
				view.lastUnfocusedColor = p_target as UIElementView;
				view.UnfocusColor(view.lastUnfocusedColor, view.checkPointColorSwatches, view.checkPointColorOutlines, ref view.raceMarkerColorSelectedIndex);
				break;
			case "settings.check-point-color.color-picker@click":
				UINavigation.Focus(view.checkPointColorSwatches[0].GetComponent<UINavigation>());
				view.scroll.enabled = false;
				break;
			}
		}

		protected void OnFormNotification(Object p_target, bool p_is_change)
		{
			if (view.notificationLock)
			{
				return;
			}
			bool flag = p_is_change;
			string text = p_target.name;
			bool flag2 = flag;
			PlayerStateModel player = model.player;
			AudioStateModel audio = player.settings.audio;
			GameStateModel game = base.app.model.storage.state.player.settings.game;
			ProfileStateModel profile = base.app.model.storage.state.player.profile;
			GameModel game2 = base.app.model.game;
			UIHUD uIHUD = (game2 ? base.app.view.ui.game.hud : null);
			bool flag3 = (bool)game2 && game2.multiplayer;
			int num = (game2 ? game2.racerCount : 0);
			float volumeMain;
			if (flag)
			{
				switch (text)
				{
				case "volume-main":
					break;
				case "volume-music":
					goto IL_052b;
				case "volume-sfx":
					goto IL_055a;
				case "volume-ui":
					goto IL_059f;
				case "language":
					goto IL_05b6;
				case "game-race-guide":
					goto IL_05f2;
				case "game-race-stats":
					goto IL_0609;
				case "game-race-fast-reset":
					goto IL_0620;
				case "game-radio-noise":
					goto IL_0637;
				case "game-gate-markers":
					goto IL_064e;
				case "game-fps-warning":
					goto IL_0665;
				case "game-controller-overlay":
					goto IL_067c;
				case "game-trails":
					goto IL_0693;
				case "game-trails-duration":
					goto IL_06aa;
				case "game-tuning-promode":
					goto IL_06c1;
				case "game-crosshair-visibility":
					goto IL_06d8;
				case "game-hotkeys":
					goto IL_06ef;
				case "game-crossplay-allowed":
					goto IL_0706;
				case "game-chat":
					goto IL_071d;
				case "game-damage":
					goto IL_073f;
				case "game-props-visibility":
					goto IL_076c;
				case "game-arm-turtle":
					goto IL_078e;
				case "game-propwash":
					goto IL_07b0;
				case "game-lens-distortion":
					goto IL_07dc;
				case "game-race-auto-standings":
					goto IL_080c;
				case "game-notifications":
				case "menu-notifications":
					goto IL_085b;
				default:
					goto IL_08e0;
				case null:
					goto IL_0958;
				}
				volumeMain = view.volumeMain;
				base.app.view.audio.volume = volumeMain;
				audio.volumeMain = volumeMain;
			}
			goto IL_08dd;
			IL_0706:
			game.crossplay = view.crossplay;
			goto IL_08dd;
			IL_076c:
			game.propsVisible = view.propsVisible;
			view.OnPropsChange();
			goto IL_08dd;
			IL_073f:
			game.damage = view.damageIndicator;
			view.OnDamageIndicatorChange(view.damageIndicator);
			goto IL_08dd;
			IL_071d:
			game.chat = view.inGameChatVisible;
			view.OnGameChatChange();
			goto IL_08dd;
			IL_07dc:
			game.lensDistortion = view.lensDistortion;
			RunOnce(delegate
			{
				view.OnFOVChange();
			}, 1.2f);
			goto IL_08dd;
			IL_07b0:
			game.propwash = view.propwashStepper.index;
			view.propwashStepper.Refresh();
			goto IL_08dd;
			IL_078e:
			game.armAndTurtle = view.armAndTurtleMode;
			view.OnArmAndTurtle();
			goto IL_08dd;
			IL_0958:
			if (flag2)
			{
				NotifyGameSettingsApply();
			}
			return;
			IL_085b:
			DRLStepperView dRLStepperView = ((text == "game-notifications") ? view.gameNotificationsStepper : view.menuNotificationsStepper);
			NotificationState notificationState = NotificationState.Off;
			notificationState = dRLStepperView.index switch
			{
				0 => NotificationState.Everyone, 
				1 => NotificationState.Friends, 
				2 => NotificationState.Off, 
				_ => NotificationState.Everyone, 
			};
			if (text == "game-notifications")
			{
				profile.notificationStateInGame = notificationState;
			}
			else
			{
				profile.notificationStateMenu = notificationState;
			}
			dRLStepperView.Refresh();
			goto IL_08dd;
			IL_080c:
			game.raceAutoStandings = view.raceAutostandingsActive;
			if (flag3 || num > 1)
			{
				uIHUD.standingsFade.Fade(game.raceAutoStandings ? 1f : (-0.1f), 0.12f);
			}
			goto IL_08dd;
			IL_08e0:
			if (!(text == "camera-tilt"))
			{
				if (text == "camera-fov")
				{
					Notify("settings.game.form.fov", view.cameraFov);
					if (flag)
					{
						DelaySave();
					}
				}
			}
			else
			{
				Notify("settings.game.form.tilt", view.cameraTilt);
				if (flag)
				{
					DelaySave();
				}
			}
			goto IL_0958;
			IL_08dd:
			if (text != null)
			{
				goto IL_08e0;
			}
			goto IL_0958;
			IL_052b:
			volumeMain = view.volumeMusic;
			base.app.view.audio.volumeMusic = volumeMain;
			audio.volumeMusic = volumeMain;
			goto IL_08dd;
			IL_055a:
			volumeMain = view.volumeSFX;
			base.app.view.audio.volumeSFX = volumeMain;
			base.app.view.audio.volumeDrones = volumeMain;
			audio.volumeSFX = volumeMain;
			goto IL_08dd;
			IL_059f:
			audio.audioUIEnabled = view.volumeUIActive;
			goto IL_08dd;
			IL_05b6:
			player.preferedLanguage = (PreferedLanguage)view.languageStepper.index;
			Notify("settings.language.apply");
			view.languageStepper.Refresh();
			goto IL_08dd;
			IL_05f2:
			game.raceGuide = view.raceGuideActive;
			goto IL_08dd;
			IL_0609:
			game.raceStats = view.raceStatsActive;
			goto IL_08dd;
			IL_0620:
			game.raceFastReset = view.raceFastResetActive;
			goto IL_08dd;
			IL_0637:
			game.radioNoise = view.radioNoiseActive;
			goto IL_08dd;
			IL_064e:
			game.gateMarkers = view.gateMarkersActive;
			goto IL_08dd;
			IL_0665:
			game.fpsWarning = view.fpsWarningActive;
			goto IL_08dd;
			IL_067c:
			game.controllerOverlay = view.controllerOverlayActive;
			goto IL_08dd;
			IL_0693:
			game.trails = view.trailsActive;
			goto IL_08dd;
			IL_06aa:
			game.trailsDuration = view.trailsDurationSeconds;
			goto IL_08dd;
			IL_06c1:
			game.tuningPromode = view.tuningPromode;
			goto IL_08dd;
			IL_06d8:
			game.crosshair = view.crosshairVisible;
			goto IL_08dd;
			IL_06ef:
			game.hotkeys = view.hotkeysEnabled;
			goto IL_08dd;
		}

		protected void NotifyGameSettingsApply()
		{
			if (m_notify_timer != null)
			{
				m_notify_timer.Stop();
			}
			m_notify_timer = Activity.RunOnce(delegate
			{
				Notify("settings.game.screen.apply");
			}, 1f);
		}

		protected void DelaySave()
		{
			if (m_delay_save != null)
			{
				m_delay_save.Stop();
			}
			m_delay_save = RunOnce(Save, 2f);
		}

		protected void Save()
		{
			FCProfileData active = model.player.settings.tuning.GetActive();
			view.GetProfile(active);
			model.player.settings.tuning.UpdateProfile(active);
			Notify("settings.tuning.profile.save", active);
			Debug.Log("UISettingsTuningController> Profile guid[" + active.guid + "] saved!");
		}
	}
}
