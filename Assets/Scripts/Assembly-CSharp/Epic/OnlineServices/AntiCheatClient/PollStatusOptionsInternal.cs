using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatClient
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PollStatusOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_OutMessageLength;

		public uint OutMessageLength
		{
			set
			{
				m_OutMessageLength = value;
			}
		}

		public void Set(PollStatusOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				OutMessageLength = other.OutMessageLength;
			}
		}

		public void Set(object other)
		{
			Set(other as PollStatusOptions);
		}

		public void Dispose()
		{
		}
	}
}
