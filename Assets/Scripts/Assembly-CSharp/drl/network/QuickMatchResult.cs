namespace drl.network
{
	public class QuickMatchResult
	{
		public QuickMatchState State;

		public NetworkRoom JoinedRoom;

		public bool IsNewRoom
		{
			get
			{
				if (JoinedRoom != null)
				{
					return JoinedRoom.IsMaster;
				}
				return false;
			}
		}
	}
}
