using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyPlayerAchievementByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_TargetUserId;

		private uint m_AchievementIndex;

		private IntPtr m_LocalUserId;

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public uint AchievementIndex
		{
			set
			{
				m_AchievementIndex = value;
			}
		}

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public void Set(CopyPlayerAchievementByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				TargetUserId = other.TargetUserId;
				AchievementIndex = other.AchievementIndex;
				LocalUserId = other.LocalUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyPlayerAchievementByIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_TargetUserId);
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}
