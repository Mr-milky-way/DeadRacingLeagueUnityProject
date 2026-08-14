using System;
using Newtonsoft.Json.Linq;
using drl.game;
using thelab.core;

namespace drl.backend
{
	public class DRLTournamentData : SerializedData
	{
		private string[] m_playerIds;

		private DRLTournamentRoundData[] m_rounds;

		private DRLTournamentPlayerData[] m_rankings;

		public string[] playerIds
		{
			get
			{
				if (m_playerIds != null)
				{
					return m_playerIds;
				}
				return new string[0];
			}
		}

		public int registeredPlayersCount => Get("players-size", 0);

		public int maxPlayers => Get("max-players", 0);

		public string id => Get("id", "");

		public string guid
		{
			get
			{
				return Get("guid", "");
			}
			set
			{
				Set("guid", value);
			}
		}

		public string title => Get("title", "");

		public string region => Get("region", "");

		public bool enabledLAN => Get("lan-support", d: false);

		public string serverLAN => Get("server-ip", "");

		public string callToAction => Get("call-to-action", "");

		public string description => Get("description", "");

		public string prizeDescription => Get("prize-description", string.Empty);

		public string prizeURL => Get("prize-url", "");

		public string imageURL => Get("image-url", "");

		public string videoURL => Get<string>("video-url");

		public bool allowRegistrations => Get("allow-new-registration", d: false);

		public bool disablePublicSpectators => Get("disable-public-spectators", d: false);

