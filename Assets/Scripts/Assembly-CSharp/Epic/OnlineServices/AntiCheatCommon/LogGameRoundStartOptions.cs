namespace Epic.OnlineServices.AntiCheatCommon
{
	public class LogGameRoundStartOptions
	{
		public string SessionIdentifier { get; set; }

		public string LevelName { get; set; }

		public string ModeName { get; set; }

		public uint RoundTimeSeconds { get; set; }
	}
}
