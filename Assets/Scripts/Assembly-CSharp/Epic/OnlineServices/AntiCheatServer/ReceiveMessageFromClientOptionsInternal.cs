using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatServer
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ReceiveMessageFromClientOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_ClientHandle;

		private uint m_DataLengthBytes;

		private IntPtr m_Data;

		public IntPtr ClientHandle
		{
			set
			{
				m_ClientHandle = value;
			}
		}

		public byte[] Data
		{
			set
			{
				Helper.TryMarshalSet(ref m_Data, value, out m_DataLengthBytes);
			}
		}

		public void Set(ReceiveMessageFromClientOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				ClientHandle = other.ClientHandle;
				Data = other.Data;
			}
		}

		public void Set(object other)
		{
			Set(other as ReceiveMessageFromClientOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ClientHandle);
			Helper.TryMarshalDispose(ref m_Data);
		}
	}
}
