using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatClient
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct BeginSessionOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private AntiCheatClientMode m_Mode;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public AntiCheatClientMode Mode
		{
			set
			{
				m_Mode = value;
			}
		}

		public void Set(BeginSessionOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 3;
				LocalUserId = other.LocalUserId;
				Mode = other.Mode;
			}
		}

		public void Set(object other)
		{
			Set(other as BeginSessionOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}
