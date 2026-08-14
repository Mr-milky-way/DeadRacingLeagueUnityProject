using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatServer
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UnregisterClientOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_ClientHandle;

		public IntPtr ClientHandle
		{
			set
			{
				m_ClientHandle = value;
			}
		}

		public void Set(UnregisterClientOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				ClientHandle = other.ClientHandle;
			}
		}

		public void Set(object other)
		{
			Set(other as UnregisterClientOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ClientHandle);
		}
	}
}
