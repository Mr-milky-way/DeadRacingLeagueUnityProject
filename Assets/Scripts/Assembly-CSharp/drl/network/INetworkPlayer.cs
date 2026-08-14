namespace drl.network
{
	public interface INetworkPlayer
	{
		int ID { get; }

		string PlayerId { get; }

		bool IsSpectator { get; }

		bool IsMaster { get; }
	}
}
