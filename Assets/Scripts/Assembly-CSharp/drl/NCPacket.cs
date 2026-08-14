using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl
{
	[Serializable]
	public class NCPacket
	{
		public List<byte> channels;

		public uint[] values;

		public NCPacket(int p_max)
		{
			channels = new List<byte>();
			values = new uint[Mathf.Clamp(p_max, 0, 255)];
		}

		public void Clear()
		{
			channels.Clear();
		}

		public void Set(NCPacket p_packet)
		{
			channels.Clear();
			channels.AddRange(p_packet.channels);
			if (p_packet.values.Length != values.Length)
			{
				Array.Resize(ref values, p_packet.values.Length);
			}
			for (int i = 0; i < channels.Count; i++)
			{
				byte b = channels[i];
				values[b] = p_packet.values[b];
			}
		}

		public void Set(byte p_channel, uint p_value)
		{
			if (!channels.Contains(p_channel))
			{
				channels.Add(p_channel);
			}
			values[p_channel] = p_value;
		}

		public uint Get(byte p_channel, uint p_default = 0u)
		{
			if (!channels.Contains(p_channel))
			{
				return p_default;
			}
			return values[p_channel];
		}

		public uint[] Get()
		{
			uint[] array = new uint[channels.Count];
			for (int i = 0; i < channels.Count; i++)
			{
				array[i] = Get(channels[i]);
			}
			return array;
		}
	}
}
