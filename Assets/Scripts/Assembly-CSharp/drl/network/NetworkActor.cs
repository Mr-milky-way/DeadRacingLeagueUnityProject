using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using drl.game;
using drl.sim;
using drl.sim.rci;
using thelab.core;

namespace drl.network
{
	[Serializable]
	public class NetworkActor : INetworkPlayer
	{
		public enum RacerState
		{
			Running = 0,
			Complete = 1,
			Timeout = 2,
			Crash = 3,
			Forfeit = 4
		}

		public static class CustomProperty
		{
			public const string BadgeLevel = "bl";

			public const string CameraFOV = "cf";

			public const string CameraTilt = "ctl";

			public const string ControllerType = "ct";

			public const string DroneRigData = "dr";

			public const string GhostsProcessed = "gp";

			public const string GhostsProcessing = "gpn";

			public const string HasSkippedAnimation = "hsa";

			public const string IsGameReady = "pr";

			public const string IsCountdownReady = "cdr";

			public const string IsLevelLoaded = "gl";

			public const string IsReplaySent = "irs";

			public const string IsRoomReady = "irr";

			public const string IsSpectator = "sp";

			public const string MainColor = "mc";

			public const string Order = "or";

			public const string ProfileColor = "pc";

			public const string SecondaryColor = "sc";

			public const string PlayfabId = "pl";

			public const string ProfileName = "pn";

			public const string ProfilePhoto = "pp";

			public const string RaceState = "rs";

			public const string RaceTime = "rt";

			public const string State = "s";

			public const string SteamId = "st";

			public const string XboxId = "xt";

			public const string PlatformId = "pt";

			public const string BlockList = "blist";

			public const string PlayerId = "pi";

			public const string ViewId = "vi";

			public const string VoteTrackGUID = "vt";

			public const string Platform = "ptf";

			public const string Crossplay = "cp";

			public const string HasSubmittedLeaderboard = "lb";
		}

		private readonly Hashtable cachedPropertyHashtable = new Hashtable();

		public PhotonPlayer RawData;

		public int ID
		{
			get
			{
				if (RawData != null)
				{
					return RawData.ID;
				}
				return -1;
			}
		}

		public string UserId
		{
			get
			{
				if (RawData != null)
				{
					return RawData.UserId;
				}
				return "";
			}
		}

		public bool IsLocal
		{
			get
			{
				if (RawData != null)
				{
					return RawData.IsLocal;
				}
				return false;
			}
		}

		public bool IsMaster
		{
			get
			{
				if (RawData != null)
				{
					return RawData.IsMasterClient;
				}
				return false;
			}
		}

		public int BadgeLevel
		{
			get
			{
				return GetCustom<int>("bl");
			}
			set
			{
				SetCustom<int>("bl", value);
			}
		}

		public float CameraFOV
		{
			get
			{
				return GetCustom<float>("cf");
			}
			set
			{
				SetCustom<float>("cf", value);
			}
		}

		public float CameraTilt
		{
			get
			{
				return GetCustom<float>("ctl");
			}
			set
			{
				SetCustom<float>("ctl", value);
			}
		}

		public int ControllerType
		{
			get
			{
				return GetCustom<int>("ct");
			}
			set
			{
				SetCustom<int>("ct", value);
			}
		}

		public string DroneRigData
		{
			get
			{
				return GetCustom<string>("dr") ?? "";
			}
			set
			{
				SetCustom<string>("dr", value);
			}
		}

		public bool HasSkippedAnimation
		{
			get
			{
				return GetCustom<bool>("hsa");
			}
			set
			{
				SetCustom<bool>("hsa", value);
			}
		}

		public bool GhostsProcessed
		{
			get
			{
				return GetCustom<bool>("gp");
			}
			set
			{
				SetCustom<bool>("gp", value);
			}
		}

		public bool GhostsProcessing
		{
			get
			{
				return GetCustom<bool>("gpn");
			}
			set
			{
				SetCustom<bool>("gpn", value);
			}
		}

		public bool IsGameReady
		{
			get
			{
				return GetCustom<bool>("pr");
			}
			set
			{
				SetCustom<bool>("pr", value);
			}
		}

		public bool IsCountdownReady
		{
			get
			{
				return GetCustom<bool>("cdr");
			}
			set
			{
				SetCustom<bool>("cdr", value);
			}
		}

		public bool IsLevelLoaded
		{
			get
			{
				return GetCustom<bool>("gl");
			}
			set
			{
				SetCustom<bool>("gl", value);
			}
		}

		public bool IsReplaySent
		{
			get
			{
				return GetCustom<bool>("irs");
			}
			set
			{
				SetCustom<bool>("irs", value);
			}
		}

		public bool IsRoomReady
		{
			get
			{
				return GetCustom<bool>("irr");
			}
			set
			{
				SetCustom<bool>("irr", value);
			}
		}

		public bool IsSpectator
		{
			get
			{
				return GetCustom<bool>("sp");
			}
			set
			{
				SetCustom<bool>("sp", value);
			}
		}

