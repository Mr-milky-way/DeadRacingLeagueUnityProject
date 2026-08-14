using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl
{
	public class NetworkEmulator : MonoBehaviour
	{
		public ulong serverId;

		public float serverElapsed;

		public float worldDeltaTime = 0.001f;

		[Range(0.01f, 1f)]
		public float timeScale = 1f;

		[Range(0f, 300f)]
		public float minRTT;

		[Range(0f, 300f)]
		public float maxRTT;

		public int dropped;

		public bool active;

		[HideInInspector]
		public List<float> pingBuffer;

		public List<Packet> outgoing;

		public List<Packet> incoming;

		public Func<object> OnPacketSend;

		public Action<Packet> OnPacketReceive;

		public List<float> bufferSmoothingKernel;

		public int bufferSmoothningLength = 10;

		public AnimationCurve bufferSmoothingCurve;

		public float ping
		{
			get
			{
				float num = 0f;
				float num2 = pingBuffer.Count;
				for (int i = 0; i < pingBuffer.Count; i++)
				{
					num += pingBuffer[i];
				}
				if (!(num2 <= 0f))
				{
					return num / num2;
				}
				return 0f;
			}
		}

		public void Clear()
		{
			outgoing.Clear();
			incoming.Clear();
			dropped = 0;
			serverElapsed = 0f;
			serverId = 0uL;
			pingBuffer.Clear();
		}

		public void Step()
		{
			if (!active)
			{
				return;
			}
			float num = worldDeltaTime * timeScale;
			if (bufferSmoothingKernel.Count != bufferSmoothningLength)
			{
				float num2 = Mathf.Max(1, bufferSmoothningLength);
				float num3 = 1f / num2;
				bufferSmoothingKernel = new List<float>();
				AnimationCurve animationCurve = bufferSmoothingCurve;
				if (animationCurve != null)
				{
					for (float num4 = 0f; num4 <= 1f; num4 += num3)
					{
						bufferSmoothingKernel.Add(animationCurve.Evaluate(num4));
					}
				}
			}
			object obj = null;
			if (OnPacketSend != null)
			{
				obj = OnPacketSend();
			}
			if (obj != null)
			{
				Emit(obj);
			}
			Listen();
			Packet packet = Pop();
			if (packet != null && OnPacketReceive != null)
			{
				OnPacketReceive(packet);
			}
			serverElapsed += num;
		}

		public void Emit(object p_data)
		{
			Packet packet = new Packet();
			packet.id = serverId++;
			packet.data = p_data;
			packet.rtt = UnityEngine.Random.Range(minRTT, maxRTT) / 1000f;
			packet.name = packet.id.ToString("000000 / " + (packet.rtt * 100f).ToString("0"));
			outgoing.Add(packet);
			pingBuffer.Add(packet.rtt * 1000f);
			if (pingBuffer.Count > 600)
			{
				pingBuffer.RemoveAt(0);
			}
		}

		public Packet Pop()
		{
			if (incoming.Count <= 0)
			{
				return null;
			}
			Packet result = incoming[0];
			incoming.RemoveAt(0);
			return result;
		}

		public void Listen()
		{
			float num = worldDeltaTime * timeScale;
			for (int i = 0; i < outgoing.Count; i++)
			{
				Packet packet = outgoing[i];
				packet.rtt -= num * timeScale;
				if (packet.rtt <= 0f)
				{
					outgoing.RemoveAt(i--);
					if (incoming.Count <= 0)
					{
						incoming.Add(packet);
					}
					else if (incoming[incoming.Count - 1].id > packet.id)
					{
						dropped++;
					}
					else
					{
						incoming.Add(packet);
					}
				}
			}
		}
	}
}
