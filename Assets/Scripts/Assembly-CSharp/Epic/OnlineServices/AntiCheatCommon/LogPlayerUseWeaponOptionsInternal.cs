using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogPlayerUseWeaponOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_UseWeaponData;

		public LogPlayerUseWeaponData UseWeaponData
		{
			set
			{
				Helper.TryMarshalSet<LogPlayerUseWeaponDataInternal, LogPlayerUseWeaponData>(ref m_UseWeaponData, value);
			}
		}

		public void Set(LogPlayerUseWeaponOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				UseWeaponData = other.UseWeaponData;
			}
		}

		public void Set(object other)
		{
			Set(other as LogPlayerUseWeaponOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UseWeaponData);
		}
	}
}
