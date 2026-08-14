using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatClient
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UnregisterPeerOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_PeerHandle;

		public IntPtr PeerHandle
		{
			set
			{
				m_PeerHandle = value;
			}
		}

		public void Set(UnregisterPeerOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				PeerHandle = other.PeerHandle;
			}
		}

		public void Set(object other)
		{
			Set(other as UnregisterPeerOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_PeerHandle);
		}
	}
}
