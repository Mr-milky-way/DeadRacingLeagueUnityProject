namespace drl.network
{
	public class MatchLockedState : IRoomState
	{
		public void OnEnter(NetworkRoom room)
		{
			room.Local.Reset();
		}

		public void OnExit(NetworkRoom room)
		{
		}

		public void OnUpdate(NetworkRoom room)
		{
		}
	}
}
