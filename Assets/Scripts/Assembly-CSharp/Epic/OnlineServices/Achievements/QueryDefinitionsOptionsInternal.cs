using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryDefinitionsOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_EpicUserId_DEPRECATED;

		private IntPtr m_HiddenAchievementIds_DEPRECATED;

		private uint m_HiddenAchievementsCount_DEPRECATED;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public EpicAccountId EpicUserId_DEPRECATED
		{
			set
			{
				Helper.TryMarshalSet(ref m_EpicUserId_DEPRECATED, value);
			}
		}

		public string[] HiddenAchievementIds_DEPRECATED
		{
			set
			{
				Helper.TryMarshalSet(ref m_HiddenAchievementIds_DEPRECATED, value, out m_HiddenAchievementsCount_DEPRECATED, true);
			}
		}

		public void Set(QueryDefinitionsOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 3;
				LocalUserId = other.LocalUserId;
				EpicUserId_DEPRECATED = other.EpicUserId_DEPRECATED;
				HiddenAchievementIds_DEPRECATED = other.HiddenAchievementIds_DEPRECATED;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryDefinitionsOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_EpicUserId_DEPRECATED);
			Helper.TryMarshalDispose(ref m_HiddenAchievementIds_DEPRECATED);
		}
	}
}
