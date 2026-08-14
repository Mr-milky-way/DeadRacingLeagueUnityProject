using thelab.core;

namespace drl.backend
{
	public class DRLTournamentLeaderboardParams : SerializedData
	{
		public string guid => Get("guid", string.Empty);

		public string match => Get("match", string.Empty);
	}
}
