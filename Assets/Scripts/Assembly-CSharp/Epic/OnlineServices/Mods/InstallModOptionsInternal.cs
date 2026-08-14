using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Mods
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct InstallModOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_Mod;

		private int m_RemoveAfterExit;

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

		public bool RemoveAfterExit
		{
			set
			{
				Helper.TryMarshalSet(ref m_RemoveAfterExit, value);
			}
		}

		public void Set(InstallModOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				Mod = other.Mod;
				RemoveAfterExit = other.RemoveAfterExit;
			}
		}

		public void Set(object other)
		{
			Set(other as InstallModOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_Mod);
		}
	}
}
