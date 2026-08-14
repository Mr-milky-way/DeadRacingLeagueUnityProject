using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopySessionHandleByUiEventIdOptionsInternal : ISettable, IDisposable
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

		public void Set(CopySessionHandleByUiEventIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				UiEventId = other.UiEventId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopySessionHandleByUiEventIdOptions);
		}

		public void Dispose()
		{
		}
	}
}