		public string registerStartDateString
		{
			get
			{
				object obj = Get<object>("register-start", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public string registerEndDateString
		{
			get
			{
				object obj = Get<object>("register-end", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public string currentTimeString
		{
			get
			{
				object obj = Get<object>("current-time", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public bool hasPenalty => Get("penalty", d: false);

		public DateTime registerStartDate
		{
			get
			{
				string s = registerStartDateString;
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(s, out result);
				return result;
			}
		}

		public DateTime registerEndDate
		{
			get
			{
				string s = registerEndDateString;
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(s, out result);
				return result;
			}
		}

		public DateTime currentTime
		{
			get
			{
				string s = currentTimeString;
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(s, out result);
				return result;
			}
		}

		public TournamentState status
		{
			get
			{
				object obj = Get<object>("status", null);
				if (obj == null)
				{
					return TournamentState.none;
				}
				return (TournamentState)Enum.Parse(typeof(TournamentState), obj.ToString());
			}
		}

		public TournamentProgression progression
		{
			get
			{
				object obj = Get<object>("progression", null);
				if (obj == null)
				{
					return TournamentProgression.auto;
				}
				return (TournamentProgression)Enum.Parse(typeof(TournamentProgression), obj.ToString());
			}
		}

		public bool invalid
		{
			get
			{
				TournamentState tournamentState = status;
				if ((uint)(tournamentState - 1) <= 1u || tournamentState == TournamentState.canceled)
				{
					return true;
				}
				return false;
			}
		}

		public string droneGuid
		{
			get
			{
				if (drlPilotMode)
				{
					return "DRD-fc5bf84d13e5bac67957921c";
				}
				return Get("drone-guid", "");
			}
		}

		public int droneClass
		{
			get
			{
				if (drlPilotMode)
				{
					return 1;
				}
				int result = -1;
				int.TryParse(droneClassString, out result);
				return result;
			}
		}

		public bool drlPilotMode => Get("drl-pilot-mode", d: false);

		public string droneClassString => Get("default-drone-class", (object)"0").ToString();

		public int minimumSkill
		{
			get
			{
				return Get("minimum-skill", 0);
			}
			set
			{
				Set("minimum-skill", value);
			}
		}

		public string streamingURL => Get("streaming-url", "");

		public bool isPrivate => Get("private", d: false);

		public bool isDAWC => Get("dawc-seeding", d: false);

		public bool hasCountdown => Get("countdown", d: false);

		public DRLTournamentRoundData[] rounds
		{
			get
			{
				if (m_rounds != null)
				{
					return m_rounds;
				}
				return new DRLTournamentRoundData[0];
			}
		}

		public DRLTournamentPlayerData[] rankings
		{
			get
			{
				if (m_rankings != null)
				{
					return m_rankings;
				}
				return new DRLTournamentPlayerData[0];
			}
		}

		public bool ageRestricted => Get<bool>("age-check");

		public int ageRestriction => Get<int>("age-check-number");

		public string termsURL => Get<string>("terms-and-conditions-url");

		public TournamentType type => (TournamentType)Enum.Parse(typeof(TournamentType), Get("type", "None"));

		public int GetActiveRoundIndex()
		{
			DRLTournamentRoundData[] array = rounds;
			if (array == null)
			{
				return -1;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && array[i].state == TournamentRoundState.active)
				{
					return i;
				}
			}
			if (status == TournamentState.complete)
			{
				return rounds.Length - 1;
			}
			return -1;
		}

		public DRLTournamentRoundData GetActiveRound()
		{
			int activeRoundIndex = GetActiveRoundIndex();
			if (activeRoundIndex >= 0)
			{
				return rounds[activeRoundIndex];
			}
			return null;
		}

		public DRLTournamentRoundData GetLastRound()
		{
			DRLTournamentRoundData result = null;
			if (rounds == null || rounds.Length == 0)
			{
				return result;
			}
			for (int i = 0; i < rounds.Length; i++)
			{
				if (rounds[i].state == TournamentRoundState.complete)
				{
					result = rounds[i];
				}
			}
			return result;
		}

		public TournamentRoundGameMode GetActiveRoundMode()
		{
			return GetActiveRound()?.gameMode ?? TournamentRoundGameMode.none;
		}

		public TournamentRoundState GetActiveRoundState()
		{
			return GetActiveRound()?.state ?? TournamentRoundState.none;
		}

		public DRLTournamentRoundData GetRoundForMatch(string p_matchId)
		{
			for (int i = 0; i < rounds.Length; i++)
			{
				if (rounds[i].matches == null || rounds[i].matches.Length == 0)
				{
					continue;
				}
				for (int j = 0; j < rounds[i].matches.Length; j++)
				{
					if (rounds[i].matches[j].Id == p_matchId)
					{
						return rounds[i];
					}
				}
			}
			return null;
		}

		public void WarmUp()
		{
			JArray jArray = (JArray)Get<object>("player-ids", null);
			m_playerIds = ((jArray == null) ? new string[0] : jArray.ToObject<string[]>());
			jArray = (JArray)Get<object>("ranking", null);
			m_rankings = ((jArray == null) ? new DRLTournamentPlayerData[0] : jArray.ToObject<DRLTournamentPlayerData[]>());
			for (int i = 0; i < m_rankings.Length; i++)
			{
				m_rankings[i].WarmUp();
			}
			jArray = (JArray)Get<object>("rounds", null);
			m_rounds = ((jArray == null) ? new DRLTournamentRoundData[0] : jArray.ToObject<DRLTournamentRoundData[]>());
			for (int j = 0; j < m_rounds.Length; j++)
			{
				m_rounds[j].WarmUp();
			}
		}

		public bool IsPlayerRegistered(string p_player_id)
		{
			if (string.IsNullOrEmpty(p_player_id))
			{
				return false;
			}
			if (playerIds == null)
			{
				return false;
			}
			for (int i = 0; i < playerIds.Length; i++)
			{
				if (playerIds[i] == p_player_id)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsRacingInMatch(string p_steamID, string p_matchID)
		{
			DRLTournamentRoundData activeRound = GetActiveRound();
			if (activeRound == null)
			{
				return false;
			}
			for (int i = 0; i < activeRound.matches.Length; i++)
			{
				if (!(activeRound.matches[i].Id == p_matchID))
				{
					continue;
				}
				for (int j = 0; j < activeRound.matches[i].playerIds.Length; j++)
				{
					if (activeRound.matches[i].playerIds[j] == p_steamID)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
