namespace drl.backend
{
	public class DRLLeaderboardResult
	{
		public bool success;

		public DRLServicePageData pagging;

		public DRLLeaderboardData[] leaderboard;

		public DRLLeaderboardResult()
		{
			success = true;
			pagging = new DRLServicePageData();
			leaderboard = new DRLLeaderboardData[0];
		}
	}
}
