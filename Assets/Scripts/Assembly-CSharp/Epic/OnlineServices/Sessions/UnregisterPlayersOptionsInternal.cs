using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UnregisterPlayersOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SessionName;

		private IntPtr m_PlayersToUnregister;

		private uint m_PlayersToUnregisterCount;

		public string SessionName
		{
			set
			{
				Helper.TryMarshalSet(ref m_SessionName, value);
			}
		}

		public ProductUserId[] PlayersToUnregister
		{
			set
			{
				Helper.TryMarshalSet(ref m_PlayersToUnregister, value, out m_PlayersToUnregisterCount);
			}
		}

		public void Set(UnregisterPlayersOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SessionName = other.SessionName;
				PlayersToUnregister = other.PlayersToUnregister;
			}
		}

		public void Set(object other)
		{
			Set(other as UnregisterPlayersOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SessionName);
			Helper.TryMarshalDispose(ref m_PlayersToUnregister);
		}
	}
}
