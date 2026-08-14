using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyUnlockedAchievementByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_UserId;

		private uint m_AchievementIndex;

		public ProductUserId UserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_UserId, value);
			}
		}

		public uint AchievementIndex
		{
			set
			{
				m_AchievementIndex = value;
			}
		}

		public void Set(CopyUnlockedAchievementByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				UserId = other.UserId;
				AchievementIndex = other.AchievementIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyUnlockedAchievementByIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserId);
		}
	}
}
