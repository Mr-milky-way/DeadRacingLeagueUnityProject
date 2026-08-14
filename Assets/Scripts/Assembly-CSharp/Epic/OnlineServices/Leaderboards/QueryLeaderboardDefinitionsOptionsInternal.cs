using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryLeaderboardDefinitionsOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private long m_StartTime;

		private long m_EndTime;

		private IntPtr m_LocalUserId;

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

		public void Set(QueryLeaderboardDefinitionsOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				StartTime = other.StartTime;
				EndTime = other.EndTime;
				LocalUserId = other.LocalUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryLeaderboardDefinitionsOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}
