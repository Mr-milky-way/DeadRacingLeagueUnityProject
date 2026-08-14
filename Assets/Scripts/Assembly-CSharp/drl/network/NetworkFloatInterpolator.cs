using System.Collections.Generic;
using thelab.core;

namespace drl.network
{
	public class NetworkFloatInterpolator
	{
		private float networkValue;

		private Queue<float> networkValuesBuffer = new Queue<float>();

		private FloatInterpolator interpolator;

		public NetworkFloatInterpolator()
		{
			interpolator = new FloatInterpolator(InterpolationType.Predictive);
			interpolator.estimate.SetSampling(25, 0.5f);
			interpolator.estimate.maxDeviation = 2f;
			interpolator.estimate.delay = PhotonNetwork.GetPing();
		}

		public float UpdateValue(float currentValue)
		{
			currentValue = interpolator.Evaluate(networkValue, (float)PhotonNetwork.time);
			return currentValue;
		}

		public void OnPhotonSerializeView(float currentNetworkValue, PhotonStream stream, PhotonMessageInfo info)
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

		private void SerializeData(float currentNetworkValue, PhotonStream stream, PhotonMessageInfo info)
		{
			stream.SendNext(currentNetworkValue);
			networkValue = currentNetworkValue;
		}

		private void DeserializeData(PhotonStream stream, PhotonMessageInfo info)
		{
			float num = (float)stream.ReceiveNext();
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
