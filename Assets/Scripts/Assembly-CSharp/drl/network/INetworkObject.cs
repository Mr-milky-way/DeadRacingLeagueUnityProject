namespace drl.network
{
	public interface INetworkObject
	{
		int ID { get; }

		NetworkActor Actor { get; set; }
	}
}