		public Color MainColor
		{
			get
			{
				return Colorf.RGBToColor((uint)GetCustom<int>("mc"));
			}
			set
			{
				SetCustom<int>("mc", (int)Colorf.ColorToRGB(value));
			}
		}

		public int Order
		{
			get
			{
				return GetCustom<int>("or");
			}
			set
			{
				SetCustom<int>("or", value);
			}
		}

		public string PlayfabId
		{
			get
			{
				return GetCustom<string>("pl") ?? "";
			}
			set
			{
				SetCustom<string>("pl", value);
			}
		}

		public Color ProfileColor
		{
			get
			{
				return Colorf.RGBToColor((uint)GetCustom<int>("pc"));
			}
			set
			{
				SetCustom<int>("pc", (int)Colorf.ColorToRGB(value));
			}
		}

		public Color SecondaryColor
		{
			get
			{
				return Colorf.RGBToColor((uint)GetCustom<int>("sc"));
			}
			set
			{
				SetCustom<int>("sc", (int)Colorf.ColorToRGB(value));
			}
		}

		public string ProfileName
		{
			get
			{
				return GetCustom<string>("pn") ?? "";
			}
			set
			{
				SetCustom<string>("pn", value);
			}
		}

		public string ProfilePhoto
		{
			get
			{
				return GetCustom<string>("pp") ?? "";
			}
			set
			{
				SetCustom<string>("pp", value);
			}
		}

		public RacerState RaceState
		{
			get
			{
				return GetCustom<RacerState>("rs");
			}
			set
			{
				SetCustom<RacerState>("rs", value);
			}
		}

		public float RaceTime
		{
			get
			{
				return GetCustom<float>("rt");
			}
			set
			{
				SetCustom<float>("rt", value);
			}
		}

		public string PlatformId
		{
			get
			{
				return GetCustom<string>("pt") ?? "";
			}
			set
			{
				SetCustom<string>("pt", value);
			}
		}

		public string BlockList
		{
			get
			{
				return GetCustom<string>("blist") ?? "";
			}
			set
			{
				SetCustom<string>("blist", value);
			}
		}

		public int ViewId
		{
			get
			{
				return GetCustom<int>("vi");
			}
			set
			{
				SetCustom<int>("vi", value);
			}
		}

		public string VotedTrackGUID
		{
			get
			{
				return GetCustom<string>("vt") ?? "";
			}
			set
			{
				SetCustom<string>("vt", value);
			}
		}

		public string PlayerId
		{
			get
			{
				return GetCustom<string>("pi") ?? "";
			}
			set
			{
				SetCustom<string>("pi", value);
			}
		}

		public string Platform
		{
			get
			{
				return GetCustom<string>("ptf") ?? "";
			}
			set
			{
				SetCustom<string>("ptf", value);
			}
		}

		public bool Crossplay
		{
			get
			{
				return GetCustom<bool>("cp");
			}
			set
			{
				SetCustom<bool>("cp", value);
			}
		}

		public bool HasSubmittedLeaderboard
		{
			get
			{
				return GetCustom<bool>("lb");
			}
			set
			{
				SetCustom<bool>("lb", value);
			}
		}

		public NetworkActor(PhotonPlayer player)
		{
			RawData = player;
		}

		public void Reset()
		{
			IsRoomReady = false;
			IsLevelLoaded = false;
			IsGameReady = false;
			IsCountdownReady = false;
			RaceState = RacerState.Running;
			IsReplaySent = false;
			RaceTime = 0f;
			VotedTrackGUID = "";
			HasSkippedAnimation = false;
			HasSubmittedLeaderboard = false;
		}

		public T GetCustom<T>(string key)
		{
			T result = default(T);
			if (RawData != null && RawData.CustomProperties.ContainsKey(key))
			{
				return (T)RawData.CustomProperties[key];
			}
			return result;
		}

		public void SetCustom<T>(string key, object newValue)
		{
			if (RawData != null && (RawData.IsLocal || PhotonNetwork.isMasterClient) && newValue != null && !EqualityComparer<T>.Default.Equals(GetCustom<T>(key), (T)newValue))
			{
				cachedPropertyHashtable.Clear();
				cachedPropertyHashtable.Add(key, newValue);
				RawData.SetCustomProperties(cachedPropertyHashtable);
			}
		}

		public void Set(PlayerStateModel p_data)
		{
			if (!p_data)
			{
				Debug.LogWarning("NetworkActor> Invalid Player State!");
				return;
			}
			ProfileColor = p_data.profile.color;
			SecondaryColor = p_data.profile.color;
			ProfileName = p_data.profile.username;
			ProfilePhoto = p_data.profile.photoURL;
			PlatformId = p_data.profile.platformId;
			PlayerId = p_data.profile.playerId;
			BadgeLevel = p_data.userRank;
			Platform = OS.prefix;
			BlockList = p_data.profile.blockList;
			ControllerType = (int)RCI.GetControllerStateType(ControllerStateType.Taranis);
			if (IsMaster)
			{
				Crossplay = p_data.settings.game.crossplay;
			}
		}
	}
}
