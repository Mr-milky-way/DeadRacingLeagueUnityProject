using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct DeleteDeviceIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(DeleteDeviceIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
			}
		}

		public void Set(object other)
		{
			Set(other as DeleteDeviceIdOptions);
		}

		public void Dispose()
		{
		}
	}
}
