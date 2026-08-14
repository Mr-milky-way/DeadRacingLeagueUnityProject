using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatClient
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AddNotifyMessageToServerOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(AddNotifyMessageToServerOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
			}
		}

		public void Set(object other)
		{
			Set(other as AddNotifyMessageToServerOptions);
		}

		public void Dispose()
		{
		}
	}
}
