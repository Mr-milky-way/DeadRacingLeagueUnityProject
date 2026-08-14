using System;

namespace drl.game
{
	[Serializable]
	public class TournamentNotificationState
	{
		public string guid;

		public TournamentState state;

		public bool soonToStartNotified;

		public TournamentNotificationState(string p_guid, TournamentState p_state)
		{
			guid = p_guid;
			state = p_state;
			soonToStartNotified = false;
		}
	}
}
