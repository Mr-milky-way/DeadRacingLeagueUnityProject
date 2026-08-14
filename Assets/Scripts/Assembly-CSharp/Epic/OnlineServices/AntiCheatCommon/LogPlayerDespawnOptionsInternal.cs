using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogPlayerDespawnOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_DespawnedPlayerHandle;

		public IntPtr DespawnedPlayerHandle
		{
			set
			{
				m_DespawnedPlayerHandle = value;
			}
		}

		public void Set(LogPlayerDespawnOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				DespawnedPlayerHandle = other.DespawnedPlayerHandle;
			}
		}

		public void Set(object other)
		{
			Set(other as LogPlayerDespawnOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_DespawnedPlayerHandle);
		}
	}
}
