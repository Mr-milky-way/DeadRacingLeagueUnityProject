using System;
using UnityEngine;

namespace drl.network
{
	[RequireComponent(typeof(PhotonService))]
	public class PhotonDebugHelper : MonoBehaviour
	{
		public string playerName = "Unnamed";

		public string userId = "";

		public string TournamentId = "tournamentTest01";

		public string MatchId = "match01";

		public string[] reservedIds;

		public bool DebugGUI;

		private PhotonService service;

		private int selectedTab;

		private int gamemode;

		private string createPassword = "";

		private string joinPassword = "";

		private string customServerAddress = "";

		private void Awake()
		{
			service = GetComponent<PhotonService>();
			if (service == null)
			{
				Debug.LogError("PhotonDebugHelper can't run without a PhotonService attached to the same GameObject");
				return;
			}
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			PhotonService photonService = service;
			photonService.OnNetworkEvent = (Action<PhotonService.EventType, object>)Delegate.Combine(photonService.OnNetworkEvent, new Action<PhotonService.EventType, object>(OnIncomingNetworkEvent));
		}

		private void OnDestroy()
		{
			PhotonService photonService = service;
			photonService.OnNetworkEvent = (Action<PhotonService.EventType, object>)Delegate.Remove(photonService.OnNetworkEvent, new Action<PhotonService.EventType, object>(OnIncomingNetworkEvent));
		}

		private void OnIncomingNetworkEvent(PhotonService.EventType eventCode, object content)
		{
			switch (eventCode)
			{
			case PhotonService.EventType.OnJoinLobby:
				OnJoinLobby((Lobby)content);
				break;
			case PhotonService.EventType.OnJoinRoom:
				OnJoinRoom((NetworkRoom)content);
				break;
			case PhotonService.EventType.OnPlayerJoin:
				OnPlayerJoin((NetworkActor)content);
				break;
			case PhotonService.EventType.OnPlayerLeft:
				OnPlayerLeft((int)content);
				break;
			case PhotonService.EventType.OnLeftRoom:
				OnLeftRoom();
				break;
			case PhotonService.EventType.OnLeftLobby:
				OnLeftLobby();
				break;
			case PhotonService.EventType.OnLobbyUpdated:
			case PhotonService.EventType.OnJoinRoomFailed:
			case PhotonService.EventType.OnCreateRoomFailed:
			case PhotonService.EventType.OnRoomUpdated:
			case PhotonService.EventType.OnPlayerUpdated:
				break;
			}
		}

		private void OnJoinLobby(Lobby lobby)
		{
			(GetHelperComponent<LobbyComponent>(base.gameObject) ?? AddHelperComponent<LobbyComponent>("Lobby", base.gameObject)).UpdateData(service.CurrentLobby);
		}

		private void OnJoinRoom(NetworkRoom room)
		{
			RemoveHelper<LobbyComponent>(base.gameObject);
			GameRoomComponent gameRoomComponent = AddHelperComponent<GameRoomComponent>("Room", base.gameObject);
			gameRoomComponent.UpdateData(room);
			foreach (NetworkActor value in room.Players.Values)
			{
				gameRoomComponent.AddPlayer(value);
			}
			room.MaxHeats = 3;
		}

		private void OnLeftRoom()
		{
			RemoveHelper<GameRoomComponent>(base.gameObject);
		}

		private void OnLeftLobby()
		{
			RemoveHelper<LobbyComponent>(base.gameObject);
		}

		private void OnPlayerJoin(NetworkActor newPlayer)
		{
			GetHelperComponent<GameRoomComponent>(base.gameObject).AddPlayer(newPlayer);
		}

		private void OnPlayerLeft(int otherPlayer)
		{
			GetHelperComponent<GameRoomComponent>(base.gameObject).RemovePlayer(otherPlayer);
		}

		public static T GetHelperComponent<T>(GameObject parentObject) where T : MonoBehaviour
		{
			return parentObject.GetComponentInChildren<T>();
		}

		public static T AddHelperComponent<T>(string childName, GameObject parentObject) where T : MonoBehaviour
		{
			GameObject obj = new GameObject();
			obj.name = childName;
			obj.transform.parent = parentObject.transform;
			return obj.AddComponent<T>();
		}

