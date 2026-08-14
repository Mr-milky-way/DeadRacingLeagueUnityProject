using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.network;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMultiplayerRoomController : Controller<DRLApp>
	{
		private const string DefaultDroneSpecificGUID = "DRD-fc5bf84d13e5bac67957921c";

		public UIMultiplayerRoomItemView userButton;

		public float stepperDelayTime;

		private Dictionary<UIElementView, MonoActivity> m_delayActivities;

		private MonoActivity m_tournamentLoader;

		private string m_prevRoomName;

		private string m_prevTournamentId;

		private UIMultiplayerRoomItemView m_swapTarget;

		private bool m_isMapLoading;

		private bool m_isRoomDataDirty;

		private Activity m_roomRefreshLoop;

		private Activity m_lockdownPoll;

		private int m_time = -1;

		public NetworkModel model => base.app.model.network;

		public UIMultiplayerRoomView view => AssertLocal<UIMultiplayerRoomView>("view");

		private void EnableSwapMode(UIMultiplayerRoomItemView p_item)
		{
			m_swapTarget = p_item;
			if ((bool)view)
			{
				view.OnSwapModeActivated(p_item);
			}
			UINavigation.Focus(m_swapTarget);
		}

		private bool IsInSwapMode()
		{
			return m_swapTarget != null;
		}

		private void OnItemClick(UIMultiplayerRoomItemView p_item, bool p_is_spectator_item)
		{
			if (!p_item)
			{
				return;
			}
			if (IsInSwapMode())
			{
				if (p_item != m_swapTarget)
				{
					if (p_item.valid && p_item.IsTaken())
					{
						if (p_item.data is NetworkActor)
						{
							model.SwapPlayers((NetworkActor)m_swapTarget.data, (NetworkActor)p_item.data);
						}
						else
						{
							model.ForceToRacer((NetworkActor)m_swapTarget.data);
						}
					}
					else if (p_item.IsPotentialSwapSlot())
					{
						int num = view.FindItemIndex(p_item, p_is_spectator_item);
						if (num != -1)
						{
							model.SwapPlayerToCard((NetworkActor)m_swapTarget.data, p_is_spectator_item, num);
						}
					}
				}
				EnableSwapMode(null);
			}
			else
			{
				OpenContextMenuOnItem(p_item);
			}
		}

		private void OpenContextMenuOnItem(UIMultiplayerRoomItemView p_item)
		{
			if (!p_item || !p_item.valid || p_item.isMaster || p_item.isBot || !view || view.room == null || !view.room.IsMaster || view.room.IsQuickMatch || view.room.IsTournamentMatch)
			{
				return;
			}
			if (true)
			{
				bool contextMenuEnabled = p_item.IsContextMenuEnabled();
				p_item.SetContextMenuEnabled(p_enabled: true);
				p_item.OpenMenu();
				if ((bool)p_item.contextMenuSpectateRace)
				{
					p_item.contextMenuSpectateRace.gameObject.SetActive(value: false);
				}
				bool active = true;
				if (!p_item.data.IsSpectator)
				{
					active = view.room != null && view.room.RacersCount + view.room.SpectatorsCount >= 2;
				}
				if ((bool)p_item.contextMenuSwap)
				{
					p_item.contextMenuSwap.gameObject.SetActive(active);
				}
				if ((bool)p_item.contextMenuKick)
				{
					p_item.contextMenuKick.gameObject.SetActive(!p_item.data.IsMaster);
				}
				p_item.SetContextMenuEnabled(contextMenuEnabled);
				if ((bool)p_item.contextMenuSwap && !p_item.contextMenuSwap.gameObject.activeInHierarchy && (bool)p_item.contextMenuKick && !p_item.contextMenuKick.gameObject.activeInHierarchy)
				{
					p_item.CloseMenu();
					return;
				}
			}
			else
			{
				if (p_item.data.IsMaster || p_item.data == model.room.Local)
				{
					return;
				}
				p_item.OpenMenu();
				bool active2 = true;
				if (!p_item.data.IsSpectator)
				{
					active2 = view.room.RacersCount >= 2;
				}
				if ((bool)p_item.contextMenuSpectateRace)
				{
					p_item.contextMenuSpectateRace.gameObject.SetActive(active2);
				}
				if ((bool)p_item.contextMenuKick)
				{
					p_item.contextMenuKick.gameObject.SetActive(value: true);
				}
				if ((bool)p_item.contextMenuSwap)
				{
					p_item.contextMenuSwap.gameObject.SetActive(value: false);
				}
			}
			userButton = p_item;
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event != null)
			{
				switch (p_event)
				{
				case "network.selection-complete":
				case "maps.selection-complete":
				{
					if (!(p_event == "network.selection-complete") && !base.app.controller.AssertMapSelection(p_target, this, p_need_return: true))
					{
						break;
					}
					string mapId = (string)p_data[0];
					string trackId = (string)p_data[1];
					string customMapId = (string)p_data[2];
					if ((bool)p_data[3])
					{
						if (!(p_data[6] is MapData))
						{
							Debug.LogError("UIMultiplayerRoomController> MapSelectionComplete received invalid DRLCommunityMapData");
							break;
						}
						MapData mapData = (MapData)p_data[6];
						base.app.model.network.room.MapId = mapData.mapId;
						base.app.model.network.room.TrackId = string.Empty;
						base.app.model.network.room.TrackLenght = 0f;
						base.app.model.network.room.CustomMapId = customMapId;
						base.app.model.network.room.CustomMapName = mapData.mapTitle;
						view.track = null;
					}
					else
					{
						if (!(p_data[4] is DRLMap) || !(p_data[5] is DRLMapTrack))
						{
							Debug.LogError("UIMultiplayerRoomController> MapSelectionComplete received invalid DRLMap or DRLMapTrack");
							break;
						}
						_ = (DRLMap)p_data[4];
						DRLMapTrack dRLMapTrack = (DRLMapTrack)p_data[5];
						base.app.model.network.room.CustomMapId = string.Empty;
						base.app.model.network.room.CustomMapName = string.Empty;
						base.app.model.network.room.MapId = mapId;
						base.app.model.network.room.TrackId = trackId;
						base.app.model.network.room.TrackLenght = dRLMapTrack.length;
					}
					view.Set(model.room, p_force: true);
					this.TimerRunOnce(delegate
					{
						view.UpdateDrone();
					}, 0.1f);
					break;
				}
				case "ui.screen@close":
				{
					Debug.Log("UIMultiplayerRoomController> ScreenClose / garage");
					if (p_data == null || p_data.Length == 0)
					{
						break;
					}
					UIScreen uIScreen = p_data[0] as UIScreen;
					if (!(uIScreen == null) && !(uIScreen != view.screen))
					{
						UIMultiplayerRoomItemView.ClearCache();
						if (view.screen.open)
						{
							view.screen.Hide(0f);
						}
					}
					break;
				}
				}
			}
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				m_isMapLoading = false;
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				Notify("network.footer@enable");
				base.app.view.ui.headerSecondary.AdjustUnderReviewWarningOffsetYByScreen(view);
				view.roomStatusField.fade.alpha = -0.1f;
				m_delayActivities = new Dictionary<UIElementView, MonoActivity>();
				NetworkRoom room = model.room;
				view.room = room;
				if (room != null)
				{
					switch (view.gameType)
					{
					case GameFlag.Freestyle:
						room.MapVotingCategory = MapCategory.None;
						room.DroneClass = 100;
						break;
					}
					m_prevRoomName = room.RoomTitle;
				}
				view.EnablePlayerStateButton(p_enable: false);
				AssertDefaultParameters();
				if (room != null && room.IsTournamentMatch)
				{
					GarageStateModel garage = base.app.model.storage.state.player.garage;
					DroneRigData droneRigData = garage.currentRigData;
					if (droneRigData == null)
					{
						int droneClass = room.DroneClass;
						if (droneClass > 0 && droneClass < 100)
						{
							droneRigData = garage.GetFirstOriginalRigWithDiameter(room.DroneClass);
							Debug.LogWarning("UIMultiplayerRoomController> OnNotification / Loading a fallback drone " + droneRigData.guid + " from {drone_class} inches class");
						}
						else
						{
							droneRigData = DroneRigData.FromJson(room.Master.DroneRigData);
							Debug.LogWarning("UIMultiplayerRoomController> OnNotification / Loading a SPECIFIC fallback drone " + droneRigData.guid);
						}
					}
					if (droneRigData == null)
					{
						droneRigData = garage.GetOriginalByGUID("DRD-fc5bf84d13e5bac67957921c");
						Debug.LogWarning("UIMultiplayerRoomController> OnNotification / Couldn't load a Drone in Tournament mode, loading a default one as a fallback " + droneRigData.guid);
					}
					view.SetSpecificDroneData(droneRigData);
					if (room.Local != null)
					{
						room.Local.DroneRigData = ((droneRigData == null) ? "" : droneRigData.ToJson());
					}
					else
					{
						Debug.Log("UIMultiplayerRoomController> OnNotification / [ScreenOpen] Local room is null.");
					}
				}
				if (room != null && model.isMaster)
				{
					string p_selected_drone_rig_data = ((base.app.model.storage.state.player.garage.currentRigData == null) ? model.room.SelectedDrone : base.app.model.storage.state.player.garage.currentRigData.guid);
					if (model.IsTournamentMatch)
					{
						p_selected_drone_rig_data = model.room.SelectedDrone;
						model.room.DroneClass = 101;
						if (base.app.inTournament && base.app.tournament.drlPilotMode)
						{
							model.room.DRLPilotMode = true;
						}
					}
					view.MasterInit(model.room.DroneClass, p_selected_drone_rig_data);
				}
				view.selectedDroneRigData = base.app.model.storage.state.player.garage.currentRigData.guid;
				view.UpdateDrone();
				view.Set(room, p_force: true);
				if (room != null && !room.IsMaster)
				{
					view.SetSpecificDroneData(view.app.model.storage.state.player.garage.currentRigData);
				}
				if (room != null)
				{
					view.UpdateGhosts(room.Ghosts, room.RacersCount);
				}
				base.app.view.ui.screens.manager.GetComponent<UIScreenManagerController>().ValidatePromo();
				if (base.app.inGame && base.app.arguments.game.tournamentData != null)
				{
					view.readyToggleNav.gameObject.SetActive(value: false);
				}
				view.backButton.gameObject.SetActive(value: true);
				bool flag = base.app.view.ui.screens.manager.history.Count > 1;
				if (base.app.inGame && !flag)
				{
					view.backButton.gameObject.SetActive(value: false);
				}
				view.startButton.interactable = false;
				this.TimerRunOnce(delegate
				{
					if (!base.app.model.network.fetchingBots)
					{
						view.startButton.interactable = true;
					}
				}, 7f);
				this.TimerRunOnce(delegate
				{
					if (view.room != null && view.room.IsMaster)
					{
						view.room.RaceId = GUID.Create(24, "", 200, 0, 15, "x1");
					}
				}, 1f);
				this.TimerRunOnce(delegate
				{
					if (base.app.model.game != null)
					{
						base.app.model.game.camera.main.enabled = false;
					}
				}, 2f);
				int num = 3;
				int num2 = 2;
				NetworkRoom.GameType gameType = room?.GameMode ?? NetworkRoom.GameType.Race;
				if (gameType == NetworkRoom.GameType.Freestyle)
				{
					num = 4;
					num2 = 3;
				}
				Debug.Log($"UIMultiplayerRoomController> SetLayout {gameType} - r[{num}] c[{num2}]");
				view.SetPlayerLayout(num, num2, gameType != NetworkRoom.GameType.Freestyle);
				RefreshRoom(p_force: true);
				break;
			}
			case "ui.screen@close":
				UIMultiplayerRoomItemView.ClearCache();
				base.app.view.ui.footer.droneButton.interactable = true;
				break;
			case "ui.screen.breadcrumb@click":
				UIMultiplayerRoomItemView.ClearCache();
				base.app.view.ui.footer.droneButton.interactable = true;
				if (model.room.State == NetworkRoom.StateCode.MatchMaking)
				{
					model.LeaveRoom();
				}
				break;
			case "multiplayer.room.racer-item@click":
			case "multiplayer.room.spectator-item@click":
			{
				UIMultiplayerRoomItemView p_item = p_target as UIMultiplayerRoomItemView;
				OnItemClick(p_item, p_event == "multiplayer.room.spectator-item@click");
				break;
			}
			case "multiplayer.room.spectator-item.menu@click":
			case "multiplayer.room.racer-item.menu@click":
			{
				UIElementView uIElementView = p_target as UIElementView;
				if (!uIElementView || !userButton)
				{
					break;
				}
				if (userButton.data is NetworkActor)
				{
					switch (uIElementView.name)
					{
					case "race":
						model.ForceToRacer((NetworkActor)userButton.data);
						break;
					case "spectate":
						model.ForceToSpectator((NetworkActor)userButton.data);
						break;
					case "kick":
						model.TryKickPlayer((NetworkActor)userButton.data);
						break;
					case "swap":
						EnableSwapMode(userButton);
						break;
					}
				}
				userButton.CloseMenu();
				break;
			}
			case "network.player@update":
			case "network.room.update":
				m_isRoomDataDirty = true;
				if (m_roomRefreshLoop != null)
				{
					break;
				}
				m_roomRefreshLoop = Activity.Run((Func<bool>)delegate
				{
					if (!m_isRoomDataDirty)
					{
						m_roomRefreshLoop = null;
						return false;
					}
					bool p_force = p_event == "network.player@update" || HasNewMaster();
					RefreshRoom(p_force);
					m_isRoomDataDirty = false;
					return true;
				}, 0f, false);
				break;
			case "network.room@enter":
				view.Set(model.room);
				base.app.model.storage.state.player.circuits.ClearInProgress();
				break;
			case "network.room@exit":
				if (!(view.screen != base.app.view.ui.screens.current) && (!base.app.inGame || base.app.model.network.photon.IsConnectedAndReady))
				{
					if (base.app.view.ui.screens.manager.history.Count == 1)
					{
						base.app.view.ui.screens.Close("multiplayer-room-screen");
					}
					else
					{
						base.app.view.ui.screens.Return();
					}
				}
				break;
			case "network.room@lock":
				base.enabled = false;
				if (model.room.IsUsingGhosts || model.room.UsingCustomMap)
				{
					view.roomStatusField.fade.FadeIn(0.2f);
					view.roomStatusField.SetLoading(0f);
				}
				if ((base.app.inGame && base.app.arguments.game.tournamentData == null) || (!base.app.inGame && base.app.arguments.tournament == null))
				{
					model.room.TimeLimit = 1200.0;
				}
				break;
			case "network.ghosts.count":
			{
				NetworkRoom room2 = model.room;
				if (room2 != null)
				{
					view.UpdateGhosts(room2.Ghosts, room2.RacersCount);
					view.UpdatePlayerList(room2.Racers, p_spectators: false, p_force: false);
					view.UpdatePlayerList(room2.Spectators, p_spectators: true, p_force: false);
				}
				break;
			}
			case "network.custom-map.load@start":
				view.roomStatusField.fade.FadeIn(0.2f);
				view.roomStatusField.SetLoading(0f);
				view.roomStatusField.message = "LOADING MAP DATA...";
				break;
			case "network.ghosts.status":
			{
				if (!model || model.room == null || !model.room.IsUsingGhosts)
				{
					break;
				}
				OpponentModel.Status status = (OpponentModel.Status)p_data[0];
				float b = (float)p_data[1];
				switch (status)
				{
				case OpponentModel.Status.Error:
					view.roomStatusField.SetWarning("LOADING FAILED!");
					view.roomStatusField.fade.FadeOut(0.2f, 0.5f);
					base.app.view.audio.PlayUIGenericError();
					break;
				case OpponentModel.Status.NoResults:
					view.roomStatusField.SetWarning("NO OPPONENTS FOUND!");
					break;
				case OpponentModel.Status.Progress:
					view.roomStatusField.SetLoading(Mathf.Min(0.99f, b));
					break;
				case OpponentModel.Status.Complete:
					view.roomStatusField.SetLoading(1f);
					view.roomStatusField.SetWarning("WAITING FOR PLAYERS...");
					break;
				case OpponentModel.Status.ManifestSuccess:
					if (!model.room.UsingCustomMap)
					{
						view.roomStatusField.SetLoading(0f);
						base.app.view.audio.PlayUIGenericSuccess();
					}
					break;
				case OpponentModel.Status.ByPass:
					break;
				}
				break;
			}
			case "network.ghosts.update-ui":
				view.startButton.interactable = false;
				this.TimerRunOnce(delegate
				{
					if (base.validContext && view.current)
					{
						view.startButton.interactable = true;
					}
				}, 7f);
				break;
			case "network.player.master@click":
			{
				if (!view.room.IsMaster || p_data.Length == 0)
				{
					break;
				}
				UIMultiplayerRoomItemView uIMultiplayerRoomItemView = (UIMultiplayerRoomItemView)p_data[0];
				if (!uIMultiplayerRoomItemView.data.IsMaster)
				{
					try
					{
						model.TrySetMaster((NetworkActor)uIMultiplayerRoomItemView.data);
					}
					catch (Exception ex)
					{
						Debug.LogError("Couldn't assign new master. " + uIMultiplayerRoomItemView.data.ID + "\n" + ex.Message);
					}
					uIMultiplayerRoomItemView.CloseMenu();
				}
				break;
			}
			case "tournament.action.reset-heat":
			case "tournament.action.reset-match":
				if (base.validContext && model.room != null && model.room.GameMode == NetworkRoom.GameType.Tournament && p_data.Length != 0)
				{
					string text = (string)p_data[0];
					if (!string.IsNullOrEmpty(text) && !(model.room.MatchId != text))
					{
						base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
					}
				}
				break;
			case "tournament.action.refresh":
			{
				if (!base.validContext || model.room == null || model.room.GameMode != NetworkRoom.GameType.Tournament || string.IsNullOrEmpty(model.room.MatchId) || !base.app.inTournament)
				{
					break;
				}
				DRLTournamentData td = base.app.model.tournament.tournament;
				if (td == null)
				{
					break;
				}
				DRLTournamentRoundData rd = td.GetActiveRound();
				if (rd == null)
				{
					base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
					break;
				}
				base.app.model.tournament.RefreshMatchData(model.room.MatchId, delegate(DRLTournamentMatchData p_result)
				{
					if (base.validContext && model.room != null && p_result != null)
					{
						UISecondaryHeaderView headerSecondary = base.app.view.ui.headerSecondary;
						bool p_is_under_review = p_result?.isUnderReview ?? false;
						if (headerSecondary != null)
						{
							headerSecondary.Refresh(view, p_is_under_review);
						}
						if (td.status != TournamentState.active || rd.state != TournamentRoundState.active || (p_result.state != TournamentMatchState.active && p_result.state != TournamentMatchState.waiting))
						{
							base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
						}
						Debug.Log("UIMultiplayerController> Refresh Event! Tournament state: " + td.status.ToString() + " round state " + rd.state.ToString() + "match state " + p_result.state);
					}
				});
				break;
			}
			case "multiplayer.room.form.event@click":
				OnFormNotification(p_target, p_is_change: false, p_event, p_data);
				break;
			case "multiplayer.room.form.event@change":
				OnFormNotification(p_target, p_is_change: true, p_event, p_data);
				break;
			case "multiplayer.room.form.event@end-edit":
				OnFormNotification(p_target, p_is_change: false, p_event, p_data);
				break;
			case "multiplayer.room.map-vote.item@click":
				this.TimerRunOnce(delegate
				{
					UINavigation.Focus(view.voteOptionsDropdown.nav);
				}, 0.3f);
				break;
			case "multiplayer.room.invite@click":
				model.SendGameInvite();
				view.inviteButton.interactable = false;
				this.TimerRunOnce(delegate
				{
					if (!(this == null))
					{
						view.inviteButton.interactable = true;
					}
				}, 60f);
				break;
			case "ui.screen.return@click":
				ReturnScreen();
				break;
			}
		}

		private IEnumerator DisableInput()
		{
			RCI.LockInput(l: true);
			yield return new WaitForSeconds(90f);
			RCI.LockInput(l: false);
		}

		private void SetupInitialDefaultDroneAndSpecificClass()
		{
			NetworkModel network = base.app.model.network;
			network.fetchingBots = true;
			model.room.DroneClass = 101;
			view.selectedDroneClass = 101;
			GarageStateModel garage = base.app.model.storage.state.player.garage;
			DroneRigData droneRigData = garage.GetOriginalByGUID("DRD-fc5bf84d13e5bac67957921c");
			if (droneRigData == null)
			{
				Debug.LogWarning("UIMultiplayerRoomController> OnNotification / Couldn't find default initial drone guid:DRD-fc5bf84d13e5bac67957921c}");
				droneRigData = garage.GetFirstOriginalRigWithDiameter(7);
				if (droneRigData == null)
				{
					Debug.LogWarning("UIMultiplayerRoomController> OnNotification / Couldn't get a fallback drone");
				}
			}
			garage.currentRigData = droneRigData;
			base.app.model.network.room.Local.DroneRigData = ((droneRigData == null) ? "" : droneRigData.ToJson());
			view.SetSpecificDroneData(droneRigData);
			network.fetchingBots = false;
		}

		public void ReturnScreen()
		{
			if (m_isMapLoading || !base.validContext)
			{
				return;
			}
			UIScreenManagerView screens = base.app.view.ui.screens;
			UIScreenManager uIScreenManager = (screens ? screens.manager : null);
			int num = (uIScreenManager ? uIScreenManager.history.Count : 0);
			bool inGame = base.app.inGame;
			if (!screens)
			{
				Debug.Log("UIMultiplayerRoomController> ReturnScreen / ScreenManagerView is <null>");
			}
			if (!uIScreenManager)
			{
				Debug.Log("UIMultiplayerRoomController> ReturnScreen / ScreenManager is <null>");
			}
			if (view.leaveRoomOnExit && !base.app.inTournament)
			{
				Debug.Log("UIMultiplayerRoomController> ReturnScreen / Leave Room on Exit");
				if ((bool)model)
				{
					model.LeaveRoom();
				}
				if (inGame)
				{
					base.app.controller.game.Exit();
					return;
				}
			}
			if (num != 1)
			{
				if ((bool)screens)
				{
					screens.Return(1);
				}
				else if (uIScreenManager != null)
				{
					uIScreenManager.Close("multiplayer-room-screen");
				}
			}
			if (!inGame)
			{
				Debug.Log("UIMultiplayerRoomController> ReturnScreen / Not ingame, skipping input/hud");
				return;
			}
			GameController game = base.app.controller.game;
			GameInputController gameInputController = (game ? game.input : null);
			GameTypeController gameTypeController = (gameInputController ? gameInputController.controller : null);
			UIHUD uIHUD = ((!game) ? null : (game.ui ? game.ui.hud : null));
			if (!game)
			{
				Debug.Log("UIMultiplayerRoomController> ReturnScreen / GameController is <null>");
			}
			if (!gameInputController)
			{
				Debug.Log("UIMultiplayerRoomController> ReturnScreen / GameInputController is <null>");
			}
			if (!gameTypeController)
			{
				Debug.Log("UIMultiplayerRoomController> ReturnScreen / GameTypeController is <null>");
			}
			if (!uIHUD)
			{
				Debug.Log("UIMultiplayerRoomController> ReturnScreen / HUD is <null>");
			}
			if ((bool)uIHUD)
			{
				uIHUD.Show(0f);
			}
			if (!(gameTypeController is NetworkRaceController) && (bool)gameTypeController)
			{
				gameTypeController.Pause(p_flag: false, p_pause_physics: false, p_open_pause_screen: false);
			}
		}

		private bool HasNewMaster()
		{
			UIMultiplayerRoomItemView uIMaster = view.GetUIMaster();
			if (uIMaster == null || uIMaster.data == null || view.room?.Master == null)
			{
				return false;
			}
			return uIMaster.data != view.room.Master;
		}

		protected void RefreshRoom(bool p_force = false)
		{
			if (!base.validContext || model.room == null)
			{
				return;
			}
			EnableSwapMode(null);
			AssertDefaultParameters();
			view.Set(model.room, p_force);
			string localisedGameMode = model.room.GameMode.ToString();
			switch (model.room.GameMode)
			{
			case NetworkRoom.GameType.Freestyle:
				localisedGameMode = base.app.model.storage.locale.Get("multiplayer.multiplayer-room-screen.game-modes.freestyle", model.room.GameMode.ToString());
				break;
			case NetworkRoom.GameType.Race:
				localisedGameMode = base.app.model.storage.locale.Get("multiplayer.multiplayer-room-screen.game-modes.race", model.room.GameMode.ToString());
				break;
			}
			string header_title = ((model.room == null) ? "NULL ROOM" : model.room.RoomTitle.ToUpper());
			base.app.model.service.platform.TextValidate(header_title, delegate(bool p_result, string p_value)
			{
				if (base.validContext)
				{
					header_title = (p_result ? p_value : "ROOM");
					header_title = localisedGameMode.ToUpper() + " " + header_title;
					if ((bool)view && (bool)view.screen)
					{
						view.screen.title = header_title;
					}
					if ((bool)base.app.view && (bool)base.app.view.ui && (bool)base.app.view.ui.header)
					{
						base.app.view.ui.header.Refresh();
					}
				}
			});
			if (view.isMaster)
			{
				while (view.room.RacersCount > view.room.MaxRacers)
				{
					Debug.LogWarning("UIMultiplayerRoomController > Room is full, switching to spectate the last player");
					view.room.TrySwitchToSpectator(view.room.lastPlayers[view.room.lastPlayers.Count - 1], forced: true);
				}
				while (view.room.RacersCount > view.room.MaxRacers)
				{
					Debug.LogWarning("UIMultiplayerRoomController > Room is full, removing last player");
					view.room.OnPlayerLeft(view.room.lastPlayers[view.room.lastPlayers.Count - 1].RawData);
				}
			}
			ListComponent racerGridField = view.racerGridField;
			List<UIMultiplayerRoomItemView> it = new List<UIMultiplayerRoomItemView>();
			for (int num = 0; num < racerGridField.Count; num++)
			{
				it.Add(racerGridField.Get<UIMultiplayerRoomItemView>(num));
			}
			Activity.RunOnce(delegate
			{
				for (int i = 0; i < it.Count; i++)
				{
					if (it[i].droneImageAlpha > 0f && !it[i].isBot && it[i].data == null)
					{
						view.Clear();
						Notify("network.player@update");
						Debug.LogWarning("UIMultiplayerRoomController > Null player found, clearing room");
					}
				}
			}, 0.2f);
			if (UINavigation.focus == null)
			{
				UINavigation.Focus(view.backButton.GetComponent<UINavigation>());
			}
		}

		protected void RefreshTournament()
		{
			if (model.room != null && model.room.IsTournamentMatch)
			{
				view.tournamentHeatField.index = model.room.HeatIdx;
				view.tournamentHeatField.Refresh();
				view.EnableTournamentControls(p_enable: false);
				view.tournamentHeatField.max = model.room.MaxHeats;
				view.tournamentHeatField.Refresh();
				view.SetTournamentActive(p_flag: true);
				UINavigation focus = UINavigation.focus;
				if (!focus || !focus.transform.IsChildOf(view.tournamentGUIDField.transform))
				{
					view.tournamentGUIDField.field.text = model.room.TournamentId;
				}
			}
		}

		protected void RefreshTournament(bool p_force)
		{
			if (model.room == null)
			{
				return;
			}
			view.tournamentHeatField.index = model.room.HeatIdx;
			view.tournamentHeatField.Refresh();
			view.EnableTournamentControls(view.isMaster);
			UINavigation focus = UINavigation.focus;
			if (!focus || !focus.transform.IsChildOf(view.tournamentGUIDField.transform))
			{
				view.tournamentGUIDField.field.text = model.room.TournamentId;
			}
			if (!(m_prevTournamentId != model.room.TournamentId || p_force))
			{
				return;
			}
			base.app.arguments.game.tournamentLegacy = null;
			view.SetTournamentActive(p_flag: false);
			if (m_tournamentLoader != null)
			{
				m_tournamentLoader.Stop();
			}
			m_tournamentLoader = RunOnce(delegate
			{
				if (!(this == null) && model.room != null)
				{
					m_prevTournamentId = model.room.TournamentId;
					base.app.model.service.GetTournamentsLegacy(model.room.TournamentId, delegate(DRLTournamentLegacyData[] p_tournaments)
					{
						if (!(this == null) && model.room != null)
						{
							if (string.IsNullOrEmpty(model.room.TournamentId) || p_tournaments.Length == 0)
							{
								base.app.view.ui.screens.manager.GetComponent<UIScreenManagerController>().ValidatePromo();
								Debug.Log("UIMultiplayerRoomController> No Tournaments found [" + model.room.TournamentId + "]");
							}
							else
							{
								Debug.Log("UIMultiplayerRoomController> Tournament [" + model.room.TournamentId + "] Found!\n" + p_tournaments[0].ToJson(p_indented: true));
								DRLTournamentLegacyData dRLTournamentLegacyData = p_tournaments[0];
								base.app.arguments.game.tournamentLegacy = dRLTournamentLegacyData;
								view.tournamentHeatField.max = dRLTournamentLegacyData.heats;
								view.tournamentHeatField.Refresh();
								view.SetTournamentActive(p_flag: true);
								view.EnableTournamentControls(view.isMaster);
								base.app.view.ui.screens.manager.GetComponent<UIScreenManagerController>().ValidatePromo();
							}
						}
					});
				}
			}, 1f);
		}

		protected string GetDefaultRoomName()
		{
			if (m_time < 0)
			{
				m_time = (int)(Time.time * 1000f);
			}
			string text = m_time.ToString("X");
			string text2 = base.app.model.storage.locale.Get("multiplayer.default-room-name", "ROOM");
			string text3 = base.app.model.storage.state.player.profile.username.ToUpper();
			return text3 + "-" + text2 + "-" + text;
		}

		public UIMultiplayerRoomController(Activity p_lockdown_poll)
		{
			m_lockdownPoll = p_lockdown_poll;
		}

		protected void AssertDefaultParameters()
		{
			if (model.room == null || !model.room.IsMaster)
			{
				return;
			}
			if (string.IsNullOrEmpty(model.room.RoomTitle))
			{
				string defaultRoomName = GetDefaultRoomName();
				view.roomNameInput.field.text = defaultRoomName;
				if (model.room.RoomTitle != defaultRoomName)
				{
					model.room.RoomTitle = defaultRoomName;
				}
			}
			if (view.gameType == GameFlag.Campaign || view.IsRandomMapMode())
			{
				return;
			}
			bool num = !string.IsNullOrEmpty(model.room.CustomMapId);
			if (string.IsNullOrEmpty(model.room.MapId) && model.room.MapId != view.mapGUID)
			{
				model.room.MapId = view.mapGUID;
			}
			if (num)
			{
				if (!string.IsNullOrEmpty(model.room.CustomMapId) && model.room.TrackId != string.Empty)
				{
					model.room.TrackId = string.Empty;
				}
			}
			else if (string.IsNullOrEmpty(model.room.TrackId) && model.room.TrackId != view.trackGUID)
			{
				model.room.TrackId = view.trackGUID;
			}
		}

		private void DelayUIElement(UIElementView p_ui_element, Action<UnityEngine.Object> p_delayed_actions)
		{
			if (m_delayActivities == null || p_ui_element == null)
			{
				return;
			}
			if (m_delayActivities.ContainsKey(p_ui_element))
			{
				m_delayActivities[p_ui_element].Stop();
				m_delayActivities[p_ui_element] = null;
			}
			else
			{
				m_delayActivities.Add(p_ui_element, null);
			}
			m_delayActivities[p_ui_element] = Run(delegate(float p_progress)
			{
				if (p_progress > stepperDelayTime)
				{
					p_delayed_actions(p_ui_element);
					return false;
				}
				return true;
			});
		}

		protected void OnFormNotification(UnityEngine.Object p_target, bool p_is_change, string p_event, object[] p_data)
		{
			bool flag = p_is_change;
			bool flag2 = p_event.Contains("@end-edit");
			string text = p_target.name;
			if (text == null)
			{
				return;
			}
			switch (text)
			{
			case "game-start":
				if (model.room.IsMaster)
				{
					model.photon.ForceStartMatch();
				}
				break;
			case "game-tournament":
			{
				DRLTournamentLegacyData tournamentLegacy = base.app.arguments.game.tournamentLegacy;
				if (tournamentLegacy != null)
				{
					UIGameTournamentOverviewView uIGameTournamentOverviewView = base.app.view.ui.screens.Open<UIGameTournamentOverviewView>("game-tournament-overview-screen");
					uIGameTournamentOverviewView.nextButton.gameObject.SetActive(value: false);
					uIGameTournamentOverviewView.replayButton.gameObject.SetActive(value: false);
					uIGameTournamentOverviewView.data = tournamentLegacy;
				}
				break;
			}
			case "ready-toggle":
			{
				DRLToggleView dRLToggleView = p_target as DRLToggleView;
				if ((bool)dRLToggleView)
				{
					model.SetRoomReady(dRLToggleView.toggle.isOn);
				}
				view.RefreshReadyToggle();
				break;
			}
			case "player-state":
				if (model.room.Local.IsSpectator)
				{
					model.room.TrySwitchToRacer(model.room.Local);
				}
				else
				{
					model.room.TrySwitchToSpectator(model.room.Local);
				}
				break;
			case "room-name":
				if (flag2)
				{
					string text2 = ((p_data.Length == 0) ? "ROOM" : ((string)p_data[0]));
					text2 = text2.Replace(" ", "");
					if (string.IsNullOrEmpty(text2))
					{
						text2 = GetDefaultRoomName();
					}
					model.room.RoomTitle = text2;
				}
				break;
			case "room-password":
				if (flag2)
				{
					DRLInputFieldView dRLInputFieldView = p_target as DRLInputFieldView;
					if (!string.IsNullOrEmpty(dRLInputFieldView.field.text))
					{
						model.room.Password = dRLInputFieldView.field.text;
					}
				}
				break;
			case "max-racers":
				if (!flag)
				{
					break;
				}
				DelayUIElement((UIElementView)p_target, delegate(UnityEngine.Object p_stepper)
				{
					if (model.room != null)
					{
						model.room.MaxRacers = ((DRLStepperView)p_stepper).index + 2;
					}
				});
				break;
			case "max-spectators":
				if (!flag)
				{
					break;
				}
				DelayUIElement((UIElementView)p_target, delegate(UnityEngine.Object p_stepper)
				{
					if (model.room != null)
					{
						model.room.MaxSpectators = ((DRLStepperView)p_stepper).index;
					}
				});
				break;
			case "ghosts-allowed":
				if (flag && model.room != null)
				{
					model.room.AllowGhosts = view.allowGhosts;
				}
				break;
			case "room-privacy":
			{
				if (!flag)
				{
					break;
				}
				DRLStepperView dRLStepperView4 = p_target as DRLStepperView;
				if (!(dRLStepperView4 == null))
				{
					model.room.IsPrivate = dRLStepperView4.index == 1;
					if (dRLStepperView4.index == 1)
					{
						view.roomPasswordInput.gameObject.SetActive(value: true);
						view.privacyRoomIcon.SetActive(value: true);
						view.inviteButton.gameObject.SetActive(value: false);
					}
					else
					{
						view.roomPasswordInput.text = "";
						view.roomPasswordInput.gameObject.SetActive(value: false);
						view.privacyRoomIcon.SetActive(value: false);
						view.inviteButton.gameObject.SetActive(view.userCommunicationAllowed);
						model.room.Password = "";
					}
				}
				break;
			}
			case "vote-options":
				if (flag && model.room != null)
				{
					model.room.MapVotingCategory = (MapCategory)view.voteOptionsDropdown.dropdown.value;
					if (model.room.IsMaster && model.room.MapVotingCategory == MapCategory.Random)
					{
						view.AssignRandomMap();
					}
				}
				break;
			case "arm-and-turtle":
				if (flag && model.room != null)
				{
					model.room.ArmAndTurtle = view.armAndTurtleDropdown.dropdown.value == 1;
				}
				break;
			case "campaign":
				if (flag)
				{
					DRLStepperView dRLStepperView = p_target as DRLStepperView;
					view.PopulateCampaigns(dRLStepperView.index);
				}
				break;
			case "map":
			{
				UIMapsCategoryView uIMapsCategoryView = base.app.view.ui.screens.Open<UIMapsCategoryView>("maps-category-screen");
				uIMapsCategoryView.screen.title = base.app.model.storage.locale.Get("maps.choose-map", "Choose Map");
				uIMapsCategoryView.caller = this;
				base.app.arguments.Clear();
				base.app.arguments.game.type = view.gameType;
				base.app.arguments.game.mode = GameFlag.NetworkMultiplayer;
				break;
			}
			case "race-timeout":
				if (flag && model.room != null)
				{
					model.room.TimeoutMode = (TimeoutMode)view.timeoutModeDropdown.dropdown.value;
				}
				break;
			case "drone-class":
				if (flag)
				{
					DRLStepperView dRLStepperView3 = p_target as DRLStepperView;
					if (!(dRLStepperView3 == null))
					{
						view.droneClassLabel.SetActive(dRLStepperView3.index != 6);
						SelectDroneClass();
						DelayUIElement((UIElementView)p_target, DroneClassStepperAction);
					}
				}
				break;
			case "drl-pilot-mode":
				if (flag)
				{
					bool isOn = view.drlPilotModeToggle.isOn;
					model.room.DRLPilotMode = isOn;
					view.droneClassSelector.index = 6;
					view.droneClassSelector.Refresh();
					view.droneSelector.interactable = !isOn && view.droneSelector.interactable;
					view.droneClassSelector.interactable = !isOn;
					view.droneClassLabel.SetActive(view.droneClassSelector.index != 6);
					SelectDroneClass();
					DelayUIElement(view.droneClassSelector, DroneClassStepperAction);
				}
				break;
			case "drone-selection":
			{
				if ((view.track != null && view.track.promoDrones != null && view.track.promoDrones.Length == 1 && view.track.promoDronesOnly) || (view.map != null && view.map.promoDrones != null && view.map.promoDrones.Length == 1 && view.map.promoDronesOnly))
				{
					break;
				}
				UIGarageRigSelectionView uIGarageRigSelectionView = base.app.view.ui.screens.Open<UIGarageRigSelectionView>("garage-rig-selection-screen");
				uIGarageRigSelectionView.screen.title = base.app.model.storage.locale.Get("multiplayer.select-drone-screen.title", "Select your Drone");
				if (view.selectedDroneClass == 100 || view.selectedDroneClass == 101)
				{
					uIGarageRigSelectionView.overrideSizes = null;
				}
				else
				{
					uIGarageRigSelectionView.overrideSizes = new List<int>();
					uIGarageRigSelectionView.overrideSizes.Add((view.selectedDroneClass < 100) ? view.selectedDroneClass : 0);
				}
				uIGarageRigSelectionView.allowCustomPhysics = view.room.GameMode == NetworkRoom.GameType.Freestyle;
				uIGarageRigSelectionView.SetCreationEnabled(p_flag: false);
				uIGarageRigSelectionView.selectionOnly = true;
				uIGarageRigSelectionView.promoList = null;
				uIGarageRigSelectionView.overrideList = null;
				if (!(view.track != null))
				{
					break;
				}
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
				break;
			}
			case "tournament-id":
				if (flag2)
				{
					DRLInputFieldView dRLInputFieldView2 = p_target as DRLInputFieldView;
					Debug.Log("tournament-id[" + dRLInputFieldView2.field.text + "]");
					model.room.TournamentId = dRLInputFieldView2.field.text;
				}
				break;
			case "tournament-heat":
				if (flag)
				{
					DRLStepperView dRLStepperView2 = p_target as DRLStepperView;
					Debug.Log("tournament-heat[" + dRLStepperView2.index + "]");
					model.room.HeatIdx = dRLStepperView2.index;
				}
				break;
			case "nav-exit":
				if (model.room == null || model.room.State != NetworkRoom.StateCode.MatchLocked)
				{
					Notify("game.pause.exit@click");
					base.app.view.audio.PlayUIGenericSuccess();
					if (base.app.inGame)
					{
						base.app.controller.game.ui.hud.timeout.StopTimeout();
					}
					if (base.app.arguments.game.tournamentData == null)
					{
						base.app.controller.game.Exit();
					}
					else
					{
						base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
					}
				}
				break;
			case "nav-settings":
				if (model.room == null || model.room.State != NetworkRoom.StateCode.MatchLocked)
				{
					base.app.view.ui.screens.Open<UISettingsView>("settings-screen");
				}
				break;
			}
		}

		private void SelectDroneClass()
		{
			if (view.droneClassSelector.index == 0 || view.droneClassSelector.index > 6)
			{
				view.selectedDroneClass = 100;
			}
			else if (view.droneClassSelector.index == 6)
			{
				view.selectedDroneClass = 101;
			}
			else
			{
				view.selectedDroneClass = view.droneClassSelector.index + 2;
			}
		}

		private void DroneClassStepperAction(UnityEngine.Object p_ui_element)
		{
			DRLStepperView dRLStepperView = (DRLStepperView)p_ui_element;
			if (model.room == null)
			{
				return;
			}
			if (model.room.IsMaster)
			{
				model.room.DroneClass = view.selectedDroneClass;
				if (model.room.DRLPilotMode)
				{
					model.room.SelectedDrone = base.app.model.storage.state.player.garage.SetOfficialRig().guid;
				}
			}
			if (dRLStepperView.index == 0 || dRLStepperView.index > 6)
			{
				view.UpdateDrone();
				if (dRLStepperView.index > 6)
				{
					Debug.LogError("UIMultiplayerRoomController> Unknown drone class stepper index: " + dRLStepperView.index);
				}
			}
			else if (dRLStepperView.index == 6)
			{
				view.UpdateDrone();
			}
			else
			{
				view.UpdateDrone();
			}
		}
	}
}
