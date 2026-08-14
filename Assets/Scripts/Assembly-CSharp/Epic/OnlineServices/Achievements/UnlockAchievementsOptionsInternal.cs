using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UnlockAchievementsOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_UserId;

		private IntPtr m_AchievementIds;

		private uint m_AchievementsCount;

		public ProductUserId UserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_UserId, value);
			}
		}

		public string[] AchievementIds
		{
			set
			{
				Helper.TryMarshalSet(ref m_AchievementIds, value, out m_AchievementsCount, true);
			}
		}

		public void Set(UnlockAchievementsOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				UserId = other.UserId;
				AchievementIds = other.AchievementIds;
			}
		}

		public void Set(object other)
		{
			Set(other as UnlockAchievementsOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserId);
			Helper.TryMarshalDispose(ref m_AchievementIds);
		}
	}
}
