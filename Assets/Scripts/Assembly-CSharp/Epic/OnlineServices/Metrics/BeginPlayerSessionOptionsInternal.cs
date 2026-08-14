using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Metrics
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct BeginPlayerSessionOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private BeginPlayerSessionOptionsAccountIdInternal m_AccountId;

		private IntPtr m_DisplayName;

		private UserControllerType m_ControllerType;

		private IntPtr m_ServerIp;

		private IntPtr m_GameSessionId;

		public BeginPlayerSessionOptionsAccountId AccountId
		{
			set
			{
				Helper.TryMarshalSet(ref m_AccountId, value);
			}
		}

		public string DisplayName
		{
			set
			{
				Helper.TryMarshalSet(ref m_DisplayName, value);
			}
		}

		public UserControllerType ControllerType
		{
			set
			{
				m_ControllerType = value;
			}
		}

		public string ServerIp
		{
			set
			{
				Helper.TryMarshalSet(ref m_ServerIp, value);
			}
		}

		public string GameSessionId
		{
			set
			{
				Helper.TryMarshalSet(ref m_GameSessionId, value);
			}
		}

		public void Set(BeginPlayerSessionOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				AccountId = other.AccountId;
				DisplayName = other.DisplayName;
				ControllerType = other.ControllerType;
				ServerIp = other.ServerIp;
				GameSessionId = other.GameSessionId;
			}
		}

		public void Set(object other)
		{
			Set(other as BeginPlayerSessionOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AccountId);
			Helper.TryMarshalDispose(ref m_DisplayName);
			Helper.TryMarshalDispose(ref m_ServerIp);
			Helper.TryMarshalDispose(ref m_GameSessionId);
		}
	}
}
