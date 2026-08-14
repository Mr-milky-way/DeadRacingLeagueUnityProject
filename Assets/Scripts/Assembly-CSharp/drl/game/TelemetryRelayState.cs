namespace drl.game
{
	public enum TelemetryRelayState
	{
		Idle = 0,
		ConnectStart = 1,
		ConnectWait = 2,
		ConnectSuccess = 3,
		ConnectError = 4,
		ConnectRetry = 5,
		Buffer = 6,
		WritePoll = 7,
		FlushPoll = 8
	}
}
