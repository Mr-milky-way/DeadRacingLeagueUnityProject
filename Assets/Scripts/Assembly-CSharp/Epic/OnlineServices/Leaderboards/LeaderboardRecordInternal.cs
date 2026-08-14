using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LeaderboardRecordInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_UserId;

		private uint m_Rank;

		private int m_Score;

		private IntPtr m_UserDisplayName;

		public ProductUserId UserId
		{
			get
			{
				Helper.TryMarshalGet(m_UserId, out ProductUserId target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UserId, value);
			}
		}

		public uint Rank
		{
			get
			{
				return m_Rank;
			}
			set
			{
				m_Rank = value;
			}
		}

		public int Score
		{
			get
			{
				return m_Score;
			}
			set
			{
				m_Score = value;
			}
		}

		public string UserDisplayName
		{
			get
			{
				Helper.TryMarshalGet(m_UserDisplayName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UserDisplayName, value);
			}
		}

		public void Set(LeaderboardRecord other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				UserId = other.UserId;
				Rank = other.Rank;
				Score = other.Score;
				UserDisplayName = other.UserDisplayName;
			}
		}

		public void Set(object other)
		{
			Set(other as LeaderboardRecord);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserId);
			Helper.TryMarshalDispose(ref m_UserDisplayName);
		}
	}
}
