using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatClient
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct EndSessionOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(EndSessionOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
			}
		}

		public void Set(object other)
		{
			Set(other as EndSessionOptions);
		}

		public void Dispose()
		{
		}
	}
}
