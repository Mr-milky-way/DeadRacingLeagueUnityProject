using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyDetailsCopyAttributeByKeyOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_AttrKey;

		public string AttrKey
		{
			set
			{
				Helper.TryMarshalSet(ref m_AttrKey, value);
			}
		}

		public void Set(LobbyDetailsCopyAttributeByKeyOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				AttrKey = other.AttrKey;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyDetailsCopyAttributeByKeyOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AttrKey);
		}
	}
}
