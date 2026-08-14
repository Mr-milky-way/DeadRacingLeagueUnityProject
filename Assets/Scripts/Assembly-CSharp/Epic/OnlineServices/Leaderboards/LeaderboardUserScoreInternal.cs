using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LeaderboardUserScoreInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_UserId;

		private int m_Score;

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

		public void Set(LeaderboardUserScore other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				UserId = other.UserId;
				Score = other.Score;
			}
		}

		public void Set(object other)
		{
			Set(other as LeaderboardUserScore);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserId);
		}
	}
}
