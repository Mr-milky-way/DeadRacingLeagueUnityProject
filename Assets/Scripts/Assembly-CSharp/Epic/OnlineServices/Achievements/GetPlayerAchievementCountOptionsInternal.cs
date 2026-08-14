using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetPlayerAchievementCountOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_UserId;

		public ProductUserId UserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_UserId, value);
			}
		}

		public void Set(GetPlayerAchievementCountOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				UserId = other.UserId;
			}
		}

		public void Set(object other)
		{
			Set(other as GetPlayerAchievementCountOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserId);
		}
	}
}
