using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct OnAchievementsUnlockedCallbackV2InfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_UserId;

		private IntPtr m_AchievementId;

		private long m_UnlockTime;

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public ProductUserId UserId
		{
			get
			{
				Helper.TryMarshalGet(m_UserId, out ProductUserId target);
				return target;
			}
		}

		public string AchievementId
		{
			get
			{
				Helper.TryMarshalGet(m_AchievementId, out string target);
				return target;
			}
		}

		public DateTimeOffset? UnlockTime
		{
			get
			{
				Helper.TryMarshalGet(m_UnlockTime, out var target);
				return target;
			}
		}
	}
}
