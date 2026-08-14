namespace drl.game
{
	public enum UILobbyFeedbackType
	{
		None = -1,
		NoRoom = 0,
		Connecting = 1,
		NoMatches = 2,
		SearchingMatches = 3,
		CreatingRoom = 4,
		CreatingServer = 5,
		StoppingServer = 6,
		WaitingForPlayers = 7,
		PlayersInQueue = 8,
		RaceLockedWithPlayers = 9,
		OperationFailure = 10,
		TimeOut = 11,
		ServerError = 12
	}
}
