namespace drl.network
{
	public enum QuickMatchState
	{
		FindingBestServer = 0,
		ConnectedBestServer = 1,
		CreatingRoom = 2,
		JoinedRoom = 3,
		Failed = 4,
		MatchmakingChanged = 5
	}
}
