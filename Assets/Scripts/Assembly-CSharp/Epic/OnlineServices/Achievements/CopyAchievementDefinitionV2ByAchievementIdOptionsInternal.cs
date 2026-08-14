using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyAchievementDefinitionV2ByAchievementIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_AchievementId;

		public string AchievementId
		{
			set
			{
				Helper.TryMarshalSet(ref m_AchievementId, value);
			}
		}

		public void Set(CopyAchievementDefinitionV2ByAchievementIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				AchievementId = other.AchievementId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyAchievementDefinitionV2ByAchievementIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AchievementId);
		}
	}
}