		public static void RemoveHelper<T>(GameObject parent) where T : MonoBehaviour
		{
			T componentInChildren = parent.GetComponentInChildren<T>();
			if (componentInChildren != null && componentInChildren.gameObject != null)
			{
				UnityEngine.Object.Destroy(componentInChildren.gameObject);
			}
		}

		private void OnGUI()
		{
			if (service == null || !DebugGUI)
			{
				return;
			}
			GUILayout.BeginArea(new Rect(Screen.width / 2 - 400, 0f, 800f, Screen.height));
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label("UserID:" + service.UserId + " Name:" + service.PlayerName);
			if (GUILayout.Button("Update Name"))
			{
				service.PlayerName = playerName;
			}
			GUILayout.EndHorizontal();
			GUILayout.Label($"Connection state: {service.State} - {service.ServerIP}");
			selectedTab = GUI.Toolbar(new Rect(0f, 100f, 800f, 30f), selectedTab, new string[5] { "Quick Game", "Browse Games", "Create Room", "Change Regions", "LAN Server" });
			if (service.State != PhotonService.ServiceState.Disconnected && service.State != PhotonService.ServiceState.InProgress)
			{
				GUILayout.Label($"Ping {service.PingTime}ms quality:{service.CurrentLobby.PingQuality}");
				if (GUILayout.Button("Disconnect"))
				{
					service.TryDisconnect();
				}
			}
			if (service.State == PhotonService.ServiceState.Disconnected)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Custom Server Address: ");
				customServerAddress = GUILayout.TextField(customServerAddress);
				GUILayout.EndHorizontal();
			}
			if (service.State != PhotonService.ServiceState.InRoom)
			{
				GUILayout.Space(100f);
			}
			switch (selectedTab)
			{
			case 0:
			{
				if (service.State == PhotonService.ServiceState.InRoom || service.State == PhotonService.ServiceState.InProgress)
				{
					break;
				}
				GUILayout.Space(20f);
				gamemode = GUILayout.Toolbar(gamemode, new string[3] { "Freestyle", "Race", "Tournament" });
				GUILayout.Space(20f);
				NetworkRoom.GameType gameType = (NetworkRoom.GameType)gamemode;
				if (GUILayout.Button("Join random " + gameType.ToString() + " Room"))
				{
					NetworkRoomOptions networkRoomOptions2 = new NetworkRoomOptions((NetworkRoom.GameType)gamemode, NetworkRoom.MatchmakingFlow.Normal);
					networkRoomOptions2.QuickMatchProperties.Add("g", gamemode);
					service.TryConnectRandomRoom(networkRoomOptions2, delegate(QuickMatchResult success)
					{
						Debug.Log("Quick Match status: " + success.ToString());
					});
				}
				break;
			}
			case 1:
				GUILayout.Space(20f);
				if (service.State == PhotonService.ServiceState.Disconnected)
				{
					if (GUILayout.Button("Connect To Default Lobby"))
					{
						service.TryConnectBestRegionLobby();
					}
					if (GUILayout.Button("Connect To Tournament Lobby with ID[" + TournamentId + "]"))
					{
						service.UserId = userId;
						service.TryConnectBestRegionLobby(TournamentId);
					}
				}
				if (service.State != PhotonService.ServiceState.InLobby)
				{
					break;
				}
				GUILayout.Label($"Joined {service.ServerLobby.Name} Lobby in {service.CurrentRegionCode}");
				GUILayout.BeginVertical();
				GUILayout.Label($"Rooms in this lobby {PhotonNetwork.GetRoomList().Length}");
				foreach (Lobby.NetworkRoomInfo room in service.CurrentLobby.Rooms)
				{
					if (!room.IsOpen)
					{
						continue;
					}
					GUILayout.BeginHorizontal();
					GUILayout.Label($"Room Title: {room.RoomTitle}  Gamemode: {room.GameMode} State: {room.State}");
					GUILayout.Label($"Racers: {room.RacersCount} / {room.MaxRacers}");
					GUILayout.Label($"Spectators: {room.SpectatorsCount} / {room.MaxSpectators}");
					if (room.CanRace)
					{
						bool flag = true;
						if (room.IsPrivate)
						{
							GUILayout.Label("Private Room");
							joinPassword = GUILayout.PasswordField(joinPassword, '*');
							flag = joinPassword.Equals(room.Password);
						}
						if (flag && GUILayout.Button("Join"))
						{
							service.TryJoinRoom(room.Name);
						}
					}
					else if (room.CanSpectate)
					{
						if (GUILayout.Button("Spectate only"))
						{
							service.TryJoinRoom(room.Name);
						}
					}
					else
					{
						GUILayout.Label("Full");
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
				break;
			case 2:
			{
				GUILayout.Space(20f);
				if (service.State == PhotonService.ServiceState.Disconnected)
				{
					if (GUILayout.Button("Connect To Lobby"))
					{
						service.TryConnectBestRegionLobby();
					}
					if (GUILayout.Button("Connect To Tournament Lobby with ID[" + TournamentId + "]"))
					{
						service.TryConnectToRegionMaster(CloudRegionCode.us, TournamentId);
					}
				}
				if (service.State != PhotonService.ServiceState.InLobby)
				{
					break;
				}
				GUILayout.Label($"Joined {service.ServerLobby.Name} Lobby in {service.CurrentRegionCode}");
				GUILayout.BeginVertical();
				GUILayout.Space(50f);
				GUILayout.BeginHorizontal();
				GUILayout.Label("Set Password: ");
				createPassword = GUILayout.PasswordField(createPassword, '*');
				GUILayout.EndHorizontal();
				gamemode = GUILayout.Toolbar(gamemode, new string[3] { "Freestyle", "Race", "Tournament" });
				GUILayout.Space(20f);
				NetworkRoom.GameType gameType = (NetworkRoom.GameType)gamemode;
				if (GUILayout.Button("Create Room: " + gameType))
				{
					NetworkRoomOptions networkRoomOptions = new NetworkRoomOptions((NetworkRoom.GameType)gamemode, NetworkRoom.MatchmakingFlow.Normal);
					networkRoomOptions.Password = createPassword;
					if (gamemode == 2)
					{
						networkRoomOptions.ExpectedPlayers = reservedIds;
						service.TryJoinOrCreateCustomRoom(MatchId, networkRoomOptions);
					}
					else
					{
						service.TryCreateRoom(networkRoomOptions);
					}
				}
				GUILayout.EndVertical();
				break;
			}
			case 3:
				GUILayout.Label("Change the current region");
				GUILayout.Space(20f);
				GUILayout.BeginVertical();
				GUILayout.Label("Available Regions " + service.AvailableRegions.Count);
				foreach (Region availableRegion in service.AvailableRegions)
				{
					GUILayout.BeginHorizontal();
					if (availableRegion.Code == service.CurrentRegionCode)
					{
						GUILayout.Label("Connected");
					}
					else if (service.State != PhotonService.ServiceState.InProgress && GUILayout.Button("Change"))
					{
						service.TryConnectToRegionMaster(availableRegion.Code);
					}
					GUILayout.Label($"Region {availableRegion.Code} - ping: {availableRegion.Ping}");
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
				break;
			}
			if (service.State == PhotonService.ServiceState.InRoom)
			{
				PhotonDebugRoom.DrawRoom(service);
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		public static string DrawProperty(string label, string property, float labelWith = 100f, float labelWidth = 100f)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(label, GUILayout.Width(labelWith));
			property = GUILayout.TextField(property, GUILayout.Width(labelWidth));
			GUILayout.EndHorizontal();
			return property;
		}

		public static DateTime UnixTimestampToDateTime(double unixTime)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
		}

		public static int DrawProperty(string label, int property, float labelWith = 100f, float labelWidth = 100f)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(label, GUILayout.Width(labelWith));
			string s = GUILayout.TextField(property.ToString(), GUILayout.Width(labelWidth));
			GUILayout.EndHorizontal();
			int.TryParse(s, out property);
			return property;
		}
	}
}
