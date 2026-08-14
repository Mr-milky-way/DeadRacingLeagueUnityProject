using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using thelab.core;

namespace drl.network
{
	[Serializable]
	public class Lobby
	{
		[Serializable]
		public class NetworkRoomInfo
		{
			public string Name;

			public string RoomTitle;

			public int PlayerCount;

			public NetworkRoom.GameType GameMode;

			public int RacersCount;

			public int SpectatorsCount;

			public int HeatIdx;

			public int LobbyCountdown;

			public bool LobbyCountdownAllowed;

			public int MaxRacers;

			public int MaxPlayers;

			public int MaxSpectators;

			public string Password;

			public string MapId;

			public string MatchId;

			public string RaceId;

			public string TrackId;

			public string CustomMapName;

			public string MasterProfilePhoto;

			public Color MasterProfileColour;

			public NetworkRoom.StateCode State;

			public bool CanRace;

			public bool CanSpectate;

			public bool IsPrivate;

			public bool IsOpen;

			public double InGameTimeStarted;

			public float Progress;

			public double TimeLimit;

			public NetworkRoom.MatchmakingFlow MatchmakingType;

			public string PlayersIdsData;

			public List<string> SteamIds = new List<string>();

			public bool Crossplay = true;

			public string MasterPlatform = "undefined";

			public bool IsQuick => MatchmakingType == NetworkRoom.MatchmakingFlow.Quick;

			public bool IsCustom => MatchmakingType == NetworkRoom.MatchmakingFlow.Normal;

			public bool UsingCustomMap => !string.IsNullOrEmpty(CustomMapName);

			public float ElapsedTime
			{
				get
				{
					float num = (float)(PhotonNetwork.time - InGameTimeStarted);
					if (!(num > 0f))
					{
						return 0f;
					}
					return num;
				}
			}

			public bool IsFull => PlayerCount >= MaxPlayers;

			public bool InGame
			{
				get
				{
					bool result = false;
					if (GameMode == NetworkRoom.GameType.Race)
					{
						result = State == NetworkRoom.StateCode.MatchLocked || State == NetworkRoom.StateCode.GameWarmup || State == NetworkRoom.StateCode.GameRunning || State == NetworkRoom.StateCode.GameLoading || State == NetworkRoom.StateCode.GameFinished;
					}
					return result;
				}
			}

			public NetworkRoomInfo(RoomInfo room)
			{
				Name = room.Name;
				PlayerCount = room.PlayerCount;
				GameMode = (NetworkRoom.GameType)room.CustomProperties["g"];
				RoomTitle = (string)room.CustomProperties["rt"];
				MaxRacers = (int)room.CustomProperties["mr"];
				MaxSpectators = (int)room.CustomProperties["ms"];
				RacersCount = (int)room.CustomProperties["rc"];
				SpectatorsCount = (int)room.CustomProperties["sc"];
				Password = (string)room.CustomProperties["p"];
				HeatIdx = (int)room.CustomProperties["hid"];
				LobbyCountdown = (int)room.CustomProperties["lc"];
				LobbyCountdownAllowed = (bool)room.CustomProperties["lca"];
				MasterProfilePhoto = (string)room.CustomProperties["mph"];
				MasterProfileColour = Colorf.RGBToColor((uint)(int)room.CustomProperties["mpc"]);
				MapId = (string)room.CustomProperties["m"];
				MatchId = (string)room.CustomProperties["mid"];
				RaceId = (string)room.CustomProperties["rid"];
				TrackId = (string)room.CustomProperties["t"];
				CustomMapName = (string)room.CustomProperties["cmn"];
				State = (NetworkRoom.StateCode)room.CustomProperties["s"];
				MaxPlayers = room.MaxPlayers;
				CanRace = (bool)room.CustomProperties["cr"];
				CanSpectate = (bool)room.CustomProperties["cs"];
				InGameTimeStarted = (double)room.CustomProperties["igs"];
				Progress = (float)room.CustomProperties["pr"];
				TimeLimit = (double)room.CustomProperties["tl"];
				IsOpen = room.IsOpen;
				MatchmakingType = (NetworkRoom.MatchmakingFlow)room.CustomProperties["mt"];
				IsPrivate = (bool)room.CustomProperties["ip"];
				PlayersIdsData = (string)room.CustomProperties["pi"];
				if (room.CustomProperties["mp"] != null)
				{
					MasterPlatform = (string)room.CustomProperties["mp"];
				}
				if (room.CustomProperties["cp"] != null)
				{
					Crossplay = (bool)room.CustomProperties["cp"];
				}
				if (!string.IsNullOrEmpty(PlayersIdsData))
				{
					SteamIds = JsonConvert.DeserializeObject<List<string>>(PlayersIdsData);
				}
			}

			public NetworkRoomInfo()
			{
			}
		}

		public string Region;

		public bool IsConnected;

		public int PingTime;

		[SerializeField]
		public List<NetworkRoomInfo> Rooms = new List<NetworkRoomInfo>();

		public float PingQuality => PhotonUtils.GetPingQualityLevel(PingTime);

		public NetworkRoomInfo FindRoomById(string p_id)
		{
			return Rooms.Find((NetworkRoomInfo it) => it.Name == p_id);
		}

		public void Reset()
		{
			IsConnected = false;
			Rooms.Clear();
			Region = CloudRegionCode.none.ToString();
		}
	}
}
