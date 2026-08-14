using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetLeaderboardUserScoreCountOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_StatName;

		public string StatName
		{
			set
			{
				Helper.TryMarshalSet(ref m_StatName, value);
			}
		}

		public void Set(GetLeaderboardUserScoreCountOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				StatName = other.StatName;
			}
		}

		public void Set(object other)
		{
			Set(other as GetLeaderboardUserScoreCountOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_StatName);
		}
	}
}
