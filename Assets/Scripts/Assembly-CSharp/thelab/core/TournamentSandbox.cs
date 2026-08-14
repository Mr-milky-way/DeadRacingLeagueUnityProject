using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelabe.core;

namespace thelab.core
{
	public class TournamentSandbox : MonoBehaviour
	{
		public InputField inputField;

		public Button buttonField;

		public Tournament data;

		public List<Text> levelFields;

		public List<Text> resultsFields;

		public string input => inputField.text;

		protected void Awake()
		{
			buttonField.onClick.AddListener(delegate
			{
				Apply();
			});
		}

		public void Apply()
		{
			string[] array = input.Split('|');
			if (array.Length < 4)
			{
				Debug.LogWarning("TournamentSandbox> Invalid Args Count");
				return;
			}
			int result = 0;
			int[] p_players_per_match = new int[0];
			int[] p_heats_per_match = new int[0];
			int[] p_winners_per_level = new int[0];
			if (array.Length != 0)
			{
				int.TryParse(array[0].Trim(), out result);
			}
			if (array.Length > 1)
			{
				string[] array2 = array[1].Trim().Split(',');
				int[] array3 = new int[array2.Length];
				for (int i = 0; i < array3.Length; i++)
				{
					int.TryParse(array2[i], out array3[i]);
				}
				p_players_per_match = array3;
			}
			if (array.Length > 2)
			{
				string[] array4 = array[2].Trim().Split(',');
				int[] array3 = new int[array4.Length];
				for (int j = 0; j < array3.Length; j++)
				{
					int.TryParse(array4[j], out array3[j]);
				}
				p_heats_per_match = array3;
			}
			if (array.Length > 3)
			{
				string[] array5 = array[3].Trim().Split(',');
				int[] array3 = new int[array5.Length];
				for (int k = 0; k < array3.Length; k++)
				{
					int.TryParse(array5[k], out array3[k]);
				}
				p_winners_per_level = array3;
			}
			int result2 = -1;
			if (array.Length > 4)
			{
				int.TryParse(array[4].Trim(), out result2);
			}
			data = new Tournament();
			data.GenerateRandomPlayers(result);
			data.Generate(p_players_per_match, p_heats_per_match, p_winners_per_level);
			if (result2 >= 0)
			{
				data.GenerateResults(result2);
				data.Generate(p_players_per_match, p_heats_per_match, p_winners_per_level);
			}
			Refresh();
		}

		public void Refresh()
		{
			string text = "";
			int num = 0;
			List<Text> list = levelFields;
			for (int i = 0; i < list.Count; i++)
			{
				list[i].text = "";
			}
			list = resultsFields;
			for (int j = 0; j < list.Count; j++)
			{
				list[j].text = "";
			}
			list = levelFields;
			num = Mathf.Min(data.levelCount, list.Count);
			for (int k = 0; k < num; k++)
			{
				Text text2 = list[k];
				text = "ROUND " + (k + 1) + "\n";
				List<TMatch> matchesByLevel = data.GetMatchesByLevel(k);
				List<TPlayer> winnersByLevel = data.GetWinnersByLevel(k - 1);
				int winnersPerMatch = data.GetWinnersPerMatch(k - 1);
				int maxWinners = data.GetMaxWinners(k - 1, winnersPerMatch);
				while (winnersByLevel.Count > maxWinners)
				{
					if (winnersByLevel.Count > 0)
					{
						winnersByLevel.RemoveAt(winnersByLevel.Count - 1);
					}
				}
				text = text + winnersByLevel.Count + " PLAYERS / " + matchesByLevel.Count + " MATCHES\n";
				text += "\n";
				for (int l = 0; l < matchesByLevel.Count; l++)
				{
					TMatch tMatch = matchesByLevel[l];
					text = text + " " + tMatch.name + "\n";
					text = text + " " + tMatch.players.Count + " PLAYERS / " + tMatch.heatCount + " HEATS / " + tMatch.maxWinners + " WPP\n";
					text += " -----------------------\n";
					for (int m = 0; m < tMatch.players.Count; m++)
					{
						TPlayer tPlayer = winnersByLevel[tMatch.players[m]];
						text = text + " " + tPlayer.name + " [" + tPlayer.skill.ToString("0.0") + "]";
						int p_count = 0;
						if (data.GetResultScoreSumByPlayer(k, tPlayer, out p_count))
						{
							float p_seconds = (float)p_count / 1000f;
							text = text + " | " + Format.SecondsToTime(p_seconds, 2, p_use_ms: true);
						}
						text += "\n";
					}
					text += "\n";
				}
				text2.text = text;
			}
			list = resultsFields;
			num = Mathf.Min(data.levelCount, list.Count);
			for (int n = 0; n < num; n++)
			{
				list[n].text = "";
			}
		}
	}
}
