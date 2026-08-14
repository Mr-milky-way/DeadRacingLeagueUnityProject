using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	public class DRLTournamentStandingsItem
	{
		public string playerId = "";

		public int rank;

		public string username = "";

		public int totalWins;

		public List<Tuple<int, float, int>> results = new List<Tuple<int, float, int>>();

		public bool isWinner;

		public bool isWinnerSecond;

		public Color color;

		public int crashes;

		public int playerBestIndex = -1;

		public int overallBestIndex = -1;
	}
}
