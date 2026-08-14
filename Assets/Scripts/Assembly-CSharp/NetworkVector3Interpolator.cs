using System.Collections.Generic;
using UnityEngine;
using thelab.core;

public class NetworkVector3Interpolator
{
	public struct NetworkValue
	{
		public Vector3 Position;

		public float Time;

		public float Delay;
	}

	private Queue<NetworkValue> networkValues;

	public NetworkValue currentValue;

	private Vector3Interpolator interpolator;

	public float Deviation
	{
		get
		{
			return interpolator.estimate.maxDeviation;
		}
		set
		{
			interpolator.estimate.maxDeviation = value;
		}
	}

	public NetworkVector3Interpolator()
	{
		interpolator = new Vector3Interpolator(InterpolationType.Predictive);
		interpolator.estimate.SetSampling(25, 0.5f);
		interpolator.estimate.maxDeviation = 2f;
		interpolator.estimate.delay = PhotonNetwork.GetPing();
		networkValues = new Queue<NetworkValue>();
	}

	public void Clear()
	{
		interpolator.Clear();
	}

	public Vector3 UpdateValue(Vector3 currentPosition)
	{
		NetworkValue networkValue = currentValue;
		if (networkValues.Count > 0)
		{
			networkValue = networkValues.Dequeue();
		}
		interpolator.estimate.delay = networkValue.Delay;
		return interpolator.Evaluate(networkValue.Position, networkValue.Time);
	}

	public void OnPhotonSerializeView(Vector3 currentNetworkValue, PhotonStream stream, PhotonMessageInfo info)
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

	private void SerializeData(Vector3 currentNetworkValue, PhotonStream stream, PhotonMessageInfo info)
	{
		stream.SendNext(currentNetworkValue);
	}

	private void DeserializeData(PhotonStream stream, PhotonMessageInfo info)
	{
		Vector3 position = (Vector3)stream.ReceiveNext();
		currentValue.Position = position;
		currentValue.Time = (float)info.timestamp;
		currentValue.Delay = Mathf.Abs((float)(PhotonNetwork.time - info.timestamp)) * 1000f;
		networkValues.Enqueue(currentValue);
	}
}
