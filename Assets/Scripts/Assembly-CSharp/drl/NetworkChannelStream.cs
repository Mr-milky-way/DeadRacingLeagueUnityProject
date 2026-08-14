using System;

namespace drl
{
	public class NetworkChannelStream : NetworkByteStream<NCPacket>
	{
		public Action<NCPacket> OnSync;

		private NCPacket m_back_buffer;

		private NCPacket m_front_buffer;

		private NCPacket m_buffer;

		public NCPacket state { get; private set; }

		public NetworkChannelStream(byte p_channel_count, NetworkByteStreamMode p_mode, bool p_use_udp = true)
			: base(p_mode, p_use_udp)
		{
			m_back_buffer = new NCPacket(p_channel_count);
			m_front_buffer = new NCPacket(p_channel_count);
			state = new NCPacket(100);
			m_buffer = m_back_buffer;
		}

		public void BeginWrite()
		{
			m_buffer.Clear();
		}

		public void Set(byte p_channel, uint p_value)
		{
			m_buffer.Set(p_channel, p_value);
		}

		public void EndWrite()
		{
			NCPacket buffer = m_buffer;
			m_buffer = ((m_buffer == m_back_buffer) ? m_front_buffer : m_back_buffer);
			state.Set(buffer);
			Send(buffer);
		}

		protected override void OnDecode(NCPacket p_data)
		{
			NCPacket nCPacket = new NCPacket(p_data.values.Length);
			nCPacket.Set(p_data);
			state = nCPacket;
			if (OnSync != null)
			{
				OnSync(nCPacket);
			}
		}
	}
}
