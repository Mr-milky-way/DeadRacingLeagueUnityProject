using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AddNotifyAchievementsUnlockedV2OptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(AddNotifyAchievementsUnlockedV2Options other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
			}
		}

		public void Set(object other)
		{
			Set(other as AddNotifyAchievementsUnlockedV2Options);
		}

		public void Dispose()
		{
		}
	}
}
