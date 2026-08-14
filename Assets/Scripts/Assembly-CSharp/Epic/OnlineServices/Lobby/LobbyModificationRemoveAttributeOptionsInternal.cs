using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyModificationRemoveAttributeOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Key;

		public string Key
		{
			set
			{
				Helper.TryMarshalSet(ref m_Key, value);
			}
		}

		public void Set(LobbyModificationRemoveAttributeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Key = other.Key;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyModificationRemoveAttributeOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Key);
		}
	}
}
