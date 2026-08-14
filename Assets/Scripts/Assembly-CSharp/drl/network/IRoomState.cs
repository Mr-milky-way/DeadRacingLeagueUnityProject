namespace drl.network
{
	public interface IRoomState
	{
		void OnEnter(NetworkRoom room);

		void OnExit(NetworkRoom room);

		void OnUpdate(NetworkRoom room);
	}
}
