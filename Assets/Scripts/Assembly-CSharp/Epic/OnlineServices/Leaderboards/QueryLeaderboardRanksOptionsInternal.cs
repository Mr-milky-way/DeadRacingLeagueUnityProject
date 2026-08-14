using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryLeaderboardRanksOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LeaderboardId;

		private IntPtr m_LocalUserId;

		public string LeaderboardId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LeaderboardId, value);
			}
		}

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public void Set(QueryLeaderboardRanksOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				LeaderboardId = other.LeaderboardId;
				LocalUserId = other.LocalUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryLeaderboardRanksOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LeaderboardId);
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}
