using System.Collections.Generic;
using thelab.core;

namespace drl.network
{
	public class NetworkULongInterpolator
	{
		private long networkValue;

		private Queue<long> networkValuesBuffer = new Queue<long>();

		private LongInterpolator interpolator;

		public NetworkULongInterpolator()
		{
			interpolator = new LongInterpolator(InterpolationType.None);
			interpolator.estimate.SetSampling(25, 0.5f);
			interpolator.estimate.maxDeviation = 2f;
			interpolator.estimate.delay = PhotonNetwork.GetPing();
		}

		public long UpdateValue(long currentValue)
		{
			currentValue = interpolator.Evaluate(networkValue, (float)PhotonNetwork.time);
			return currentValue;
		}

		public void OnPhotonSerializeView(long currentNetworkValue, PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.isWriting)
			{
				SerializeData(currentNetworkValue, stream, info);
			}
			else
			{
				DeserializeData(stream, info);
			}
		}

		private void SerializeData(long currentNetworkValue, PhotonStream stream, PhotonMessageInfo info)
		{
			stream.SendNext(currentNetworkValue);
			networkValue = currentNetworkValue;
		}

		private void DeserializeData(PhotonStream stream, PhotonMessageInfo info)
		{
			long num = (long)stream.ReceiveNext();
			if (networkValuesBuffer.Count == 0)
			{
				networkValue = num;
			}
			networkValuesBuffer.Enqueue(networkValue);
			networkValue = num;
			while (networkValuesBuffer.Count > 1)
			{
				networkValuesBuffer.Dequeue();
			}
		}
	}
}
