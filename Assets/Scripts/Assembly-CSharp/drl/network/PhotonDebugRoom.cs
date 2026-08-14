using UnityEngine;

namespace drl.network
{
	public class PhotonDebugRoom
	{
		private static int roomOptions;

		private static string newChatMessage = "";

		private static Vector2 scrollPosition;

		public static void DrawRoom(PhotonService service)
		{
			if (service == null || service.CurrentRoom == null)
			{
				return;
			}
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(800f), GUILayout.Height(400f));
			NetworkRoom currentRoom = service.CurrentRoom;
			GUILayout.Space(50f);
			if (GUILayout.Button("LeaveRoom"))
			{
				service.TryLeaveRoom();
			}
			roomOptions = GUI.Toolbar(new Rect(0f, 100f, 800f, 30f), roomOptions, new string[3] { "General", "Players", "Chat" });
			GUILayout.Space(80f);
			switch (roomOptions)
			{
			case 0:
				GUILayout.Label($"Server time: {service.CurrentRoom.ServerTime}");
				GUILayout.Label($"Heat: {service.CurrentRoom.HeatIdx} / {service.CurrentRoom.MaxHeats} ");
				GUILayout.Space(10f);
				GUILayout.Label($"Room Id: {currentRoom.Id}");
				GUILayout.Label($"State: {currentRoom.State} - Gamemode: {currentRoom.GameMode} - MaxPlayers {currentRoom.MaxPlayers}");
				GUILayout.Label($"Racers: {currentRoom.RacersCount}/{currentRoom.MaxRacers}  -  Spectators: {currentRoom.SpectatorsCount}/{currentRoom.MaxSpectators}");
				if (currentRoom.State == NetworkRoom.StateCode.MatchMaking)
				{
					GUILayout.Label("Lobby countdown..." + currentRoom.LobbyCountdown);
					if (GUILayout.Button("Start"))
					{
						currentRoom.ForceStartMatch();
					}
					if (currentRoom.Local.IsRoomReady)
					{
						if (GUILayout.Button("READY"))
						{
							currentRoom.Local.IsRoomReady = false;
						}
					}
					else if (GUILayout.Button("NOT READY"))
					{
						currentRoom.Local.IsRoomReady = true;
					}
					if (GUILayout.Button("Webhook Test"))
					{
						currentRoom.Outgoing.SendWebHookTest();
					}
					if (GUILayout.Button("Gate Test"))
					{
						currentRoom.Outgoing.SendGateEvent(0);
					}
				}
				if (currentRoom.State == NetworkRoom.StateCode.MatchLocked)
				{
					GUILayout.Label("Client is ready: " + service.CurrentRoom.Local.IsLevelLoaded);
				}
				if (currentRoom.State == NetworkRoom.StateCode.GameRunning)
				{
					GUILayout.Label("Time Limit: " + service.CurrentRoom.TimeLimit);
					GUILayout.Label("Time Left: " + service.CurrentRoom.TimeLeft);
				}
				if (currentRoom.State == NetworkRoom.StateCode.GameFinished && GUILayout.Button("Play Again"))
				{
					currentRoom.StartMatchmaking();
				}
				break;
			case 1:
				GUILayout.Label("Player count: " + currentRoom.PlayerCount);
				GUILayout.Label($"-------------Racers: {currentRoom.RacersCount} ------------------");
				foreach (NetworkActor racer in currentRoom.Racers)
				{
					PhotonDebugPlayer.DrawPlayer(racer);
				}
				GUILayout.Label($"-------------Spectators: {currentRoom.SpectatorsCount} ------------------");
				foreach (NetworkActor spectator in currentRoom.Spectators)
				{
					PhotonDebugPlayer.DrawPlayer(spectator);
				}
				break;
			case 2:
				GUILayout.Label("Chat messages: " + currentRoom.Chat.History.Count);
				GUILayout.BeginVertical("", GUI.skin.window);
				foreach (NetworkRoomChat.Message item in currentRoom.Chat.History)
				{
					GUILayout.BeginHorizontal("", GUI.skin.box);
					GUILayout.Label(string.Format("{0} : {1} at {2}", item.IsMine ? "Me:" : item.SenderName, item.Content, PhotonUtils.TimeAgo(item.Date)));
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
				GUILayout.BeginHorizontal();
				newChatMessage = GUILayout.TextField(newChatMessage);
				if (GUILayout.Button("Send Message"))
				{
					currentRoom.Chat.SendChatMessage(newChatMessage);
					newChatMessage = "";
				}
				GUILayout.EndHorizontal();
				break;
			}
			GUILayout.EndScrollView();
		}
	}
}
