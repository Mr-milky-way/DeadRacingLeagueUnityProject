using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatServer
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SetClientNetworkStateOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_ClientHandle;

		private int m_IsNetworkActive;

		public IntPtr ClientHandle
		{
			set
			{
				m_ClientHandle = value;
			}
		}

		public bool IsNetworkActive
		{
			set
			{
				Helper.TryMarshalSet(ref m_IsNetworkActive, value);
			}
		}

		public void Set(SetClientNetworkStateOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				ClientHandle = other.ClientHandle;
				IsNetworkActive = other.IsNetworkActive;
			}
		}

		public void Set(object other)
		{
			Set(other as SetClientNetworkStateOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ClientHandle);
		}
	}
}
