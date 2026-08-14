using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Mods
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UninstallModOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_Mod;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public ModIdentifier Mod
		{
			set
			{
				Helper.TryMarshalSet<ModIdentifierInternal, ModIdentifier>(ref m_Mod, value);
			}
		}

		public void Set(UninstallModOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				Mod = other.Mod;
			}
		}

		public void Set(object other)
		{
			Set(other as UninstallModOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_Mod);
		}
	}
}
