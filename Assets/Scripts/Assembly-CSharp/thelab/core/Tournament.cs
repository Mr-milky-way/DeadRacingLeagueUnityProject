using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using thelabe.core;

namespace thelab.core
{
	[Serializable]
	public class Tournament
	{
		public List<TPlayer> players;

		public List<TMatch> matches;

		public List<TMatchResult> results;

		public List<List<TPlayer>> winners;

		public int levelCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < matches.Count; i++)
				{
					num = Mathf.Max(matches[i].level + 1, num);
				}
				return num;
			}
		}

		public Tournament()
		{
			matches = new List<TMatch>();
			players = new List<TPlayer>();
			results = new List<TMatchResult>();
		}

		public void GenerateRandomPlayers(int p_count)
		{
			List<TPlayer> list = new List<TPlayer>();
			for (int i = 0; i < p_count; i++)
			{
				TPlayer tPlayer = new TPlayer();
				tPlayer.name = "P-" + i.ToString("X4");
				tPlayer.id = i;
				tPlayer.skill = UnityEngine.Random.value;
				list.Add(tPlayer);
			}
			list.Sort((TPlayer a, TPlayer b) => (!(a.skill > b.skill)) ? 1 : (-1));
			players = list;
		}

		public void Generate(int[] p_players_per_match, int[] p_heats_per_match, int[] p_winners_per_level)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			matches = new List<TMatch>();
			while (num < 100)
			{
				num++;
				Get(num2 - 1, p_players_per_match);
				int p_wpl = Get(num2 - 1, p_winners_per_level);
				int p_ppm = Get(num2, p_players_per_match);
				int p_hpm = Get(num2, p_heats_per_match);
				int p_wpl2 = Get(num2, p_winners_per_level);
				List<TPlayer> winnersByLevel = GetWinnersByLevel(num2 - 1);
				int maxWinners = GetMaxWinners(num2 - 1, p_wpl);
				while (winnersByLevel.Count > maxWinners)
				{
					winnersByLevel.RemoveAt(winnersByLevel.Count - 1);
				}
				int num4 = winnersByLevel.Count;
				if (num4 < maxWinners)
				{
					num4 = GetMaxWinners(num2 - 1, p_wpl2);
				}
				int matchCount = GetMatchCount(num4, p_ppm);
				List<TMatch> list = GenerateMatches(num2, matchCount, num3, p_hpm, p_wpl2);
				num3 += list.Count;
				AddMatchesPlayers(list, num2 <= 0, p_ppm, num4);
				BalanceMatches(list, p_ppm);
				matches.AddRange(list);
				num2++;
				if (matchCount <= 1)
				{
					break;
				}
			}
		}

		public int GetMaxWinners(int p_level, int p_wpl)
		{
			if (p_level < 0)
			{
				return players.Count;
			}
			return GetMatchesByLevel(p_level).Count * p_wpl;
		}

		public List<TMatch> GenerateMatches(int p_level, int p_count, int p_base_id, int p_hpm, int p_wpl)
		{
			List<TMatch> list = new List<TMatch>();
			for (int i = 0; i < p_count; i++)
			{
				TMatch tMatch = new TMatch();
				tMatch.id = p_base_id++;
				tMatch.name = "M" + p_level + "-" + tMatch.id.ToString("X4");
				tMatch.players = new List<int>();
				tMatch.maxWinners = ((p_count <= 1) ? 1 : p_wpl);
				tMatch.heatCount = p_hpm;
				tMatch.level = p_level;
				list.Add(tMatch);
			}
			return list;
		}

		public void AddMatchesPlayers(List<TMatch> p_matches, bool p_seeded, int p_ppm, int p_max_players)
		{
			int num = 0;
			for (int i = 0; i < p_matches.Count; i++)
			{
				if (num >= p_max_players)
				{
					break;
				}
				TMatch tMatch = p_matches[i];
				int num2 = (p_seeded ? 1 : p_ppm);
				for (int j = 0; j < num2; j++)
				{
					tMatch.players.Add(num);
					num++;
					if (num >= p_max_players)
					{
						break;
					}
				}
				if (i >= p_matches.Count - 1)
				{
					i = -1;
				}
			}
		}

		public int GetBalanceCount(List<TMatch> p_list, int p_ppm)
		{
			if (p_list.Count <= 1)
			{
				return 0;
			}
			return Mathf.Max(0, p_ppm - p_list[p_list.Count - 1].players.Count);
		}

		public void BalanceMatches(List<TMatch> p_matches, int p_ppm)
		{
			TMatch tMatch = p_matches[p_matches.Count - 1];
			int balanceCount = GetBalanceCount(p_matches, p_ppm - 1);
			int num = 0;
			int num2 = p_matches.Count - 2;
			int num3 = num2;
			int num4 = tMatch.players.Count - 1;
			for (int i = 0; i < balanceCount; i++)
			{
				TMatch tMatch2 = p_matches[num3];
				int count = tMatch2.players.Count;
				int count2 = tMatch.players.Count;
				num3--;
				if (num3 < num)
				{
					num3 = num2;
				}
				if (count - count2 > 1)
				{
					int index = num4 % count;
					int item = tMatch2.players[index];
					tMatch2.players.RemoveAt(index);
					tMatch.players.Add(item);
					num4++;
				}
			}
		}

		public int GetWinnersPerMatch(int p_level)
		{
			List<TMatch> matchesByLevel = GetMatchesByLevel(Mathf.Max(p_level, 0));
			if (matchesByLevel.Count > 0)
			{
				return matchesByLevel[0].maxWinners;
			}
			return 0;
		}

		public List<TPlayer> GetWinnersByLevel(int p_level)
		{
			if (p_level < 0)
			{
				return players;
			}
			List<TPlayer> list = new List<TPlayer>();
			List<TMatch> matchesByLevel = GetMatchesByLevel(p_level);
			for (int i = 0; i < matchesByLevel.Count; i++)
			{
				int maxWinners = matchesByLevel[i].maxWinners;
				List<TPlayer> winnersByMatch = GetWinnersByMatch(matchesByLevel[i]);
				while (winnersByMatch.Count > maxWinners)
				{
					winnersByMatch.RemoveAt(winnersByMatch.Count - 1);
				}
				list.AddRange(winnersByMatch);
			}
			return list;
		}

		public bool IsLevelComplete(int p_level)
		{
			List<TMatch> matchesByLevel = GetMatchesByLevel(p_level);
			for (int i = 0; i < matchesByLevel.Count; i++)
			{
				if (!IsMatchComplete(matchesByLevel[i]))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsMatchComplete(int p_id)
		{
			TMatch matchById = GetMatchById(p_id);
			return GetResultsByMatch(matchById).Count >= matchById.maxResults;
		}

		public bool IsMatchComplete(TMatch p_match)
		{
			return IsMatchComplete(p_match.id);
		}

		public TPlayer GetPlayerById(int p_id)
		{
			return players.Find((TPlayer it) => it.id == p_id);
		}

		public List<TMatch> GetMatchesByLevel(int p_level)
		{
			return matches.FindAll((TMatch it) => it.level == p_level);
		}

		public TMatch GetMatchById(int p_id)
		{
			return matches.Find((TMatch it) => it.id == p_id);
		}

		public List<TMatchResult> GetResultsByMatchId(int p_id)
		{
			return results.FindAll((TMatchResult it) => it.match == p_id);
		}

		public List<TMatchResult> GetResultsByMatch(TMatch p_match)
		{
			return GetResultsByMatchId(p_match.id);
		}

		public bool GetResultScoreSumByPlayer(int p_level, TPlayer p_player, out int p_count)
		{
			List<TMatchResult> resultsByLevel = GetResultsByLevel(p_level);
			return GetResultScoreSumByPlayer(resultsByLevel, p_player, out p_count);
		}

		public bool GetResultScoreSumByPlayer(List<TMatchResult> p_list, TPlayer p_player, out int p_count)
		{
			List<TMatchResult> list = new List<TMatchResult>(p_list);
			list.RemoveAll((TMatchResult it) => it.player != p_player.id);
			p_count = -1;
			if (list.Count <= 0)
			{
				return false;
			}
			int num = 0;
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				num += list[num2].score;
			}
			p_count = num;
			return true;
		}

		public List<TMatchResult> GetResultsByLevel(int p_level)
		{
			return results.FindAll((TMatchResult it) => it.level == p_level);
		}

		public void RemoveResultsByLevel(int p_level)
		{
			results.RemoveAll((TMatchResult it) => it.level == p_level);
		}

		public List<TPlayer> GetWinnersByResults(List<TMatchResult> p_results)
		{
			List<TPlayer> list = new List<TPlayer>();
			for (int i = 0; i < p_results.Count; i++)
			{
				TPlayer playerById = GetPlayerById(p_results[i].player);
				if (!list.Contains(playerById))
				{
					list.Add(playerById);
				}
			}
			list.Sort(delegate(TPlayer a, TPlayer b)
			{
				int p_count = 0;
				GetResultScoreSumByPlayer(p_results, a, out p_count);
				int p_count2 = 0;
				GetResultScoreSumByPlayer(p_results, b, out p_count2);
				return (p_count != p_count2) ? ((p_count >= p_count2) ? 1 : (-1)) : 0;
			});
			return list;
		}

		public List<TPlayer> GetWinnersByMatch(int p_match_id)
		{
			return GetWinnersByMatch(GetMatchById(p_match_id));
		}

		public List<TPlayer> GetWinnersByMatch(TMatch p_match)
		{
			List<TMatchResult> resultsByMatch = GetResultsByMatch(p_match);
			int maxResults = p_match.maxResults;
			List<TPlayer> list = new List<TPlayer>();
			if (resultsByMatch.Count < maxResults)
			{
				for (int i = 0; i < p_match.maxWinners; i++)
				{
					TPlayer placeHolderPlayer = GetPlaceHolderPlayer(i, p_match);
					list.Add(placeHolderPlayer);
				}
			}
			else
			{
				list.AddRange(GetWinnersByResults(resultsByMatch));
			}
			return list;
		}

		public void GenerateResults(int p_level)
		{
			results.Clear();
			int num = 0;
			int num2 = Mathf.Min(p_level, levelCount);
			for (int i = 0; i < num2; i++)
			{
				List<TMatch> matchesByLevel = GetMatchesByLevel(i);
				List<TPlayer> winnersByLevel = GetWinnersByLevel(i - 1);
				for (int j = 0; j < matchesByLevel.Count; j++)
				{
					TMatch tMatch = matchesByLevel[j];
					int count = tMatch.players.Count;
					for (int k = 0; k < count; k++)
					{
						for (int l = 0; l < tMatch.heatCount; l++)
						{
							TMatchResult tMatchResult = new TMatchResult();
							tMatchResult.id = num++;
							tMatchResult.level = i;
							tMatchResult.match = tMatch.id;
							tMatchResult.heat = k;
							tMatchResult.player = winnersByLevel[tMatch.players[k]].id;
							tMatchResult.score = UnityEngine.Random.Range(30000, 60000);
							results.Add(tMatchResult);
						}
					}
				}
			}
		}

		public TPlayer GetPlaceHolderPlayer(int p_index, TMatch p_match)
		{
			return new TPlayer
			{
				name = "WINNER #" + (p_index + 1) + " - " + p_match.name,
				skill = 0f,
				isPlaceHolder = true,
				id = 0
			};
		}

		public int Clamp(int p_index, IList p_list)
		{
			return Mathf.Clamp(p_index, 0, p_list.Count - 1);
		}

		public int Get(int p_index, IList p_list)
		{
			return (int)p_list[Clamp(p_index, p_list)];
		}

		public int GetMatchCount(int p_player_count, int p_ppm)
		{
			int num = p_player_count % p_ppm;
			return p_player_count / p_ppm + ((num > 0) ? 1 : 0);
		}
	}
}
