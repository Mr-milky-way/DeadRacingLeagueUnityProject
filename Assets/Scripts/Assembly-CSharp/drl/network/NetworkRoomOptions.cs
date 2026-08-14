using System;
using ExitGames.Client.Photon;
using UnityEngine;
using thelab.core;

namespace drl.network
{
	public class NetworkRoomOptions
	{
		public NetworkRoom.MatchmakingFlow MatchmakingType;

		public string MapGUID;

		public string TrackGUID;

		public string CustomMapId;

		public string Campaign;

		public string Password;

		public string RoomTitle;

		public string MasterPhoto;

		public Color MasterColor;

		public string MatchId;

		public int MaxRacers;

		public int MaxSpectators;

		public string MasterPlatform;

		public bool Crossplay;

		public Hashtable QuickMatchProperties;

		public string[] ExpectedPlayers;

		public DateTime ServerTime;

		public string MasterId;

		public string MasterBlockList;

		public int HeatIdx;

		public bool LobbyCountdownAllowed;

		public int MaxHeats;

		public int DroneClass = 7;

		public bool HasDelayedStart;

		public string DefaultDrone = "DRD-fc5bf84d13e5bac67957921c";

		public NetworkRoom.GameType Gamemode { get; private set; }

		public NetworkRoomOptions(NetworkRoom.GameType gamemode, NetworkRoom.MatchmakingFlow matchmakingType)
		{
			Gamemode = gamemode;
			MatchmakingType = matchmakingType;
			CustomMapId = "";
			MapGUID = "";
			TrackGUID = "";
			Campaign = "";
			Password = "";
			RoomTitle = "";
			MasterPhoto = "";
			MasterColor = Color.black;
			MatchId = "";
			MasterId = "";
			MasterBlockList = "";
			MaxRacers = ((gamemode == NetworkRoom.GameType.Freestyle) ? 12 : 6);
			MaxSpectators = 15;
			MasterPlatform = "undefined";
			Crossplay = true;
			HasDelayedStart = false;
			QuickMatchProperties = new Hashtable();
		}

		public RoomOptions GetPhotonRoomOptions()
		{
			RoomOptions roomOptions = new RoomOptions();
			Hashtable hashtable = new Hashtable();
			hashtable.Add("auc", false);
			hashtable.Add("cmi", CustomMapId);
			hashtable.Add("cmn", string.Empty);
			hashtable.Add("g", Gamemode);
			hashtable.Add("hid", HeatIdx);
			hashtable.Add("ip", false);
			hashtable.Add("lc", 60);
			hashtable.Add("lca", LobbyCountdownAllowed);
			hashtable.Add("mt", MatchmakingType);
			hashtable.Add("m", MapGUID);
			hashtable.Add("t", TrackGUID);
			hashtable.Add("c", Campaign);
			hashtable.Add("p", Password);
			hashtable.Add("rc", 0);
			hashtable.Add("sc", 0);
			hashtable.Add("mr", MaxRacers);
			hashtable.Add("ms", MaxSpectators);
			hashtable.Add("mph", MasterPhoto);
			hashtable.Add("mpc", (int)Colorf.ColorToRGB(MasterColor));
			hashtable.Add("mid", MatchId);
			hashtable.Add("s", NetworkRoom.StateCode.None);
			hashtable.Add("cr", true);
			hashtable.Add("cs", true);
			hashtable.Add("igs", 0.0);
			hashtable.Add("pr", 0f);
			hashtable.Add("tl", 180.0);
			hashtable.Add("stu", ServerTime.Ticks);
			hashtable.Add("mh", MaxHeats);
			hashtable.Add("d", DroneClass);
			hashtable.Add("pi", string.Empty);
			hashtable.Add("cp", Crossplay);
			hashtable.Add("mp", MasterPlatform);
			hashtable.Add("mi", MasterId);
			hashtable.Add("mblist", MasterBlockList);
			hashtable.Add("ds", HasDelayedStart);
			hashtable.Add("sd", DefaultDrone);
			roomOptions.CustomRoomProperties = hashtable;
			roomOptions.CustomRoomPropertiesForLobby = new string[32]
			{
				"cr", "cs", "g", "m", "cmn", "hid", "ip", "lc", "lca", "mpc",
				"mph", "mt", "mid", "rid", "mr", "ms", "p", "pi", "pr", "rc",
				"rt", "igs", "sc", "s", "t", "tl", "cp", "mp", "mi", "mblist",
				"ds", "sd"
			};
			roomOptions.MaxPlayers = (byte)(MaxRacers + MaxSpectators);
			roomOptions.PublishUserId = true;
			roomOptions.MasterPlatform = MasterPlatform;
			roomOptions.Crossplay = Crossplay;
			return roomOptions;
		}
	}
}
