using UnityEngine;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UINetworkController : Controller<DRLApp>
	{
		public FloatProperty pingQuality;

		public IntProperty pingTime;

		public StringProperty regionName;

		public RoomProperty room;

		public LobbyProperty lobby;

		private Activity m_lanConnectionActivity;

		private bool m_disconnect_dialog_cooldown;

		public UINetworkView view => AssertLocal<UINetworkView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!base.validContext)
			{
				return;
			}
			Localization loc = base.app.model.storage.locale;
			UIFooterView fv = base.app.view.ui.footer;
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "network.footer@enable":
				view.EnableNetworkInFooter(p_enabled: true);
				view.UpdateLobby(regionName.Value, lobby.Value.Rooms.Count);
				if (room.Value.Room != null)
				{
					view.ShowNetworkInFooter(p_visible: true);
				}
				break;
			case "network.footer@disable":
				view.EnableNetworkInFooter(p_enabled: false);
				break;
			case "network.ping.update":
				view.pingMs = pingTime.Value;
				view.pingQuality = pingQuality.Value;
				break;
			case "network.connection@complete":
				view.UpdateLobby(regionName.Value, lobby.Value.Rooms.Count);
				view.SetLobbyServerListLANEnabled(p_lanEnabled: true);
				break;
			case "network.disconnect":
				_ = (PhotonService.DisconnectionCode)p_data[0];
				view.ShowNetworkInFooter(p_visible: false);
				view.UpdateLobby(regionName.Value, lobby.Value.Rooms.Count);
				view.SetLobbyServerListLANEnabled(p_lanEnabled: true);
				break;
			case "network.lobby.server-list@enable":
				view.SetLobbyServerButtonEnabled(p_enable: true);
				break;
			case "network.lobby.server-list@disable":
				view.SetLobbyServerButtonEnabled(p_enable: false);
				break;
			case "network.lobby@enter":
				view.ShowNetworkInFooter(p_visible: true);
				view.UpdateLobby(regionName.Value, lobby.Value.Rooms.Count);
				break;
			case "network.lobby@update":
				view.ShowNetworkInFooter(p_visible: true);
				view.UpdateLobby(regionName.Value, lobby.Value.Rooms.Count);
				break;
			case "multiplayer.lobby.server-list-button@click":
				view.ToggleLobbyServerList();
				break;
			case "multiplayer.lobby.server-list-item@click":
				view.ShowLobbyServerList(p_show: false);
				UINavigation.Focus(base.app.view.ui.footer.buttonNavigations[0]);
				break;
			case "network.LAN.online":
				view.UpdateLobby(regionName.Value, lobby.Value.Rooms.Count);
				view.SetLobbyServerListLANEnabled(p_lanEnabled: true);
				break;
			case "network.LAN.offline":
				view.ShowNetworkInFooter(p_visible: false);
				view.UpdateLobby(regionName.Value, lobby.Value.Rooms.Count);
				view.SetLobbyServerListLANEnabled(p_lanEnabled: true);
				break;
			case "multiplayer.lobby.lan-ip@change":
			{
				bool flag = string.IsNullOrEmpty(view.lanIPInputField.text);
				view.lanConnectButton.SetActive(!flag);
				view.lanStartButton.SetActive(flag);
				view.lanIPInputField.GetComponent<UINavigation>().right = (flag ? view.lanStartButton.GetComponent<UINavigation>() : view.lanConnectButton.GetComponent<UINavigation>());
				break;
			}
			case "multiplayer.lobby.lan-ui-connect@click":
			{
				string text2 = view.lanIPInputField.text;
				if (string.IsNullOrEmpty(text2))
				{
					base.app.view.audio.PlayUIGenericError();
					break;
				}
				Notify("multiplayer.lobby.lan-connect", text2);
				fv.network.SetLanStatus(loc.Get("multiplayer.lan.connecting", "CONNECTING..."));
				m_lanConnectionActivity = Activity.RunOnce(delegate
				{
					view.lobbyServerListLANButton.gameObject.SetActive(value: true);
					view.lanConnectingButton.SetActive(value: false);
				}, 15f);
				break;
			}
			case "network.LAN.starting":
				fv.network.SetLanStatus(loc.Get("multiplayer.lan.connecting", "CONNECTING..."));
				break;
			case "network.LAN.stopping":
				fv.network.SetLanStatus(loc.Get("multiplayer.lan.connecting", "WAIT..."));
				break;
			case "multiplayer.lobby.lan-start@click":
				base.app.model.network.photon.LanServer.OnState = delegate(PhotonLANServer.ServerState p_state)
				{
					switch (p_state)
					{
					case PhotonLANServer.ServerState.Starting:
						fv.network.SetLanStatus(loc.Get("multiplayer.lan.connecting", "WAIT..."));
						break;
					case PhotonLANServer.ServerState.Online:
						fv.network.SetLanStatus("SUCCESS!");
						Activity.RunOnce(delegate
						{
							fv.network.SetLanStatus();
							base.app.model.network.ConnectToLAN(base.app.model.network.photon.LanServer.localIp);
						}, 2f);
						break;
					case PhotonLANServer.ServerState.Offline:
						fv.network.SetLanStatus("LAN OFFLINE");
						Activity.RunOnce(delegate
						{
							fv.network.SetLanStatus();
						}, 2f);
						base.app.model.network.photon.LanServer.OnState = null;
						break;
					}
				};
				base.app.model.network.photon.LanServer.Run();
				break;
			case "multiplayer.lobby.lan-disconnect@click":
				Notify("multiplayer.lobby.lan-disconnect");
				break;
			case "multiplayer.lan.connected":
			{
				string text = (string)p_data[0];
				SetServerList(p_connected: true);
				view.lanIPInputField.text = text;
				if (m_lanConnectionActivity != null)
				{
					m_lanConnectionActivity.Stop();
				}
				break;
			}
			case "multiplayer.lan.disconnected":
				SetServerList(p_connected: false);
				break;
			case "multiplayer.lobby.lan-start@focus":
				view.lanIPInputField.placeholder = loc.Get("multiplayer.lan-input.placeholder-server", "CREATE SERVER");
				break;
			case "multiplayer.lobby.lan-start@unfocus":
				view.lanIPInputField.placeholder = loc.Get("multiplayer.lan-input.placeholder-ip", "CONNECT TO IP...");
				break;
			}
		}

		protected void SetServerList(bool p_connected)
		{
			view.ShowLobbyServerList(p_show: false, p_force: true);
			this.TimerRunOnce(delegate
			{
				if (base.validContext)
				{
					foreach (GameObject server in view.servers)
					{
						server.SetActive(!p_connected);
					}
					view.lanDisconnectButton.SetActive(p_connected);
					if (p_connected)
					{
						view.lanIPInputField.GetComponent<UINavigation>().right = view.lanDisconnectButton.GetComponent<UINavigation>();
					}
					view.lobbyServerListHeightOpen = (p_connected ? 65f : 280f);
				}
			}, 0.3f);
		}

		protected void OnDisable()
		{
			if (m_lanConnectionActivity != null)
			{
				m_lanConnectionActivity.Stop();
				m_lanConnectionActivity.manager.Remove(m_lanConnectionActivity);
				m_lanConnectionActivity = null;
			}
		}
	}
}
