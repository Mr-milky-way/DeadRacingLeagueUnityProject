using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class DTournament
	{
		[Serializable]
		public class Player
		{
			public int id;

			public string name;

			public float skill;
		}

		[Serializable]
		public class Match
		{
			public int id;

			public int level;

			public string name;

			public List<int> players;

			public int maxWinners;

			public int heatCount;
		}

		public List<Player> players;

		public List<Match> matches;

		public int defaultPPM = 6;

		public int defaultHeatCount = 3;

		public int totalLevels
		{
			get
			{
				int num = 0;
				for (int i = 0; i < matches.Count; i++)
				{
					num = Mathf.Max(matches[i].level, num);
				}
				if (matches.Count <= 0)
				{
					return num;
				}
				return num + 1;
			}
		}

		public int defaultMaxWinner => defaultPPM / 2;

		public static int GetMatchCount(List<Match> p_list, int p_level)
		{
			int num = 0;
			for (int i = 0; i < p_list.Count && p_list[i].level <= p_level; i++)
			{
				if (p_list[i].level == p_level)
				{
					num++;
				}
			}
			return num;
		}

		public static Match GetMatchByIndex(List<Match> p_list, int p_level, int p_index)
		{
			int num = 0;
			for (int i = 0; i < p_list.Count && p_list[i].level <= p_level; i++)
			{
				if (p_list[i].level == p_level)
				{
					if (num == p_index)
					{
						return p_list[i];
					}
					num++;
				}
			}
			return null;
		}

		public static Match GetTailMatch(List<Match> p_list, int p_level)
		{
			int matchCount = GetMatchCount(p_list, p_level);
			if (matchCount <= 0)
			{
				return null;
			}
			return GetMatchByIndex(p_list, p_level, matchCount - 1);
		}

		public static int GetMaxMatchCount(int p_total, int p_ppm)
		{
			int num = p_total % p_ppm;
			return p_total / p_ppm + ((num > 0) ? 1 : 0);
		}

		public static int GetRemainingWinnersByDepth(List<Match> p_list, int p_level, int p_max_winners, int p_default)
		{
			if (p_list.Count <= 0)
			{
				return p_default;
			}
			if (p_level < 0)
			{
				return p_default;
			}
			int num = 0;
			for (int i = 0; i < p_list.Count; i++)
			{
				Match match = p_list[i];
				if (match.level == p_level)
				{
					int num2 = Mathf.Min(p_max_winners, match.players.Count);
					num += num2;
				}
			}
			return num;
		}

		public static int GetBalanceCount(List<Match> p_list, int p_ppm)
		{
			if (p_list.Count <= 1)
			{
				return 0;
			}
			return Mathf.Max(0, p_ppm - p_list[p_list.Count - 1].players.Count);
		}

		public static List<Match> GenerateMatches(int p_max_players, int p_max_winners, int p_ppm, int p_heat_count)
		{
			return GenerateMatches(p_max_players, new int[1] { p_max_winners }, p_ppm, new int[1] { p_heat_count });
		}

		public static List<Match> GenerateMatches(int p_max_players, int[] p_max_winners, int p_ppm, int[] p_heat_count)
		{
			int num = Mathf.Max(2, (p_ppm <= 0) ? 2 : p_ppm);
			int num2 = 1;
			int num3 = 1;
			int num4 = 0;
			int num5 = 0;
			List<Match> list = new List<Match>();
			Debug.Log("Tournament> GenerateMatches / " + p_max_players + " Players / " + num + " Players Per Match / [" + string.Join(",", p_max_winners) + "] Winners Per Level");
			int num6 = 0;
			while (num4 < 1500)
			{
				num4++;
				int num7 = Mathf.Clamp(num5, 0, p_max_winners.Length - 1);
				int num8 = Mathf.Clamp(num5, 0, p_heat_count.Length - 1);
				num2 = p_max_winners[num7];
				num3 = p_max_winners[num8];
				num2 = Mathf.Max(1, (num2 <= 0) ? 1 : num2);
				num3 = Mathf.Max(1, (num3 <= 0) ? 1 : num3);
				num7 = Mathf.Clamp(num5 - 1, 0, p_max_winners.Length - 1);
				int p_max_winners2 = p_max_winners[num7];
				int remainingWinnersByDepth = GetRemainingWinnersByDepth(list, num5 - 1, p_max_winners2, p_max_players);
				int maxMatchCount = GetMaxMatchCount(remainingWinnersByDepth, num);
				Debug.Log("Level: " + num5 + " / " + maxMatchCount + " Matches / " + remainingWinnersByDepth + " Players / " + num2 + " Winners Per Match");
				for (int i = 0; i < maxMatchCount; i++)
				{
					Match match = new Match();
					match.id = num6++;
					match.name = "M" + num5 + "-" + match.id.ToString("X4");
					match.players = new List<int>();
					match.maxWinners = ((maxMatchCount <= 1) ? 1 : num2);
					match.heatCount = num3;
					match.level = num5;
					list.Add(match);
				}
				int num9 = 0;
				if (num5 <= 0)
				{
					while (num9 < remainingWinnersByDepth)
					{
						for (int j = 0; j < list.Count; j++)
						{
							Match match2 = list[j];
							if (match2.level == num5)
							{
								match2.players.Add(num9);
								num9++;
								if (num9 >= remainingWinnersByDepth)
								{
									break;
								}
							}
						}
					}
				}
				else
				{
					for (int k = 0; k < list.Count; k++)
					{
						Match match3 = list[k];
						if (match3.level != num5)
						{
							continue;
						}
						for (int l = 0; l < num; l++)
						{
							if (num9 >= remainingWinnersByDepth)
							{
								break;
							}
							match3.players.Add(num9++);
						}
					}
				}
				if (maxMatchCount <= 1)
				{
					break;
				}
				Match tailMatch = GetTailMatch(list, num5);
				int balanceCount = GetBalanceCount(list, num - 1);
				int num10 = list.IndexOf(GetMatchByIndex(list, num5, 0));
				int num11 = list.IndexOf(tailMatch) - 1;
				int num12 = num11;
				int num13 = tailMatch.players.Count - 1;
				Debug.Log("Tail Match: " + tailMatch.name + " / Players: " + tailMatch.players.Count + " / Needs: " + balanceCount + " for " + (num - 1) + " Players / Swap Start: " + num13);
				for (int m = 0; m < balanceCount; m++)
				{
					Match match4 = list[num12];
					int count = match4.players.Count;
					int count2 = tailMatch.players.Count;
					if (count - count2 > 1)
					{
						int index = num13 % count;
						int item = match4.players[index];
						match4.players.RemoveAt(index);
						tailMatch.players.Add(item);
						num13++;
						Debug.Log("  Swap / " + match4.name + " [" + index + "] / tail-count[" + tailMatch.players.Count + "]");
					}
					num12--;
					if (num12 < num10)
					{
						num12 = num11;
					}
				}
				num5++;
			}
			Debug.Log("======");
			return list;
		}

		public static List<Player> GenerateRandomPlayers(int p_count)
		{
			List<Player> list = new List<Player>();
			for (int i = 0; i < p_count; i++)
			{
				Player player = new Player();
				player.name = "P-" + i.ToString("X4");
				player.id = i;
				player.skill = UnityEngine.Random.value;
				list.Add(player);
			}
			return list;
		}

		public static List<Player> GetPlaceHolderPlayers(List<Match> p_list, int p_level, int p_max_winners)
		{
			int a = Mathf.Max(1, p_max_winners);
			int matchCount = GetMatchCount(p_list, p_level);
			Debug.Log("Tournament> GetPlaceHolderPlayers / level[" + p_level + "] / max-winners[" + a + "] match-count[" + matchCount + "]");
			List<Player> list = new List<Player>();
			for (int i = 0; i < matchCount; i++)
			{
				Match matchByIndex = GetMatchByIndex(p_list, p_level, i);
				int num = Mathf.Min(a, matchByIndex.players.Count);
				for (int j = 0; j < num; j++)
				{
					Player player = new Player();
					player.name = "Winner " + matchByIndex.name + " " + (j + 1);
					list.Add(player);
				}
			}
			return list;
		}

		public DTournament()
		{
			matches = new List<Match>();
			players = new List<Player>();
		}

		public void GeneratePlayers(int p_count)
		{
			players = GenerateRandomPlayers(p_count);
		}

		public void GenerateMatches(int p_max_winners, int p_ppm, int p_max_heats)
		{
			matches = GenerateMatches(players.Count, new int[1] { p_max_winners }, p_ppm, new int[1] { p_max_heats });
		}

		public void GenerateMatches(int[] p_max_winners, int p_ppm, int[] p_max_heats)
		{
			int[] array = p_max_winners;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Mathf.Max(1, (array[i] > defaultMaxWinner) ? 1 : array[i]);
			}
			array = p_max_heats;
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = ((array[j] <= 0) ? 1 : array[j]);
			}
			Mathf.Max(2, (p_ppm <= 0) ? defaultPPM : p_ppm);
			matches = GenerateMatches(players.Count, p_max_winners, p_ppm, p_max_heats);
		}

		public void GenerateMatches(int p_max_winners, int p_ppm)
		{
			GenerateMatches(p_max_winners, p_ppm, 1);
		}

		public void GenerateMatches(int p_max_winners)
		{
			GenerateMatches(p_max_winners, 2, 1);
		}

		public void GenerateMatches()
		{
			GenerateMatches(1, 2, 1);
		}

		public string Print()
		{
			int num = totalLevels;
			List<Match> list = matches;
			string text = "";
			if (list.Count <= 0)
			{
				return text;
			}
			text = text + "Tournament / " + num + " Levels / " + list.Count + " Matches / " + players.Count + " Players\n";
			for (int i = 0; i < num; i++)
			{
				int matchCount = GetMatchCount(list, i);
				if (matchCount <= 0)
				{
					continue;
				}
				Match match = ((i <= 0) ? null : GetMatchByIndex(list, i - 1, 0));
				List<Player> list2 = ((i <= 0) ? players : GetPlaceHolderPlayers(list, i - 1, match.maxWinners));
				if (i <= 0)
				{
					list2.Sort((Player a, Player b) => (a.skill < b.skill) ? 1 : (-1));
				}
				text = text + " Level " + i + " / " + matchCount + " Matches / " + list2.Count + " Players\n";
				for (int num2 = 0; num2 < matchCount; num2++)
				{
					Match matchByIndex = GetMatchByIndex(list, i, num2);
					List<int> list3 = matchByIndex.players;
					text = text + "  Match " + matchByIndex.id + " " + matchByIndex.name + ": " + list3.Count + " Players / " + matchByIndex.heatCount + " Heats / " + matchByIndex.maxWinners + " Max Winner\n";
					for (int num3 = 0; num3 < list3.Count; num3++)
					{
						int num4 = list3[num3];
						Player player = ((num4 < 0) ? null : ((num4 >= list2.Count) ? null : list2[num4]));
						text = ((player == null) ? (text + "   " + num3 + " null S[" + 0f + "]\n") : (text + "   " + num3 + " " + player.name + " S[" + player.skill.ToString("0.00") + "]\n"));
					}
				}
			}
			return text;
		}
	}
}
