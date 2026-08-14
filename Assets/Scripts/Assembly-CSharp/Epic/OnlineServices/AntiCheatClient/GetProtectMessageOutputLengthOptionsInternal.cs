using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatClient
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetProtectMessageOutputLengthOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_DataLengthBytes;

		public uint DataLengthBytes
		{
			set
			{
				m_DataLengthBytes = value;
			}
		}

		public void Set(GetProtectMessageOutputLengthOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				DataLengthBytes = other.DataLengthBytes;
			}
		}

		public void Set(object other)
		{
			Set(other as GetProtectMessageOutputLengthOptions);
		}

		public void Dispose()
		{
		}
	}
}
