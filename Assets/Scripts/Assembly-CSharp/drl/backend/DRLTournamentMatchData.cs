using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class DRLTournamentMatchData : SerializedData
	{
		private int m_index = -1;

		private string[] m_parents;

		private DRLTournamentPlayerData[] m_players;

		private string[] m_playerIds;

		private string[] m_playerOrder;

		private DRLTournamentReplayData[] m_replayURLs;

		private DRLTournamentScoreData[] m_scores;

		public string Id => Get("id", "");

		public int index
		{
			get
			{
				return m_index;
			}
			set
			{
				m_index = value;
			}
		}

		public string[] parents
		{
			get
			{
				if (m_parents != null)
				{
					return m_parents;
				}
				return new string[0];
			}
		}

		public string roundId => Get("round-id", "");

		public int roundIndex => Get("round-norder", 1) - 1;

		public string mapId => Get("map", "");

		public string trackId => Get("track", "");

		public bool isCustomMap => Get("is-custom-map", d: false);

		public string customMapId => Get("custom-map", "");

		public string customMapTitle => Get<string>("custom-map-title");

		public bool roomTimerAllowed => Get("multiplayer-room-timer", d: true);

		public bool isUnderReview => Get("is-under-review", d: false);

		public DRLTournamentPlayerData[] players
		{
			get
			{
				if (m_players != null)
				{
					return m_players;
				}
				return new DRLTournamentPlayerData[0];
			}
		}

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

		public string[] playerOrder
		{
			get
			{
				if (m_playerOrder != null)
				{
					return m_playerOrder;
				}
				return new string[0];
			}
		}

		public DRLTournamentReplayData[] replayURLs
		{
			get
			{
				if (m_replayURLs != null)
				{
					return m_replayURLs;
				}
				return new DRLTournamentReplayData[0];
			}
		}

		public int playersCount => Get<int>("players-size");

		public float throttleCap => Get("throttle-cap", 0f);

		public int currentHeat => Get<int>("current-heat");

		public int activeHeat => Get<int>("active-heat");

		public TournamentMatchState state
		{
			get
			{
				object obj = Get<object>("status", null);
				if (obj == null)
				{
					return TournamentMatchState.none;
				}
				return (TournamentMatchState)Enum.Parse(typeof(TournamentMatchState), obj.ToString());
			}
		}

		public bool invalid
		{
			get
			{
				TournamentMatchState tournamentMatchState = state;
				if (tournamentMatchState == TournamentMatchState.none || (uint)(tournamentMatchState - 4) <= 1u)
				{
					return true;
				}
				return false;
			}
		}

		public int groupNumber => Get("norder", 0);

		public int heatCount => Get("heats", 1);

		public int maxWinners => Get("num-winners", 1);

		public DRLTournamentScoreData[] scores
		{
			get
			{
				if (m_scores != null)
				{
					return m_scores;
				}
				return new DRLTournamentScoreData[0];
			}
		}

		public DateTime activeDate
		{
			get
			{
				string s = activeDateString;
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(s, out result);
				return result;
			}
		}

		public string activeDateString
		{
			get
			{
				object obj = Get<object>("start-at", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public DateTime completeDate
		{
			get
			{
				string s = completeDateString;
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(s, out result);
				return result;
			}
		}

		public string completeDateString
		{
			get
			{
				object obj = Get<object>("end-at", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
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

		public TimeSpan elapsedTime => currentTime - activeDate;

		public TimeSpan remainingTime => completeDate - currentTime;

		public int progress => Get("progress", 1);

		public float totalScore
		{
			get
			{
				if (scores == null)
				{
					return 0f;
				}
				float num = 0f;
				for (int i = 0; i < scores.Length; i++)
				{
					num += (float.IsNaN(scores[i].score) ? 0f : ((float)scores[i].score));
				}
				return num;
			}
		}

		public int droneClass => Get("default-drone-class", 0);

		public string mode
		{
			get
			{
				object obj = Get<object>("mode", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public TournamentRoundGameMode gameMode => mode switch
		{
			"match_points" => TournamentRoundGameMode.matchPoints, 
			"match_leaderboard" => TournamentRoundGameMode.matchLeaderboard, 
			"match_timesum" => TournamentRoundGameMode.matchTimeSum, 
			"leaderboard" => TournamentRoundGameMode.leaderboard, 
			"sudden_death" => TournamentRoundGameMode.suddenDeath, 
			"golden_heat" => TournamentRoundGameMode.goldenHeat, 
			_ => TournamentRoundGameMode.none, 
		};

		public bool ContainsPlayer(string p_id)
		{
			if (m_playerIds == null || m_playerIds.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < m_playerIds.Length; i++)
			{
				if (m_playerIds[i] == p_id)
				{
					return true;
				}
			}
			return false;
		}

		public DRLTournamentPlayerData GetPlayerById(string p_id)
		{
			if (m_playerIds == null || m_playerIds.Length == 0)
			{
				return null;
			}
			for (int i = 0; i < m_players.Length; i++)
			{
				if (m_players[i].playerId == p_id)
				{
					return m_players[i];
				}
			}
			return null;
		}

		internal void WarmUp()
		{
			try
			{
				JArray jArray = (JArray)Get<object>("parents", null);
				m_parents = ((jArray == null) ? new string[0] : jArray.ToObject<string[]>());
				jArray = (JArray)Get<object>("player-ids", null);
				m_playerIds = ((jArray == null) ? new string[0] : jArray.ToObject<string[]>());
				jArray = (JArray)Get<object>("player-order", null);
				m_playerOrder = ((jArray == null) ? new string[0] : jArray.ToObject<string[]>());
				jArray = (JArray)Get<object>("players", null);
				m_players = ((jArray == null) ? new DRLTournamentPlayerData[0] : jArray.ToObject<DRLTournamentPlayerData[]>());
				for (int i = 0; i < m_players.Length; i++)
				{
					m_players[i].WarmUp();
				}
				jArray = (JArray)Get<object>("scores", null);
				m_scores = ((jArray == null) ? new DRLTournamentScoreData[0] : jArray.ToObject<DRLTournamentScoreData[]>());
				jArray = (JArray)Get<object>("replay-urls", null);
				m_replayURLs = ((jArray == null) ? new DRLTournamentReplayData[0] : jArray.ToObject<DRLTournamentReplayData[]>());
			}
			catch (Exception ex)
			{
				Debug.LogWarning("DRLTournamentMatchData> WarmUp: error on deserializing data!\n" + ex.Message);
			}
		}
	}
}
