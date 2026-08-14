namespace drl.network
{
	public class NetworkRacerLocal : NetworkRacer
	{
		public static NetworkRacerLocal Create(NetworkActor localActor, INetworkObservable observedObject, NetworkRoom room)
		{
			NetworkRacerLocal networkRacerLocal = observedObject.gameObject.AddComponent<NetworkRacerLocal>();
			localActor.ViewId = PhotonNetwork.AllocateViewID();
			networkRacerLocal.SetRacer(localActor, room);
			observedObject.NetworkObject = networkRacerLocal;
			return networkRacerLocal;
		}
	}
}
