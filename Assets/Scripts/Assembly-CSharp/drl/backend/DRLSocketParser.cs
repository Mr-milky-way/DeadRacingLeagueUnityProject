using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace drl.backend
{
	public static class DRLSocketParser
	{
		[Serializable]
		private class PacketJson
		{
			public string type = "unknown";

			public object data;
		}

		private static PacketJson Parse(string p_data)
		{
			PacketJson result = null;
			try
			{
				object[] array = JsonConvert.DeserializeObject<object[]>(p_data);
				if (array.Length <= 1)
				{
					return result;
				}
				return JsonConvert.DeserializeObject<PacketJson>(array[1].ToString());
			}
			catch (Exception ex)
			{
				Debug.LogWarning("DRLSocketParser: Couldn't parse data - " + ex.Message + " data " + p_data);
				return null;
			}
		}

		private static string Parse(DRLSocketPacket p_packet)
		{
			string arg = JsonConvert.SerializeObject(new PacketJson
			{
				type = p_packet.eventName,
				data = p_packet.message
			}, Formatting.None);
			return $"[\"event\",{arg}]";
		}

		public static string Encode(DRLSocketPacket packet)
		{
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append((int)packet.enginePacketType);
				if (!packet.enginePacketType.Equals(EnginePacketType.MESSAGE))
				{
					return stringBuilder.ToString();
				}
				stringBuilder.Append((int)packet.socketPacketType);
				if (packet.socketPacketType == SocketPacketType.BINARY_EVENT || packet.socketPacketType == SocketPacketType.BINARY_ACK)
				{
					stringBuilder.Append(packet.attachments);
					stringBuilder.Append('-');
				}
				if (!string.IsNullOrEmpty(packet.nsp) && !packet.nsp.Equals("/"))
				{
					stringBuilder.Append(packet.nsp);
					stringBuilder.Append(',');
				}
				if (packet.id > -1)
				{
					stringBuilder.Append(packet.id);
				}
				if (packet.eventName != null)
				{
					string value = Parse(packet);
					stringBuilder.Append(value);
				}
				return stringBuilder.ToString();
			}
			catch (Exception ex)
			{
				Debug.LogWarning("DRLSocketPacket> Error: " + ex.Message);
				return null;
			}
		}

		public static DRLSocketPacket Decode(string p_message)
		{
			if (string.IsNullOrEmpty(p_message))
			{
				Debug.LogWarning("DRLSocketPacket: Decode - message is empty!");
				return null;
			}
			try
			{
				DRLSocketPacket dRLSocketPacket = new DRLSocketPacket();
				int num = 0;
				int num2 = (int)(dRLSocketPacket.enginePacketType = (EnginePacketType)int.Parse(p_message.Substring(num, 1)));
				if (num2 != 4)
				{
					return dRLSocketPacket;
				}
				num++;
				if (num2 == 4)
				{
					int socketPacketType = int.Parse(p_message.Substring(num, 1));
					dRLSocketPacket.socketPacketType = (SocketPacketType)socketPacketType;
				}
				if (p_message.Length <= 2)
				{
					return dRLSocketPacket;
				}
				num++;
				if (num < p_message.Length - 1)
				{
					PacketJson packetJson = Parse(p_message.Substring(num));
					if (packetJson != null)
					{
						dRLSocketPacket.eventName = packetJson.type;
						dRLSocketPacket.message = packetJson.data.ToString();
					}
				}
				return dRLSocketPacket;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("DRLSocketPacket> Decode - error " + ex.Message);
				return null;
			}
		}
	}
}
