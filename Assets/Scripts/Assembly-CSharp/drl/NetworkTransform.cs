using System.Threading;
using UnityEngine;

namespace drl
{
	public class NetworkTransform : MonoBehaviour
	{
		public bool useUDP;

		public NetworkByteStreamMode mode;

		public NetworkChannelStream ncs;

		public string ip;

		public int port = 30123;

		public Vector3 lp;

		public Vector3 lr;

		public Vector3 ls;

		private Thread m_receive_thread;

		private Thread m_send_thread;

		public bool active
		{
			get
			{
				if (ncs != null)
				{
					return ncs.active;
				}
				return false;
			}
		}

		private void Start()
		{
			if (ncs != null)
			{
				return;
			}
			ClearThreads();
			ncs = new NetworkChannelStream(16, mode, useUDP);
			ncs.OnSync = OnChannelSync;
			switch (mode)
			{
			case NetworkByteStreamMode.Receive:
				m_receive_thread = new Thread((ThreadStart)delegate
				{
					ncs.Start(port);
					ip = ncs.ip;
				});
				m_receive_thread.Start();
				break;
			case NetworkByteStreamMode.Send:
				m_send_thread = new Thread((ThreadStart)delegate
				{
					Thread.Sleep(100);
					ncs.Start(ip, port);
					while (true)
					{
						ncs.BeginWrite();
						ncs.EndWrite();
					}
				});
				m_send_thread.Start();
				break;
			}
		}

		protected void OnChannelSync(NCPacket p_packet)
		{
			lp = new Vector3(p_packet.Get(0), p_packet.Get(1), p_packet.Get(2));
			lr = new Vector3(p_packet.Get(3), p_packet.Get(4), p_packet.Get(5));
			ls = new Vector3(p_packet.Get(6, 1u), p_packet.Get(7, 1u), p_packet.Get(8, 1u));
		}

		protected void Update()
		{
			if (active)
			{
				switch (mode)
				{
				case NetworkByteStreamMode.Receive:
					base.transform.localPosition = lp;
					base.transform.localEulerAngles = lr;
					base.transform.localScale = ls;
					break;
				case NetworkByteStreamMode.Send:
					lp = base.transform.localPosition;
					lr = base.transform.localEulerAngles;
					ls = base.transform.localScale;
					break;
				}
			}
		}

		protected void OnDestroy()
		{
			ncs.Disconnect();
			ClearThreads();
		}

		protected void ClearThreads()
		{
			Thread receive_thread = m_receive_thread;
			if (receive_thread != null && receive_thread.IsAlive)
			{
				receive_thread.Abort();
			}
			m_receive_thread = null;
			receive_thread = m_send_thread;
			if (receive_thread != null && receive_thread.IsAlive)
			{
				receive_thread.Abort();
			}
			m_send_thread = null;
		}
	}
}
