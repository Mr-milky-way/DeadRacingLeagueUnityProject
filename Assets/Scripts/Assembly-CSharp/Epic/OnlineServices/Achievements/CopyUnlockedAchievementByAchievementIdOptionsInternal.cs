using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyUnlockedAchievementByAchievementIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_UserId;

		private IntPtr m_AchievementId;

		public ProductUserId UserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_UserId, value);
			}
		}

		public string AchievementId
		{
			set
			{
				Helper.TryMarshalSet(ref m_AchievementId, value);
			}
		}

		public void Set(CopyUnlockedAchievementByAchievementIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				UserId = other.UserId;
				AchievementId = other.AchievementId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyUnlockedAchievementByAchievementIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserId);
			Helper.TryMarshalDispose(ref m_AchievementId);
		}
	}
}
