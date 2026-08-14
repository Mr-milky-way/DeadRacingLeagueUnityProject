using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatServer
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct BeginSessionOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_RegisterTimeoutSeconds;

		private IntPtr m_ServerName;

		private int m_EnableGameplayData;

		private IntPtr m_LocalUserId;

		public uint RegisterTimeoutSeconds
		{
			set
			{
				m_RegisterTimeoutSeconds = value;
			}
		}

		public string ServerName
		{
			set
			{
				Helper.TryMarshalSet(ref m_ServerName, value);
			}
		}

		public bool EnableGameplayData
		{
			set
			{
				Helper.TryMarshalSet(ref m_EnableGameplayData, value);
			}
		}

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public void Set(BeginSessionOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 3;
				RegisterTimeoutSeconds = other.RegisterTimeoutSeconds;
				ServerName = other.ServerName;
				EnableGameplayData = other.EnableGameplayData;
				LocalUserId = other.LocalUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as BeginSessionOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ServerName);
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}
