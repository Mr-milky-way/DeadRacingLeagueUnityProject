using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyAchievementDefinitionByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_AchievementIndex;

		public uint AchievementIndex
		{
			set
			{
				m_AchievementIndex = value;
			}
		}

		public void Set(CopyAchievementDefinitionByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				AchievementIndex = other.AchievementIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyAchievementDefinitionByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}
