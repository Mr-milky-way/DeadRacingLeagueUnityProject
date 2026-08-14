using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetLeaderboardRecordCountOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(GetLeaderboardRecordCountOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
			}
		}

		public void Set(object other)
		{
			Set(other as GetLeaderboardRecordCountOptions);
		}

		public void Dispose()
		{
		}
	}
}
