using System;
using System.Runtime.InteropServices;
using Epic.OnlineServices.AntiCheatCommon;

namespace Epic.OnlineServices.AntiCheatServer
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct RegisterClientOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_ClientHandle;

		private AntiCheatCommonClientType m_ClientType;

		private AntiCheatCommonClientPlatform m_ClientPlatform;

		private IntPtr m_AccountId;

		private IntPtr m_IpAddress;

		public IntPtr ClientHandle
		{
			set
			{
				m_ClientHandle = value;
			}
		}

		public AntiCheatCommonClientType ClientType
		{
			set
			{
				m_ClientType = value;
			}
		}

		public AntiCheatCommonClientPlatform ClientPlatform
		{
			set
			{
				m_ClientPlatform = value;
			}
		}

		public string AccountId
		{
			set
			{
				Helper.TryMarshalSet(ref m_AccountId, value);
			}
		}

		public string IpAddress
		{
			set
			{
				Helper.TryMarshalSet(ref m_IpAddress, value);
			}
		}

		public void Set(RegisterClientOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				ClientHandle = other.ClientHandle;
				ClientType = other.ClientType;
				ClientPlatform = other.ClientPlatform;
				AccountId = other.AccountId;
				IpAddress = other.IpAddress;
			}
		}

		public void Set(object other)
		{
			Set(other as RegisterClientOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ClientHandle);
			Helper.TryMarshalDispose(ref m_AccountId);
			Helper.TryMarshalDispose(ref m_IpAddress);
		}
	}
}
