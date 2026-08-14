using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatClient
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UnprotectMessageOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_DataLengthBytes;

		private IntPtr m_Data;

		private uint m_OutBufferSizeBytes;

		public byte[] Data
		{
			set
			{
				Helper.TryMarshalSet(ref m_Data, value, out m_DataLengthBytes);
			}
		}

		public uint OutBufferSizeBytes
		{
			set
			{
				m_OutBufferSizeBytes = value;
			}
		}

		public void Set(UnprotectMessageOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Data = other.Data;
				OutBufferSizeBytes = other.OutBufferSizeBytes;
			}
		}

		public void Set(object other)
		{
			Set(other as UnprotectMessageOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Data);
		}
	}
}
