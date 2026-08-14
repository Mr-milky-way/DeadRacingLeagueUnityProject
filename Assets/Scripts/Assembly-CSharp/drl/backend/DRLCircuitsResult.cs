namespace drl.backend
{
	public class DRLCircuitsResult
	{
		public bool success;

		public DRLServicePageData pagging;

		public DRLCircuitLeaderboardData[] leaderboard;

		public DRLCircuitsResult()
		{
			success = true;
			pagging = new DRLServicePageData();
			leaderboard = new DRLCircuitLeaderboardData[0];
		}
	}
}
