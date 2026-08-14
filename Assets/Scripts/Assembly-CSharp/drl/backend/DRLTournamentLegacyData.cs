using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class DRLTournamentLegacyData : SerializedData
	{
		[Serializable]
		public class PlayerData
		{
			public string playerId;

			public string profileName;

			public Color profileColor;

			public string profileThumbURL;

			public List<float> points;

			public List<float> scores;

			public float totalPoint
			{
				get
				{
					if (points == null)
					{
						return 0f;
					}
					float num = 0f;
					for (int i = 0; i < points.Count; i++)
					{
						num += (float.IsNaN(points[i]) ? 0f : points[i]);
					}
					return num;
				}
			}

			public float totalScore
			{
				get
				{
					if (scores == null)
					{
						return 0f;
					}
					float num = 0f;
					for (int i = 0; i < scores.Count; i++)
					{
						num += (float.IsNaN(scores[i]) ? 0f : scores[i]);
					}
					return num;
				}
			}
		}

		private DRLRaceResultData[] m_results;

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

		public string name
		{
			get
			{
				return Get("name", "");
			}
			set
			{
				Set("name", value);
			}
		}

		public int heats
		{
			get
			{
				return Get("heats", 5);
			}
			set
			{
				Set("heats", value);
			}
		}

		public DRLRaceResultData[] results
		{
			get
			{
				if (m_results != null)
				{
					return m_results;
				}
				object obj = Get<object>("results", null);
				string p_data = ((obj == null) ? "[]" : obj.ToString());
				return m_results = Serialize.FromJson<DRLRaceResultData[]>(p_data);
			}
			set
			{
				m_results = value;
			}
		}

		public int order
		{
			get
			{
				DRLRaceResultData[] array = results;
				int num = 0;
				if (array.Length == 0)
				{
					return num;
				}
				for (int i = 0; i < array.Length; i++)
				{
					num = Mathf.Max(array[i].order);
				}
				return num + 1;
			}
		}

		public List<PlayerData> players
		{
			get
			{
				DRLRaceResultData[] array = results;
				List<PlayerData> list = new List<PlayerData>();
				foreach (DRLRaceResultData dRLRaceResultData in array)
				{
					PlayerData playerData = new PlayerData();
					playerData.playerId = dRLRaceResultData.playerId;
					playerData.profileName = dRLRaceResultData.profileName;
					if (string.IsNullOrEmpty(playerData.profileName))
					{
						playerData.profileName = "P" + playerData.playerId;
					}
					playerData.profileThumbURL = dRLRaceResultData.profileThumbURL;
					playerData.profileColor = dRLRaceResultData.profileColor;
					playerData.points = new List<float>();
					playerData.scores = new List<float>();
					list.Add(playerData);
				}
				list.Sort((PlayerData pda, PlayerData pdb) => string.Compare(pda.profileName, pdb.profileName));
				for (int num = 0; num < list.Count; num++)
				{
					for (int num2 = num + 1; num2 < list.Count; num2++)
					{
						if (list[num].playerId == list[num2].playerId)
						{
							list.RemoveAt(num2--);
						}
					}
				}
				return list;
			}
		}

		public List<DRLRaceResultData> GetResultsByOrder(int p_order)
		{
			List<DRLRaceResultData> list = new List<DRLRaceResultData>();
			DRLRaceResultData[] array = results;
			foreach (DRLRaceResultData dRLRaceResultData in array)
			{
				if (dRLRaceResultData.order == p_order)
				{
					list.Add(dRLRaceResultData);
				}
			}
			return list;
		}

		public List<PlayerData> GetPlayerPoints(int p_max_results, params float[] p_points_per_result)
		{
			List<PlayerData> list = players;
			for (int i = 0; i < list.Count; i++)
			{
				list[i].points = new List<float>();
			}
			Debug.Log("DRLTournamentData> GetPlayerPoints - max-heats[" + p_max_results + "]");
			for (int j = 0; j < p_max_results; j++)
			{
				List<DRLRaceResultData> resultsByOrder = GetResultsByOrder(j);
				resultsByOrder.Sort(DRLRaceResultData.SortByScore(ScoreType.TimeMin));
				for (int k = 0; k < list.Count; k++)
				{
					PlayerData playerData = list[k];
					int playerIndex = GetPlayerIndex(playerData.playerId, resultsByOrder);
					int num = Mathf.Min(playerIndex, p_points_per_result.Length - 1);
					float num2 = ((num < 0) ? 0f : p_points_per_result[num]);
					float item = ((playerIndex >= 0) ? num2 : float.NaN);
					float playerScore = GetPlayerScore(playerData.playerId, resultsByOrder);
					if (float.IsNaN(playerScore))
					{
						item = float.NaN;
					}
					Debug.Log("DRLTournamentData> Result - heat[" + j + "] player[" + k + "/" + playerData.playerId + "/" + playerData.profileName + "] idx[" + playerIndex + "] point[" + item + "] score[" + playerScore + "]");
					playerData.points.Add(item);
					playerData.scores.Add(playerScore);
				}
				Debug.Log("DRLTournamentData> ================= ");
			}
			list.Sort(GetPlayerPointSort());
			return list;
		}

		public int GetPlayerIndex(string p_player_id, List<DRLRaceResultData> p_results)
		{
			for (int i = 0; i < p_results.Count; i++)
			{
				if (p_results[i].playerId == p_player_id)
				{
					return i;
				}
			}
			return -1;
		}

		public float GetPlayerScore(string p_player_id, List<DRLRaceResultData> p_results, float p_default = float.NaN)
		{
			for (int i = 0; i < p_results.Count; i++)
			{
				if (!(p_results[i].playerId != p_player_id) && p_results[i].status != ResultStatusType.Timeout && p_results[i].status != ResultStatusType.Crash && p_results[i].status != ResultStatusType.Quit)
				{
					return p_results[i].score;
				}
			}
			return p_default;
		}

		public static Comparison<PlayerData> GetPlayerPointSort(int p_order = -1)
		{
			return delegate(PlayerData a, PlayerData b)
			{
				float totalPoint = a.totalPoint;
				float totalPoint2 = b.totalPoint;
				if (p_order < 0)
				{
					if (Mathf.Abs(totalPoint - totalPoint2) <= 0f)
					{
						return string.Compare(a.profileName, b.profileName);
					}
					if (!(totalPoint > totalPoint2))
					{
						return 1;
					}
					return -1;
				}
				if (p_order >= a.points.Count)
				{
					return 0;
				}
				if (p_order >= b.points.Count)
				{
					return 0;
				}
				totalPoint = a.points[p_order];
				totalPoint2 = b.points[p_order];
				if (float.IsNaN(totalPoint) && float.IsNaN(totalPoint2))
				{
					return 0;
				}
				if (float.IsNaN(totalPoint))
				{
					return 1;
				}
				if (float.IsNaN(totalPoint2))
				{
					return -1;
				}
				if (Mathf.Abs(totalPoint - totalPoint2) <= 0f)
				{
					return string.Compare(a.profileName, b.profileName);
				}
				return (!(totalPoint > totalPoint2)) ? 1 : (-1);
			};
		}

		public static Comparison<PlayerData> GetPlayerScoreSort(int p_order = -1, int p_default = 0)
		{
			return delegate(PlayerData a, PlayerData b)
			{
				float totalScore = a.totalScore;
				float totalScore2 = b.totalScore;
				if (p_order < 0)
				{
					if (Mathf.Abs(totalScore - totalScore2) <= 0f)
					{
						return string.Compare(a.profileName, b.profileName);
					}
					if (!(totalScore < totalScore2))
					{
						return 1;
					}
					return -1;
				}
				if (p_order >= a.points.Count)
				{
					return p_default;
				}
				if (p_order >= b.points.Count)
				{
					return p_default;
				}
				totalScore = a.scores[p_order];
				totalScore2 = b.scores[p_order];
				if (float.IsNaN(totalScore) && float.IsNaN(totalScore2))
				{
					return p_default;
				}
				if (float.IsNaN(totalScore))
				{
					return 1;
				}
				if (float.IsNaN(totalScore2))
				{
					return -1;
				}
				if (Mathf.Abs(totalScore - totalScore2) <= 0f)
				{
					return string.Compare(a.profileName, b.profileName);
				}
				return (!(totalScore < totalScore2)) ? 1 : (-1);
			};
		}
	}
}
