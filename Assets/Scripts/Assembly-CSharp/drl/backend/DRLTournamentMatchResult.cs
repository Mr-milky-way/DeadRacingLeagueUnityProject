using thelab.core;

namespace drl.backend
{
	public class DRLTournamentMatchResult : SerializedData
	{
		public const string StatusPending = "waiting";

		public const string StatusSuccess = "success";

		public DRLTournamentMatchData[] matches;

		public string status => Get("status", "waiting");

		public DRLTournamentMatchResult()
		{
			matches = new DRLTournamentMatchData[0];
		}
	}
}
