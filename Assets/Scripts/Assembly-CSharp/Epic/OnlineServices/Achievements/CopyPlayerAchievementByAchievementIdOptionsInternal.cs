using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyPlayerAchievementByAchievementIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_TargetUserId;

		private IntPtr m_AchievementId;

		private IntPtr m_LocalUserId;

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public string AchievementId
		{
			set
			{
				Helper.TryMarshalSet(ref m_AchievementId, value);
			}
		}

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public void Set(CopyPlayerAchievementByAchievementIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				TargetUserId = other.TargetUserId;
				AchievementId = other.AchievementId;
				LocalUserId = other.LocalUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyPlayerAchievementByAchievementIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_TargetUserId);
			Helper.TryMarshalDispose(ref m_AchievementId);
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}
