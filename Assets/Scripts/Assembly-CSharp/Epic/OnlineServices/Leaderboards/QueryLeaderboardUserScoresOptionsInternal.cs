using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryLeaderboardUserScoresOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_UserIds;

		private uint m_UserIdsCount;

		private IntPtr m_StatInfo;

		private uint m_StatInfoCount;

		private long m_StartTime;

		private long m_EndTime;

		private IntPtr m_LocalUserId;

		public ProductUserId[] UserIds
		{
			set
			{
				Helper.TryMarshalSet(ref m_UserIds, value, out m_UserIdsCount);
			}
		}

		public UserScoresQueryStatInfo[] StatInfo
		{
			set
			{
				Helper.TryMarshalSet<UserScoresQueryStatInfoInternal, UserScoresQueryStatInfo>(ref m_StatInfo, value, out m_StatInfoCount);
			}
		}

		public DateTimeOffset? StartTime
		{
			set
			{
				Helper.TryMarshalSet(ref m_StartTime, value);
			}
		}

		public DateTimeOffset? EndTime
		{
			set
			{
				Helper.TryMarshalSet(ref m_EndTime, value);
			}
		}

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public void Set(QueryLeaderboardUserScoresOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				UserIds = other.UserIds;
				StatInfo = other.StatInfo;
				StartTime = other.StartTime;
				EndTime = other.EndTime;
				LocalUserId = other.LocalUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryLeaderboardUserScoresOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserIds);
			Helper.TryMarshalDispose(ref m_StatInfo);
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}
