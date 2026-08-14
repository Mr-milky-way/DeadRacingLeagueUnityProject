using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatClient
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ReceiveMessageFromServerOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_DataLengthBytes;

		private IntPtr m_Data;

		public byte[] Data
		{
			set
			{
				Helper.TryMarshalSet(ref m_Data, value, out m_DataLengthBytes);
			}
		}

		public void Set(ReceiveMessageFromServerOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Data = other.Data;
			}
		}

		public void Set(object other)
		{
			Set(other as ReceiveMessageFromServerOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Data);
		}
	}
}
