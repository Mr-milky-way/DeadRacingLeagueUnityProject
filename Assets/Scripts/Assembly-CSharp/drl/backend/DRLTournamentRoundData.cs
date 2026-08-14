using System;
using Newtonsoft.Json.Linq;
using thelab.core;

namespace drl.backend
{
	public class DRLTournamentRoundData : SerializedData
	{
		private DRLTournamentMatchData[] m_matches;

		public TournamentRoundState state
		{
			get
			{
				object obj = Get<object>("status", null);
				if (obj == null)
				{
					return TournamentRoundState.none;
				}
				return (TournamentRoundState)Enum.Parse(typeof(TournamentRoundState), obj.ToString());
			}
		}

		public int order => Get("norder", 0);

		public int index => order - 1;

		public string title
		{
			get
			{
				object obj = Get<object>("title", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return string.Empty;
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

		public string mapId => Get("map", "");

		public string trackId => Get("track", "");

		public bool isCustomMap => Get("is-custom-map", d: false);

		public string customMapId => Get("custom-map", "");

		public string customMapTitle => Get<string>("custom-map-title");

		public bool multiplayerCountdown => Get("multiplayer-countdown", d: true);

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

		public int timeout => Get("timeout", 0);

		public int totalPlayerCount { get; protected set; }

		public DRLTournamentMatchData[] matches
		{
			get
			{
				if (m_matches != null)
				{
					return m_matches;
				}
				return new DRLTournamentMatchData[0];
			}
		}

		public int GetPlayerMatchIndex(string p_id)
		{
			if (matches == null)
			{
				return -1;
			}
			for (int i = 0; i < matches.Length; i++)
			{
				if (matches[i].ContainsPlayer(p_id))
				{
					return i;
				}
			}
			return -1;
		}

		public DRLTournamentMatchData GetPlayerMatch(string p_id)
		{
			int playerMatchIndex = GetPlayerMatchIndex(p_id);
			if (playerMatchIndex < 0)
			{
				return null;
			}
			return matches[playerMatchIndex];
		}

		internal void WarmUp()
		{
			JArray jArray = (JArray)Get<object>("matches", null);
			m_matches = ((jArray == null) ? new DRLTournamentMatchData[0] : jArray.ToObject<DRLTournamentMatchData[]>());
			totalPlayerCount = 0;
			for (int i = 0; i < m_matches.Length; i++)
			{
				m_matches[i].WarmUp();
				totalPlayerCount += m_matches[i].playersCount;
			}
		}
	}
}
