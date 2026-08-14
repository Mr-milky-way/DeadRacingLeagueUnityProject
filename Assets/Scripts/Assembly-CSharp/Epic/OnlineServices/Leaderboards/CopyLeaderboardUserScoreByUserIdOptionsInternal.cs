using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyLeaderboardUserScoreByUserIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_UserId;

		private IntPtr m_StatName;

		public ProductUserId UserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_UserId, value);
			}
		}

		public string StatName
		{
			set
			{
				Helper.TryMarshalSet(ref m_StatName, value);
			}
		}

		public void Set(CopyLeaderboardUserScoreByUserIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				UserId = other.UserId;
				StatName = other.StatName;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyLeaderboardUserScoreByUserIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserId);
			Helper.TryMarshalDispose(ref m_StatName);
		}
	}
}
