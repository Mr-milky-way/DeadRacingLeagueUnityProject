using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyAchievementDefinitionV2ByIndexOptionsInternal : ISettable, IDisposable
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

		public void Set(CopyAchievementDefinitionV2ByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				AchievementIndex = other.AchievementIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyAchievementDefinitionV2ByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}
