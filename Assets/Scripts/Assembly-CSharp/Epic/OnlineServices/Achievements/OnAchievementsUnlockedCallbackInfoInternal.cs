using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct OnAchievementsUnlockedCallbackInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_UserId;

		private uint m_AchievementsCount;

		private IntPtr m_AchievementIds;

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

		public string[] AchievementIds
		{
			get
			{
				Helper.TryMarshalGet<string>(m_AchievementIds, out var target, m_AchievementsCount, isElementAllocated: true);
				return target;
			}
		}
	}
}
