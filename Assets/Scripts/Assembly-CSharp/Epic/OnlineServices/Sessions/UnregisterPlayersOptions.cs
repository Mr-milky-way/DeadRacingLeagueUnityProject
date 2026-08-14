namespace Epic.OnlineServices.Sessions
{
	public class UnregisterPlayersOptions
	{
		public string SessionName { get; set; }

		public ProductUserId[] PlayersToUnregister { get; set; }
	}
}
