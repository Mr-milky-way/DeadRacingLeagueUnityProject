using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using drl.backend;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMapTrackOverviewController : Controller<DRLApp>
	{
		public List<WebAsyncRequest> ghostRequests;

		public WebAsyncRequest ghostDataRequest;

		public List<Thread> ghostParsers;

		public int ghostParsersComplete;

		public List<byte[]> ghostReplays;

		public Activity ghostProcessingLoop;

		public List<BlackboxRecord> ghostRecords;

		public Mutex ghostParsersMtx;

		public float ghostProcessTimeout;

		private bool m_starting;

		private bool m_hasCustomBotSelectedPreviously;

		private MonoActivity m_delaySave;

		public UIMapTrackOverviewView view => AssertLocal<UIMapTrackOverviewView>("view");

		public StateModel model => base.app.model.storage.state;

		protected bool valid
		{
			get
			{
				if (!this)
				{
					return false;
				}
				if (!base.gameObject)
				{
					return false;
				}
				if (!base.app)
				{
					return false;
				}
				if (!view)
				{
					return false;
				}
				return true;
			}
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "ui.screen@open":
				if (base.app.inMultiplayer)
				{
					Activity.RunOnce(delegate
					{
						view.screen.Hide(0f);
					}, 0.1f);
				}
				break;
			case "maps.track-selection-complete":
				if (base.app.inMultiplayer)
				{
					view.screen.TimerRunOnce(delegate
					{
						view.screen.Hide(0f);
					}, 0.1f);
				}
				break;
			case "fly.map-track-card@click":
				if (base.app.inMultiplayer)
				{
					base.app.view.ui.screens.Close(view.screen.name);
				}
				break;
			}
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				GameFlag type = base.app.arguments.game.type;
				view.Set(type);
				view.SetRatingsAvailable(p_available: false);
				FCProfileData active = base.app.model.storage.state.player.settings.tuning.GetActive();
				if (active == null)
				{
					Debug.LogWarning("UIMapTrackOverviewController> Invalid Profile\n" + base.app.model.storage.state.player.data.Get<string>("settings-fc-profiles"));
					break;
				}
				view.cameraFovSlider.slider.minValue = base.app.model.storage.state.player.settings.tuning.cameraMinFOV;
				view.cameraFovSlider.slider.maxValue = base.app.model.storage.state.player.settings.tuning.cameraMaxFOV;
				view.cameraFovSlider.slider.onValueChanged.AddListener(view.OnFOVChange);
				if (type == GameFlag.Race && m_hasCustomBotSelectedPreviously)
				{
					view.opponentMode = OpponentModeType.Custom;
				}
				view.SetProfile(active);
				FCMode activeFCMode = base.app.model.storage.state.player.activeFCMode;
				view.SetFCMode(activeFCMode, p_toggleHardcore: false);
				view.status.fade.alpha = -0.1f;
				FadeComponent fadeComponent = null;
				FadeComponent fadeComponent2 = null;
				fadeComponent = view.leaderCard.GetComponent<FadeComponent>();
				if ((bool)fadeComponent)
				{
					fadeComponent.alpha = 1f;
				}
				view.leaderCard.Set((DRLLeaderboardData)null, 0.4f);
				if ((bool)view.playerCard)
				{
					fadeComponent2 = view.playerCard.GetComponent<FadeComponent>();
				}
				if ((bool)fadeComponent2)
				{
					fadeComponent2.alpha = 1f;
				}
				if ((bool)view.playerCard)
				{
					view.playerCard.Set((DRLLeaderboardData)null, 0.4f);
				}
				m_starting = false;
				switch (type)
				{
				case GameFlag.Race:
					LoadLeaderboardFirst();
					LoadLeaderboardPlayer();
					break;
				case GameFlag.Collectable:
					LoadLeaderboardFirst();
					LoadLeaderboardPlayer();
					break;
				case GameFlag.Campaign:
					LoadLeaderboardFirst();
					LoadLeaderboardPlayer();
					break;
				case GameFlag.Freestyle:
					if ((bool)fadeComponent)
					{
						fadeComponent.Fade(0.1f, 1f);
					}
					if ((bool)fadeComponent2)
					{
						fadeComponent2.Fade(0.1f, 1f);
					}
					break;
				}
				bool opponentModeFlag = !DRLApp.offline && type == GameFlag.Race;
				view.SetOpponentModeFlag(opponentModeFlag);
				if (view.IsGoldbergDrone())
				{
					view.SetProModeOnly();
				}
				else
				{
					view.SetAllModes();
				}
				break;
			}
			case "network.update.offline":
				view.SetOpponentModeFlag(!DRLApp.offline);
				break;
			case "fly.map-track-overview.start@click":
				if (view.opponentMode == OpponentModeType.Custom)
				{
					m_hasCustomBotSelectedPreviously = true;
					Debug.Log("UIMapTrackOverviewController> OnNotification / OpponentModeType.Custom selected");
					UIOpponentSelectionView uIOpponentSelectionView = base.app.view.ui.screens.Open<UIOpponentSelectionView>("opponent-selection-screen");
					view.cameraFovSlider.slider.onValueChanged.RemoveListener(view.OnFOVChange);
					uIOpponentSelectionView.map = view.map;
					uIOpponentSelectionView.track = view.track;
					uIOpponentSelectionView.customMap = view.customData;
					uIOpponentSelectionView.droneRigData = base.app.model.storage.state.player.garage.currentRigData;
				}
				else
				{
					if (m_starting)
					{
						break;
					}
					m_hasCustomBotSelectedPreviously = false;
					if (view.opponentMode == OpponentModeType.Custom)
					{
						m_hasCustomBotSelectedPreviously = true;
						Debug.Log("UIMapTrackOverviewController> OnNotification / OpponentModeType.Custom selected");
						UIOpponentSelectionView uIOpponentSelectionView2 = base.app.view.ui.screens.Open<UIOpponentSelectionView>("opponent-selection-screen");
						view.cameraFovSlider.slider.onValueChanged.RemoveListener(view.OnFOVChange);
						uIOpponentSelectionView2.map = view.map;
						uIOpponentSelectionView2.track = view.track;
						uIOpponentSelectionView2.customMap = view.customData;
						uIOpponentSelectionView2.droneRigData = base.app.model.storage.state.player.garage.currentRigData;
					}
					else
					{
						if (m_starting)
						{
							break;
						}
						m_hasCustomBotSelectedPreviously = false;
						m_starting = true;
						ServiceModel sm = base.app.model.service;
						view.map.data = null;
						if (view.customData != null)
						{
							view.map.data = new MapData();
							view.map.data.Load(view.customData.ToJson());
						}
						sm.opponent.Cancel();
						view.cameraFovSlider.slider.onValueChanged.RemoveListener(view.OnFOVChange);
						if (view.isCustomMap || view.opponentMode != OpponentModeType.Off)
						{
							view.status.SetLoading(0f);
							view.status.fade.FadeIn(0.2f);
						}
						if (view.opponentMode == OpponentModeType.Off)
						{
							Notify("fly.map-track-overview.ready", new MapLoadData(view.map, view.track, view.customData, view.opponentMode));
							break;
						}
						DroneRigData currentRigData = base.app.model.storage.state.player.garage.currentRigData;
						int p_drone_class = ((currentRigData == null) ? 6 : currentRigData.diameter);
						bool p_drone_official = !(currentRigData == null) && base.app.model.storage.state.player.garage.IsOfficial(currentRigData);
						bool p_custom_physics = !(currentRigData == null) && currentRigData.hasCustomPhysics;
						int p_count = 1;
						switch (view.opponentMode)
						{
						case OpponentModeType.Leader:
							p_count = 1;
							break;
						case OpponentModeType.Top5:
							p_count = 6;
							break;
						case OpponentModeType.Rival5:
							p_count = 6;
							break;
						case OpponentModeType.Self:
							p_count = 3;
							break;
						case OpponentModeType.Random5:
							p_count = 6;
							break;
						case OpponentModeType.Random50:
							p_count = 50;
							break;
						}
						sm.opponent.Load(view.opponentMode, view.map, view.track, p_count, p_drone_class, p_drone_official, p_custom_physics, delegate
						{
							switch (sm.opponent.status)
							{
							case OpponentModel.Status.Error:
								view.status.SetWarning("LOADING FAILED!");
								view.status.fade.FadeOut(0.5f, 2f);
								base.app.view.audio.PlayUIGenericError();
								sm.opponent.Cancel();
								break;
							case OpponentModel.Status.NoResults:
								if (view.opponentMode == OpponentModeType.Rival5)
								{
									view.status.SetWarning("INITIAL TIME REQUIRED!");
								}
								else
								{
									view.status.SetWarning("NO OPPONENTS FOUND!");
								}
								sm.opponent.Cancel();
								Notify(1.5f, "fly.map-track-overview.ready", new MapLoadData(view.map, view.track, view.customData, view.opponentMode));
								break;
							case OpponentModel.Status.ByPass:
								Notify("fly.map-track-overview.ready", new MapLoadData(view.map, view.track, view.customData, view.opponentMode));
								break;
							case OpponentModel.Status.Progress:
								view.status.SetLoading(sm.opponent.progress);
								break;
							case OpponentModel.Status.Complete:
								Notify(1f / 60f, "fly.map-track-overview.ready", new MapLoadData(view.map, view.track, view.customData, view.opponentMode, sm.opponent.ghostRecords, sm.opponent.ghostRecordsV2));
								break;
							case OpponentModel.Status.ManifestSuccess:
								view.status.SetLoading(0f);
								base.app.view.audio.PlayUIGenericSuccess();
								break;
							}
						});
					}
				}
				break;
			case "fly.map-track-overview.form.event@click":
				OnFormNotification(p_target, p_change: false);
				break;
			case "fly.map-track-overview.form.event@change":
				OnFormNotification(p_target, p_change: true);
				break;
			case "fly.map-track-overview.pro@click":
				view.SetFCMode(FCMode.Pro);
				base.app.view.audio.PlayUILoadingSuccess();
				break;
			case "ui.screen.return@click":
				Debug.Log("UIMapTrackOverviewController> OnNotification [ScreenReturnClick] / ");
				m_hasCustomBotSelectedPreviously = view.opponentMode == OpponentModeType.Custom;
				if (m_starting)
				{
					break;
				}
				view.cameraFovSlider.slider.onValueChanged.RemoveListener(view.OnFOVChange);
				if (!base.app.inGame)
				{
					view.campaign = null;
					if (view.map != null)
					{
						view.map.data = null;
						view.map = null;
						view.usingCommunityMap = false;
					}
					base.app.model.storage.state.player.garage.currentRigData = null;
				}
				base.app.model.service.opponent.Cancel();
				base.app.view.ui.screens.Return();
				break;
			}
		}

		private IEnumerator DisableInput()
		{
			RCI.LockInput(l: true);
			yield return new WaitForSeconds(90f);
			RCI.LockInput(l: false);
		}

		private void OnFormNotification(Object p_target, bool p_change)
		{
			string text = (p_target ? p_target.name : "");
			bool flag = p_change;
			if (text == null)
			{
				return;
			}
			switch (text)
			{
			case "opponent-difficulty":
				break;
			case "mode-beginner":
				if (!flag)
				{
					base.app.model.storage.state.player.activeFCMode = FCMode.Beginner;
					view.SetFCMode(FCMode.Beginner, p_toggleHardcore: false);
					base.app.view.audio.PlayUILoadingSuccess();
				}
				break;
			case "mode-intermediate":
				if (!flag)
				{
					base.app.model.storage.state.player.activeFCMode = FCMode.Intermediate;
					view.SetFCMode(FCMode.Intermediate, p_toggleHardcore: false);
					base.app.view.audio.PlayUILoadingSuccess();
				}
				break;
			case "opponent-mode":
				if (flag)
				{
					switch (view.opponentMode)
					{
					case OpponentModeType.Off:
						view.SetOpponentDifficultyFlag(p_flag: false);
						break;
					case OpponentModeType.Leader:
						view.SetOpponentDifficultyFlag(p_flag: false);
						break;
					case OpponentModeType.Rival5:
						view.SetOpponentDifficultyFlag(p_flag: false);
						break;
					case OpponentModeType.Top5:
						view.SetOpponentDifficultyFlag(p_flag: false);
						break;
					case OpponentModeType.Custom:
						view.SetOpponentDifficultyFlag(p_flag: false);
						break;
					case OpponentModeType.TrackMaster:
						view.SetOpponentDifficultyFlag(p_flag: true);
						break;
					case OpponentModeType.Self:
					case OpponentModeType.Random5:
					case OpponentModeType.Random50:
						break;
					}
				}
				break;
			case "drone":
			{
				if (view == null || (view.track != null && view.track.promoDrones != null && view.track.promoDrones.Length == 1 && view.track.promoDronesOnly) || (view.map != null && view.map.promoDrones != null && view.map.promoDrones.Length == 1 && view.map.promoDronesOnly) || (view.campaign != null && view.campaign.drone != null))
				{
					break;
				}
				UIGarageRigSelectionView uIGarageRigSelectionView = base.app.view.ui.screens.Open<UIGarageRigSelectionView>("garage-rig-selection-screen");
				uIGarageRigSelectionView.screen.title = base.app.model.storage.locale.Get("multiplayer.select-drone-screen.title", "Select your Drone");
				uIGarageRigSelectionView.SetCreationEnabled(p_flag: false);
				uIGarageRigSelectionView.selectionOnly = true;
				uIGarageRigSelectionView.allowCustomPhysics = true;
				uIGarageRigSelectionView.promoList = null;
				uIGarageRigSelectionView.overrideList = null;
				uIGarageRigSelectionView.overrideSizes = null;
				if (view.track != null)
				{
					if (view.track.promoDrones != null && view.track.promoDrones.Length != 0)
					{
						if (view.track.promoDronesOnly)
						{
							uIGarageRigSelectionView.overrideList = new List<DroneRigData>(view.track.promoDrones);
						}
						else
						{
							uIGarageRigSelectionView.promoList = new List<DroneRigData>(view.track.promoDrones);
						}
					}
					else if (view.map != null && view.map.promoDrones != null && view.map.promoDrones.Length != 0)
					{
						if (view.map.promoDronesOnly)
						{
							uIGarageRigSelectionView.overrideList = new List<DroneRigData>(view.map.promoDrones);
						}
						else
						{
							uIGarageRigSelectionView.promoList = new List<DroneRigData>(view.map.promoDrones);
						}
					}
					if (view.track.droneSizes != null && view.track.droneSizes.Length != 0)
					{
						uIGarageRigSelectionView.overrideSizes = new List<int>(view.track.droneSizes);
					}
					else if (view.map != null && view.map.droneSizes != null && view.map.droneSizes.Length != 0)
					{
						uIGarageRigSelectionView.overrideSizes = new List<int>(view.map.droneSizes);
					}
				}
				uIGarageRigSelectionView.SetDroneClassEnabled(true);
				break;
			}
			case "camera-tilt":
			{
				float cameraTilt = view.cameraTilt;
				Notify("settings.game.form.tilt", cameraTilt);
				if (flag)
				{
					DelaySave();
				}
				break;
			}
			case "camera-fov":
			{
				float cameraFov = view.cameraFov;
				Notify("settings.game.form.fov", cameraFov);
				if (flag)
				{
					DelaySave();
				}
				break;
			}
			}
		}

		protected void DelaySave()
		{
			if (m_delaySave != null)
			{
				m_delaySave.Stop();
			}
			m_delaySave = RunOnce(Save, 2f);
		}

		protected void Save()
		{
			FCProfileData active = model.player.settings.tuning.GetActive();
			view.GetProfile(active);
			model.player.settings.tuning.UpdateProfile(active);
			Notify("settings.tuning.profile.save", active);
			Debug.Log("UIMapTrackOverviewController> Profile guid[" + active.guid + "] saved!");
		}

		protected void LoadLeaderboardFirst()
		{
			bool p_collectable = false;
			if (base.app.arguments.game.type == GameFlag.Collectable)
			{
				p_collectable = true;
			}
			DroneRigData currentRigData = base.app.model.storage.state.player.garage.currentRigData;
			MapData data = view.map.data;
			view.map.data = view.customData;
			base.app.model.service.SetLeaderboardCard(view.leaderCard, p_self: false, view.map, view.track, currentRigData.diameter, base.app.model.storage.state.player.garage.IsOfficial(currentRigData), currentRigData.hasCustomPhysics, p_collectable);
			view.map.data = data;
		}

		protected void LoadLeaderboardPlayer()
		{
			bool p_collectable = false;
			if (base.app.arguments.game.type == GameFlag.Collectable)
			{
				p_collectable = true;
			}
			DroneRigData currentRigData = base.app.model.storage.state.player.garage.currentRigData;
			if ((bool)view.playerCard)
			{
				MapData data = view.map.data;
				view.map.data = view.customData;
				base.app.model.service.SetLeaderboardCard(view.playerCard, p_self: true, view.map, view.track, currentRigData.diameter, base.app.model.storage.state.player.garage.IsOfficial(currentRigData), currentRigData.hasCustomPhysics, p_collectable);
				view.map.data = data;
			}
		}
	}
}
