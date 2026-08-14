using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyLeaderboardUserScoreByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_LeaderboardUserScoreIndex;

		private IntPtr m_StatName;

		public uint LeaderboardUserScoreIndex
		{
			set
			{
				m_LeaderboardUserScoreIndex = value;
			}
		}

		public string StatName
		{
			set
			{
				Helper.TryMarshalSet(ref m_StatName, value);
			}
		}

		public void Set(CopyLeaderboardUserScoreByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LeaderboardUserScoreIndex = other.LeaderboardUserScoreIndex;
				StatName = other.StatName;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyLeaderboardUserScoreByIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_StatName);
		}
	}
}
