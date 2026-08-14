using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UnlockedAchievementInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_AchievementId;

		private long m_UnlockTime;

		public string AchievementId
		{
			get
			{
				Helper.TryMarshalGet(m_AchievementId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AchievementId, value);
			}
		}

		public DateTimeOffset? UnlockTime
		{
			get
			{
				Helper.TryMarshalGet(m_UnlockTime, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UnlockTime, value);
			}
		}

		public void Set(UnlockedAchievement other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				AchievementId = other.AchievementId;
				UnlockTime = other.UnlockTime;
			}
		}

		public void Set(object other)
		{
			Set(other as UnlockedAchievement);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AchievementId);
		}
	}
}
