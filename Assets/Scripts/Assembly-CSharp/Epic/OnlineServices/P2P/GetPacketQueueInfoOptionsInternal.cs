using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetPacketQueueInfoOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(GetPacketQueueInfoOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
			}
		}

		public void Set(object other)
		{
			Set(other as GetPacketQueueInfoOptions);
		}

		public void Dispose()
		{
		}
	}
}
