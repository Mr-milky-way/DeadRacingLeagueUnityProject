namespace Epic.OnlineServices.Sessions
{
	public class RegisterPlayersOptions
	{
		public string SessionName { get; set; }

		public ProductUserId[] PlayersToRegister { get; set; }
	}
}
