using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogPlayerReviveOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_RevivedPlayerHandle;

		private IntPtr m_ReviverPlayerHandle;

		public IntPtr RevivedPlayerHandle
		{
			set
			{
				m_RevivedPlayerHandle = value;
			}
		}

		public IntPtr ReviverPlayerHandle
		{
			set
			{
				m_ReviverPlayerHandle = value;
			}
		}

		public void Set(LogPlayerReviveOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				RevivedPlayerHandle = other.RevivedPlayerHandle;
				ReviverPlayerHandle = other.ReviverPlayerHandle;
			}
		}

		public void Set(object other)
		{
			Set(other as LogPlayerReviveOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_RevivedPlayerHandle);
			Helper.TryMarshalDispose(ref m_ReviverPlayerHandle);
		}
	}
}
