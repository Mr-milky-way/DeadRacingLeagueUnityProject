using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyLeaderboardDefinitionByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_LeaderboardIndex;

		public uint LeaderboardIndex
		{
			set
			{
				m_LeaderboardIndex = value;
			}
		}

		public void Set(CopyLeaderboardDefinitionByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LeaderboardIndex = other.LeaderboardIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyLeaderboardDefinitionByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}
