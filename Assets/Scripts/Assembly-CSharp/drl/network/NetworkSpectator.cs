using UnityEngine;

namespace drl.network
{
	public class NetworkSpectator : MonoBehaviour, INetworkObject
	{
		public int ID
		{
			get
			{
				if (Actor != null)
				{
					return Actor.ID;
				}
				return -1;
			}
		}

		public NetworkActor Actor { get; set; }
	}
}
