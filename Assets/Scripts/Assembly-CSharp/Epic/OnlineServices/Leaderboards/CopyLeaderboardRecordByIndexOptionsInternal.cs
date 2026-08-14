using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyLeaderboardRecordByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_LeaderboardRecordIndex;

		public uint LeaderboardRecordIndex
		{
			set
			{
				m_LeaderboardRecordIndex = value;
			}
		}

		public void Set(CopyLeaderboardRecordByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				LeaderboardRecordIndex = other.LeaderboardRecordIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyLeaderboardRecordByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}
