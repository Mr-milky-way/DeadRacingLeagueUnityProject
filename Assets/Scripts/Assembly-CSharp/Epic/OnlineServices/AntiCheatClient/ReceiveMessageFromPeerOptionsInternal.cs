using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatClient
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ReceiveMessageFromPeerOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_PeerHandle;

		private uint m_DataLengthBytes;

		private IntPtr m_Data;

		public IntPtr PeerHandle
		{
			set
			{
				m_PeerHandle = value;
			}
		}

		public byte[] Data
		{
			set
			{
				Helper.TryMarshalSet(ref m_Data, value, out m_DataLengthBytes);
			}
		}

		public void Set(ReceiveMessageFromPeerOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				PeerHandle = other.PeerHandle;
				Data = other.Data;
			}
		}

		public void Set(object other)
		{
			Set(other as ReceiveMessageFromPeerOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_PeerHandle);
			Helper.TryMarshalDispose(ref m_Data);
		}
	}
}
