using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyLeaderboardDefinitionByLeaderboardIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LeaderboardId;

		public string LeaderboardId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LeaderboardId, value);
			}
		}

		public void Set(CopyLeaderboardDefinitionByLeaderboardIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LeaderboardId = other.LeaderboardId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyLeaderboardDefinitionByLeaderboardIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LeaderboardId);
		}
	}
}
