using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AddNotifyJoinGameAcceptedOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(AddNotifyJoinGameAcceptedOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
			}
		}

		public void Set(object other)
		{
			Set(other as AddNotifyJoinGameAcceptedOptions);
		}

		public void Dispose()
		{
		}
	}
}
