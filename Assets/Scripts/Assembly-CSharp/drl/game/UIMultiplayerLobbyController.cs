using System;
using System.Collections.Generic;
using UnityEngine;
using drl.network;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMultiplayerLobbyController : Controller<DRLApp>
	{
		public int pageLength = 10;

		private MonoActivity m_lobby_connect_timer;

		private MonoActivity m_search_timer;

		private bool m_in_quick_match_flow;

		private bool m_lock_ui;

		private Activity m_disconnect_retry;

		private UINavigation lastNavItem;

		private Activity m_connect_timeout_loop;

		public NetworkModel model => base.app.model.network;

		public UIMultiplayerLobbyView view => AssertLocal<UIMultiplayerLobbyView>("view");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "ui.screen@close")
			{
				base.app.view.audio.StopUILoadingLoop();
				ClearConnectTimeout();
				m_lock_ui = false;
			}
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "network.LAN.online":
				break;
			case "network.LAN.offline":
				break;
			case "multiplayer.lobby.item.entry@click":
				break;
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					m_lock_ui = false;
					base.app.view.ui.screenBack = true;
					view.SetInQuickMatchMode(p_inQuickMatch: false);
					m_in_quick_match_flow = false;
					view.topContainer.SetActive(value: false);
					bool flag = base.app.model.network.connected || base.app.model.network.IsConnectedToLAN;
					view.Clear();
					CloudRegionCode cloudRegionCode = (CloudRegionCode)(-1);
					CloudRegionCode cloudRegionCode2 = (base.app.model.network.IsConnectedToLAN ? cloudRegionCode : base.app.model.storage.state.player.selectedNetworkRegion);
					if (cloudRegionCode2 > CloudRegionCode.asia)
					{
						cloudRegionCode2 = CloudRegionCode.none;
					}
					Debug.Log($"UIMultiplayerLobbyController> ScreenOpen / connected[{flag}] lan-connected[{model.IsConnectedToLAN}] server-ip[{model.ServerIP}] local-ip[{model.LocalIPAddress}] region-name[{base.app.model.network.regionName}] region-code[{cloudRegionCode2}] detailed-state[{PhotonNetwork.connectionStateDetailed}] state[{model.connectionState}]");
					ClientState connectionStateDetailed = PhotonNetwork.connectionStateDetailed;
					if (connectionStateDetailed == ClientState.ConnectingToMasterserver || connectionStateDetailed == ClientState.ConnectingToNameServer)
					{
						m_lock_ui = true;
						view.Clear();
						view.SetFeedback(UILobbyFeedbackType.Connecting, p_hide_list: true);
						base.app.view.audio.PlayUILoadingLoop();
						StartConnectTimeout();
					}
					else if (!flag)
					{
						SetServer((int)cloudRegionCode2, p_connect: false);
						ConnectToServer();
					}
					else
					{
						view.Clear();
						SetServer((int)cloudRegionCode2);
						view.UpdateList(model.lobby, 0, pageLength);
					}
					Notify("network.lobby.server-list@enable");
				}
				break;
			case "network.connection@start":
				Debug.Log("UIMultiplayerLobbyController> ConnectionStart");
				m_lock_ui = true;
				view.Clear();
				view.SetFeedback(UILobbyFeedbackType.Connecting, p_hide_list: true);
				base.app.view.audio.PlayUILoadingLoop();
				StartConnectTimeout();
				break;
			case "network.connection@complete":
				Debug.Log("UIMultiplayerLobbyController> ConnectionComplete");
				view.Clear();
				base.app.view.audio.StopUILoadingLoop();
				m_lock_ui = false;
				ClearConnectTimeout();
				break;
			case "network.room-create@error":
			case "network.room-enter@error":
				Debug.Log("UIMultiplayerLobbyController> Room creation error!");
				view.SetFeedback(UILobbyFeedbackType.OperationFailure, p_hide_list: true);
				base.app.view.audio.StopUILoadingLoop();
				break;
			case "network.disconnect":
			{
				ClearConnectTimeout();
				PhotonService.DisconnectionCode disconnectionCode = (PhotonService.DisconnectionCode)p_data[0];
				Debug.Log($"UIMultiplayerLobbyController> Disconnected / reason[{disconnectionCode}]");
				switch (disconnectionCode)
				{
				case PhotonService.DisconnectionCode.Timeout:
					view.SetFeedback(UILobbyFeedbackType.TimeOut, p_hide_list: true);
					break;
				case PhotonService.DisconnectionCode.ByServerLogic:
				case PhotonService.DisconnectionCode.Exception:
				case PhotonService.DisconnectionCode.Unknown:
					view.SetFeedback(UILobbyFeedbackType.ServerError, p_hide_list: true);
					break;
				}
				if (m_disconnect_retry != null)
				{
					m_disconnect_retry.Stop();
				}
				m_disconnect_retry = Activity.RunOnce(delegate
				{
					m_disconnect_retry = null;
					if (!base.app.model.network.connected && view.screen.open && base.app.model.network.connectionState == PhotonService.ServiceState.Disconnected)
					{
						ConnectToServer();
					}
				}, 10f);
				base.app.view.audio.StopUILoadingLoop();
				m_lock_ui = false;
				break;
			}
			case "network.lobby@enter":
				m_lock_ui = false;
				ClearConnectTimeout();
				Debug.Log("UIMultiplayerLobbyController> LobbyEnter - region[" + model.regionName + "]");
				base.app.view.audio.StopUILoadingLoop();
				base.app.view.audio.PlayUILoadingSuccess();
				view.SetFeedback(UILobbyFeedbackType.None);
				RefreshLobbyTitleAndFooter(model.regionName);
				base.app.model.storage.state.player.connectedNetworkRegion = model.region;
				base.app.view.ui.header.Refresh();
				break;
			case "network.lobby.room-list":
			{
				List<Lobby.NetworkRoomInfo> list = (List<Lobby.NetworkRoomInfo>)p_data[0];
				Debug.Log("UIMultiplayerLobbyController> LobbyRoomList - total[" + list.Count + "]");
				if (!m_in_quick_match_flow)
				{
					view.UpdateList(list, view.pageField.index, pageLength);
				}
				break;
			}
			case "network.room@enter":
				m_lock_ui = false;
				if (model.room.IsQuickMatch)
				{
					m_in_quick_match_flow = true;
					bool flag3 = model.room.GameMode == NetworkRoom.GameType.Race;
					DroneRigData currentRigData = base.app.model.storage.state.player.garage.currentRigData;
					model.room.Local.DroneRigData = ((currentRigData == null) ? "" : currentRigData.ToJson());
					if (string.IsNullOrEmpty(model.room.RoomTitle))
					{
						int num = Mathf.Abs(model.room.GetHashCode()) % 1000;
						string text3 = (flag3 ? view.quickMatchQuickRaceLabel.text : view.quickMatchQuickFreestyleLabel.text);
						text3 = text3 + "-" + base.app.model.storage.state.player.profile.username.ToUpper() + num.ToString("000");
						model.room.RoomTitle = text3;
					}
					view.SetInQuickMatchMode(p_inQuickMatch: true, flag3);
					view.RefreshQuickMatchFeedback(model.room.RacersCount, model.room.MaxRacers, model.room.LobbyCountdown);
					Notify("network.lobby.server-list@disable");
				}
				break;
			case "network.room.update":
				if ((bool)model && model.room != null && model.room.IsQuickMatch)
				{
					view.RefreshQuickMatchFeedback(model.room.RacersCount, model.room.MaxRacers, model.room.LobbyCountdown);
				}
				break;
			case "multiplayer.lobby.form.event@click":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "multiplayer.lobby.form.event@change":
				OnFormNotification(p_target, p_is_change: true, p_event);
				break;
			case "multiplayer.lobby.form.event@end-edit":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "multiplayer.lobby.page@select":
			{
				int p_page = (int)p_data[0];
				Debug.Log("UIMultiplayerLobbyController> Page Select [" + p_page + "]");
				view.UpdateList(model.rooms, p_page, pageLength);
				break;
			}
			case "multiplayer.lobby.page.previous@click":
				if (view.pageField.index != 0)
				{
					view.pageField.index = view.pageField.index - 1;
					Debug.Log("UIMultiplayerLobbyController> Page Select [" + view.pageField.index + "]");
					view.UpdateList(model.rooms, view.pageField.index, pageLength);
				}
				break;
			case "multiplayer.lobby.page.next@click":
				if (view.pageField.index + 1 != view.pageField.listField.Count)
				{
					view.pageField.index = view.pageField.index + 1;
					Debug.Log("UIMultiplayerLobbyController> Page Select [" + view.pageField.index + "]");
					view.UpdateList(model.rooms, view.pageField.index, pageLength);
				}
				break;
			case "multiplayer.lobby.item.action@click":
			{
				Component component4 = p_target as Component;
				if (!component4)
				{
					break;
				}
				UIMultiplayerLobbyItemView uIMultiplayerLobbyItemView2 = Hierarchy.FindReverse<UIMultiplayerLobbyItemView>(component4.transform);
				Lobby.NetworkRoomInfo data = uIMultiplayerLobbyItemView2.data;
				Debug.Log("UIMultiplayerLobbyController> Action Click [" + uIMultiplayerLobbyItemView2?.ToString() + "] is-full[" + data.IsFull + "] in-game[" + data.InGame + "] can-race[" + data.CanRace + "] can-spectate[" + data.CanSpectate + "] is-private[" + data.IsPrivate + "]");
				bool flag2 = (data.CanRace || data.CanSpectate) && model.photon.KickedFromRoom != data.Name;
				if (data.IsFull)
				{
					flag2 = false;
				}
				if (data.InGame)
				{
					flag2 = false;
				}
				if (!data.IsPrivate)
				{
					if (flag2)
					{
						m_lock_ui = true;
						base.app.view.audio.PlayUILoadingLoop();
						view.SetFeedback(UILobbyFeedbackType.Connecting, p_hide_list: false);
						model.JoinRoom(data.Name);
					}
					else
					{
						base.app.view.audio.PlayUIGenericError();
					}
				}
				else
				{
					uIMultiplayerLobbyItemView2.TogglePasswordInput();
				}
				break;
			}
			case "multiplayer.lobby.server-list-item@click":
			{
				Component component2 = p_target as Component;
				if ((bool)component2)
				{
					UIElementView component3 = component2.gameObject.GetComponent<UIElementView>();
					if (component3 != null)
					{
						int p_serverVal = ServerValueFromName(component3.name);
						SetServer(p_serverVal);
					}
				}
				break;
			}
			case "multiplayer.lobby.lan-connect":
				if (p_data.Length != 0)
				{
					string text2 = (string)p_data[0];
					Debug.Log("UIMultiplayerLobbyController> LobbyLANConnect / ip[" + text2 + "]");
					base.app.model.network.ConnectToLAN(text2);
				}
				break;
			case "multiplayer.lobby.lan-disconnect":
				Notify("network.footer@disable");
				Notify("network.lobby.server-list@disable");
				model.Disconnect();
				model.LocalIPAddress = "";
				view.ShowLanControls(p_show: false);
				base.app.view.ui.screens.Return();
				break;
			case "multiplayer.lobby.item.private.join@click":
			{
				Component component = p_target as Component;
				if (component == null)
				{
					break;
				}
				UIMultiplayerLobbyItemView uIMultiplayerLobbyItemView = Hierarchy.FindReverse<UIMultiplayerLobbyItemView>(component.transform);
				if (uIMultiplayerLobbyItemView.data.IsPrivate)
				{
					string text = uIMultiplayerLobbyItemView.privateJoinPasswordInput.text;
					if (string.IsNullOrEmpty(text))
					{
						uIMultiplayerLobbyItemView.TogglePasswordInput();
					}
					else if (uIMultiplayerLobbyItemView.data.Password != text)
					{
						uIMultiplayerLobbyItemView.PulseIncorrectPassword();
						base.app.view.audio.PlayUIGenericError();
					}
				}
				break;
			}
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				Notify("network.footer@disable");
				Notify("network.lobby.server-list@disable");
				if (m_connect_timeout_loop != null)
				{
					m_connect_timeout_loop.Stop();
				}
				m_connect_timeout_loop = null;
				model.Disconnect();
				view.ShowLanControls(p_show: false);
				break;
			}
		}

		private int ServerValueFromName(string p_name)
		{
			int result = 4;
			switch (p_name)
			{
			case "server-europe":
				result = 0;
				break;
			case "server-america":
				result = 1;
				break;
			case "server-asia":
				result = 2;
				break;
			case "server-best":
				result = 4;
				break;
			case "server-local":
				result = -1;
				break;
			}
			return result;
		}

		private void SetServer(int p_serverVal, bool p_connect = true)
		{
			if (m_lobby_connect_timer != null)
			{
				m_lobby_connect_timer.Stop();
			}
			if (p_connect && p_serverVal == -1)
			{
				SetLobbyServer(-1);
				return;
			}
			view.ShowLanControls(p_show: false);
			base.app.model.storage.state.player.selectedNetworkRegion = (CloudRegionCode)p_serverVal;
			if (p_connect)
			{
				m_lobby_connect_timer = RunOnce(delegate
				{
					SetLobbyServer(p_serverVal);
				}, 0.3f);
			}
		}

		protected void ConnectToServer()
		{
			RefreshLobbyTitleAndFooter();
			view.Clear();
			view.SetFeedback(UILobbyFeedbackType.Connecting, p_hide_list: true);
			base.app.view.audio.PlayUILoadingLoop();
			CloudRegionCode selectedNetworkRegion = base.app.model.storage.state.player.selectedNetworkRegion;
			Debug.Log("UIMultiplayerLobbyController> ConnectToServer - region[" + selectedNetworkRegion.ToString() + "]");
			if (selectedNetworkRegion == CloudRegionCode.none)
			{
				base.app.model.network.ConnectToLobby();
			}
			else
			{
				base.app.model.network.SwitchRegion(selectedNetworkRegion);
			}
			StartConnectTimeout();
		}

		protected void ClearConnectTimeout()
		{
			if (m_connect_timeout_loop != null)
			{
				m_connect_timeout_loop.Stop();
			}
			m_connect_timeout_loop = null;
		}

		protected void StartConnectTimeout()
		{
			ClearConnectTimeout();
			float timeout_elapsed = 0f;
			m_connect_timeout_loop = Activity.Run((Func<bool>)delegate
			{
				if (!base.validContext)
				{
					return false;
				}
				if (base.app.model.network.connected)
				{
					m_connect_timeout_loop = null;
					Debug.Log("UIMultiplayerLobbyController> StartConnectTimeout / Connect Success!");
					return false;
				}
				timeout_elapsed += Time.unscaledDeltaTime;
				if (timeout_elapsed < 30f)
				{
					return true;
				}
				base.app.model.network.Disconnect();
				view.SetFeedback(UILobbyFeedbackType.TimeOut, p_hide_list: true);
				base.app.view.audio.StopUILoadingLoop();
				m_connect_timeout_loop = null;
				Debug.Log("UIMultiplayerLobbyController> StartConnectTimeout / TimeOut... Repeating...");
				Activity.RunOnce(ConnectToServer, 4f);
				return false;
			}, 0f, false);
		}

		protected void OnFormNotification(UnityEngine.Object p_target, bool p_is_change, string p_event)
		{
			if (m_lock_ui)
			{
				return;
			}
			bool flag = p_is_change;
			string text = p_target.name;
			if (text == null)
			{
				return;
			}
			switch (text)
			{
			case "server":
				if (flag)
				{
					DRLIntStepperView dRLIntStepperView = p_target as DRLIntStepperView;
					SetServer(dRLIntStepperView.value);
				}
				break;
			case "quick-race":
				Debug.Log("UILobbyController> OnFormNotification - quick-race");
				base.app.model.network.QuickRace(delegate(QuickMatchResult p_result)
				{
					HandleQuickMatchResultState(p_result);
				});
				break;
			case "quick-freestyle":
				Debug.Log("UILobbyController> OnFormNotification - quick-freestyle");
				base.app.model.network.QuickFreestyle(delegate(QuickMatchResult p_result)
				{
					HandleQuickMatchResultState(p_result);
				});
				break;
			case "qm-exit-nav":
				base.app.model.network.LeaveRoom();
				view.SetInQuickMatchMode(p_inQuickMatch: false);
				m_in_quick_match_flow = false;
				break;
			case "create-room":
			{
				Debug.Log("UILobbyController> OnFormNotification - create-room - type[" + view.gameType.ToString() + "]");
				m_lock_ui = true;
				base.app.view.audio.PlayUILoadingLoop();
				view.SetFeedback(UILobbyFeedbackType.CreatingRoom, p_hide_list: false);
				GameFlag gameType = view.gameType;
				model.CreateRoom(gameType);
				break;
			}
			case "search-room":
				if (flag)
				{
					DRLInputFieldView dRLInputFieldView2 = p_target as DRLInputFieldView;
					Debug.Log("UILobbyController> OnFormNotification - search[" + dRLInputFieldView2.field.text + "]");
					if (m_search_timer != null)
					{
						m_search_timer.Stop();
					}
					m_search_timer = RunOnce(delegate
					{
						view.Clear();
						view.UpdateList(model.rooms, 0, pageLength);
					}, 0.2f);
				}
				break;
			case "lan-create-ip":
				break;
			case "create-server":
				lastNavItem = view.lanCreateServerBtn.GetComponent<UINavigation>();
				Debug.Log("UILobbyController> OnFormNotification - create-server");
				view.listField.Clear();
				view.SetFeedback(UILobbyFeedbackType.CreatingServer);
				model.Disconnect();
				break;
			case "stop-server":
				lastNavItem = view.lanStopServerBtn.GetComponent<UINavigation>();
				view.SetFeedback(UILobbyFeedbackType.StoppingServer);
				model.Disconnect();
				model.StopLANServer();
				break;
			case "lan-connect":
			{
				lastNavItem = view.lanConnectBtn.GetComponent<UINavigation>();
				Debug.Log("UILobbyController> OnFormNotification - lan-connect");
				view.listField.Clear();
				view.SetFeedback(UILobbyFeedbackType.Connecting);
				RefreshLobbyTitleAndFooter();
				view.Clear();
				view.SetFeedback(UILobbyFeedbackType.Connecting, p_hide_list: true);
				base.app.view.audio.PlayUILoadingLoop();
				string text2 = view.lanCreateIpInput.field.text;
				Debug.Log("UIMultiplayerLobbyController> ConnectToServer - LAN IP[" + text2 + "]");
				base.app.model.network.ConnectToLAN(text2);
				break;
			}
			case "lan-disconnect":
				lastNavItem = view.lanDisconnectBtn.GetComponent<UINavigation>();
				Debug.Log("UILobbyController> OnFormNotification - lan-disconnect");
				view.listField.Clear();
				view.SetFeedback(UILobbyFeedbackType.None);
				model.Disconnect();
				model.LocalIPAddress = "";
				break;
			case "room-password-input":
			{
				if (flag)
				{
					break;
				}
				DRLInputFieldView dRLInputFieldView = p_target as DRLInputFieldView;
				if (dRLInputFieldView == null)
				{
					break;
				}
				UIMultiplayerLobbyItemView uIMultiplayerLobbyItemView = Hierarchy.FindReverse<UIMultiplayerLobbyItemView>(dRLInputFieldView.transform);
				if (uIMultiplayerLobbyItemView.data.IsPrivate)
				{
					if (uIMultiplayerLobbyItemView.data.Password == dRLInputFieldView.text && !string.IsNullOrEmpty(dRLInputFieldView.text))
					{
						Lobby.NetworkRoomInfo data = uIMultiplayerLobbyItemView.data;
						m_lock_ui = true;
						uIMultiplayerLobbyItemView.HidePasswordInput();
						base.app.view.audio.PlayUILoadingLoop();
						view.SetFeedback(UILobbyFeedbackType.Connecting, p_hide_list: false);
						model.JoinRoom(data.Name);
					}
					else if (!string.IsNullOrEmpty(dRLInputFieldView.text))
					{
						uIMultiplayerLobbyItemView.PulseIncorrectPassword();
						base.app.view.audio.PlayUIGenericError();
					}
				}
				break;
			}
			}
		}

		private void HandleQuickMatchResultState(QuickMatchResult p_result)
		{
			if (this == null || view == null || p_result == null)
			{
				return;
			}
			m_in_quick_match_flow = p_result.State != QuickMatchState.Failed;
			switch (p_result.State)
			{
			case QuickMatchState.FindingBestServer:
				view.SetFeedback(UILobbyFeedbackType.Connecting, p_hide_list: true);
				break;
			case QuickMatchState.ConnectedBestServer:
				view.SetFeedback(UILobbyFeedbackType.SearchingMatches, p_hide_list: true);
				break;
			case QuickMatchState.Failed:
				view.SetFeedback(UILobbyFeedbackType.OperationFailure, p_hide_list: false);
				break;
			case QuickMatchState.JoinedRoom:
			{
				bool flag = model.room != null && model.room.GameMode == NetworkRoom.GameType.Race;
				view.SetInQuickMatchMode(p_inQuickMatch: true, flag);
				if (p_result.IsNewRoom)
				{
					view.SetFeedback(UILobbyFeedbackType.WaitingForPlayers, p_hide_list: true);
					if (flag)
					{
						DRLMap randomMap = GetRandomMap(GameFlag.Race);
						if (randomMap == null)
						{
							Debug.LogError("UIMultiplayerLobbyController > QuickFreestyle - No random map found");
						}
						DRLMapTrack randomTrack = GetRandomTrack(randomMap, GameFlag.Race);
						if (randomTrack == null)
						{
							Debug.LogError("UIMultiplayerLobbyController > QuickFreestyle - No random track found");
						}
						p_result.JoinedRoom.MapId = randomMap.guid;
						p_result.JoinedRoom.TrackId = randomTrack.guid;
					}
					else
					{
						DRLMap dRLMap = base.app.model.storage.GetMaps(GameFlag.Freestyle)[0];
						if (dRLMap == null)
						{
							break;
						}
						List<DRLMapTrack> mapTracks = base.app.model.storage.GetMapTracks(dRLMap);
						p_result.JoinedRoom.MapId = dRLMap.guid;
						p_result.JoinedRoom.TrackId = mapTracks[0].guid;
						p_result.JoinedRoom.Local.IsRoomReady = true;
					}
					model.SendGameInvite();
				}
				else
				{
					NetworkRoom joinedRoom2 = p_result.JoinedRoom;
					if (joinedRoom2 != null)
					{
						view.RefreshQuickMatchFeedback(joinedRoom2.RacersCount, joinedRoom2.MaxRacers, joinedRoom2.LobbyCountdown);
					}
				}
				break;
			}
			case QuickMatchState.MatchmakingChanged:
			{
				NetworkRoom joinedRoom = p_result.JoinedRoom;
				if (joinedRoom == null)
				{
					view.SetInQuickMatchMode(p_inQuickMatch: false);
					m_in_quick_match_flow = false;
				}
				else
				{
					bool p_isRace = joinedRoom.GameMode == NetworkRoom.GameType.Race;
					view.SetInQuickMatchMode(p_inQuickMatch: true, p_isRace);
					view.RefreshQuickMatchFeedback(joinedRoom.RacersCount, joinedRoom.MaxRacers, joinedRoom.LobbyCountdown);
				}
				break;
			}
			case QuickMatchState.CreatingRoom:
				break;
			}
		}

		private void RefreshLanControls()
		{
			if (!view.IsLanControlsVisible())
			{
				return;
			}
			PhotonLANServer lanServer = model.LanServer;
			bool isConnectedToLAN = model.IsConnectedToLAN;
			bool running = lanServer.running;
			if (lanServer.supported)
			{
				UINavigation component = view.lanCreateServerBtn.GetComponent<UINavigation>();
				UINavigation component2 = view.lanStopServerBtn.GetComponent<UINavigation>();
				view.ShowLanCreateServerButton(!running);
				view.ShowLanStopServerButton(running);
				if (lastNavItem != null && (lastNavItem == component || lastNavItem == component2))
				{
					UINavigation.Focus(running ? component2 : component);
				}
			}
			else
			{
				view.ShowLanCreateServerButton(p_show: false);
				view.ShowLanStopServerButton(p_show: false);
			}
			view.ShowLanDisconnectButton(isConnectedToLAN);
			view.ShowLanConnectButton(!isConnectedToLAN);
			UINavigation component3 = view.lanConnectBtn.GetComponent<UINavigation>();
			UINavigation component4 = view.lanDisconnectBtn.GetComponent<UINavigation>();
			if (lastNavItem != null && (lastNavItem == component3 || lastNavItem == component4))
			{
				UINavigation.Focus(isConnectedToLAN ? component4 : component3);
			}
			view.ShowLanCreateIpInput(!running && !isConnectedToLAN);
			string p_string = "";
			string text = (isConnectedToLAN ? "CONNECTED" : "DISCONNECTED");
			if (running)
			{
				p_string = "SERVER: " + lanServer.localIp + " / " + text;
			}
			else if (isConnectedToLAN)
			{
				p_string = "CONNECTED / " + view.lanCreateIpInput.field.text;
			}
			view.ShowLanServerIpLabel(running || isConnectedToLAN, p_string);
			RefreshLobbyTitleAndFooter();
			view.EnableCreateRoomButton(isConnectedToLAN);
		}

		private void RefreshLobbyTitleAndFooter(string p_region = "")
		{
			Localization locale = base.app.model.storage.locale;
			string text = locale.Get("fly.screen.lobby", "Lobby");
			string text2 = locale.Get("fly.multiplayer.lan", "LAN");
			if (view.IsLanControlsVisible())
			{
				view.screen.title = text2 + " " + text;
			}
			else if (p_region != "")
			{
				view.screen.title = p_region + " " + text;
			}
			else
			{
				view.screen.title = text;
			}
			base.app.view.ui.header.Refresh();
		}

		protected void RefreshList()
		{
			view.listField.Clear();
		}

		private DRLMap GetRandomMap(GameFlag gameType)
		{
			List<DRLMap> maps = base.app.model.storage.GetMaps(gameType);
			DRLMap result = null;
			if (maps != null && maps.Count > 0)
			{
				result = maps[UnityEngine.Random.Range(0, maps.Count)];
			}
			return result;
		}

		private DRLMapTrack GetRandomTrack(DRLMap map, GameFlag gameType)
		{
			List<DRLMapTrack> mapTracks = base.app.model.storage.GetMapTracks(map, gameType);
			DRLMapTrack result = null;
			if (mapTracks != null && mapTracks.Count > 0)
			{
				result = mapTracks[UnityEngine.Random.Range(0, mapTracks.Count)];
			}
			return result;
		}

		protected void SetLobbyServer(int p_value)
		{
			Debug.Log("UIMultiplayerLobbyController> SetLobbyServer - value[" + p_value + "]");
			if (p_value == -1 && string.IsNullOrEmpty(model.ServerIP))
			{
				p_value = 4;
			}
			switch (p_value)
			{
			case 4:
				ConnectToServer();
				break;
			case -1:
				model.ConnectToLAN(model.ServerIP.Split(':')[0]);
				RefreshLanControls();
				break;
			default:
				ConnectToServer();
				break;
			}
		}
	}
}
