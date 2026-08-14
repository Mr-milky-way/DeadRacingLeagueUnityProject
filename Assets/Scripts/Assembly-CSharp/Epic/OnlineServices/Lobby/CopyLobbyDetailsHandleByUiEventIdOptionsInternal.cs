using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyLobbyDetailsHandleByUiEventIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private ulong m_UiEventId;

		public ulong UiEventId
		{
			set
			{
				m_UiEventId = value;
			}
		}

		public void Set(CopyLobbyDetailsHandleByUiEventIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				UiEventId = other.UiEventId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyLobbyDetailsHandleByUiEventIdOptions);
		}

		public void Dispose()
		{
		}
	}
}
