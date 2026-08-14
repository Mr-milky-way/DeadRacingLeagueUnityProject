using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ActiveSessionGetRegisteredPlayerByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_PlayerIndex;

		public uint PlayerIndex
		{
			set
			{
				m_PlayerIndex = value;
			}
		}

		public void Set(ActiveSessionGetRegisteredPlayerByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				PlayerIndex = other.PlayerIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as ActiveSessionGetRegisteredPlayerByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}
