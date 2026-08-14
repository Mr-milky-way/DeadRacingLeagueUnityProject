using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct RegisterPlayersOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SessionName;

		private IntPtr m_PlayersToRegister;

		private uint m_PlayersToRegisterCount;

		public string SessionName
		{
			set
			{
				Helper.TryMarshalSet(ref m_SessionName, value);
			}
		}

		public ProductUserId[] PlayersToRegister
		{
			set
			{
				Helper.TryMarshalSet(ref m_PlayersToRegister, value, out m_PlayersToRegisterCount);
			}
		}

		public void Set(RegisterPlayersOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SessionName = other.SessionName;
				PlayersToRegister = other.PlayersToRegister;
			}
		}

		public void Set(object other)
		{
			Set(other as RegisterPlayersOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SessionName);
			Helper.TryMarshalDispose(ref m_PlayersToRegister);
		}
	}
}
