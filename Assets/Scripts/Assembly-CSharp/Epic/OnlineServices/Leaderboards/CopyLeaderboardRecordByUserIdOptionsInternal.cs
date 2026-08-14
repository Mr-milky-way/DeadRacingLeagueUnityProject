using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyLeaderboardRecordByUserIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_UserId;

		public ProductUserId UserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_UserId, value);
			}
		}

		public void Set(CopyLeaderboardRecordByUserIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				UserId = other.UserId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyLeaderboardRecordByUserIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserId);
		}
	}
}
