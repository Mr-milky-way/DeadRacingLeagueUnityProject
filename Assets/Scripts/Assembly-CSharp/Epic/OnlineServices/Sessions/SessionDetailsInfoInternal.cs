using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionDetailsInfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SessionId;

		private IntPtr m_HostAddress;

		private uint m_NumOpenPublicConnections;

		private IntPtr m_Settings;

		public string SessionId
		{
			get
			{
				Helper.TryMarshalGet(m_SessionId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_SessionId, value);
			}
		}

		public string HostAddress
		{
			get
			{
				Helper.TryMarshalGet(m_HostAddress, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_HostAddress, value);
			}
		}

		public uint NumOpenPublicConnections
		{
			get
			{
				return m_NumOpenPublicConnections;
			}
			set
			{
				m_NumOpenPublicConnections = value;
			}
		}

		public SessionDetailsSettings Settings
		{
			get
			{
				Helper.TryMarshalGet<SessionDetailsSettingsInternal, SessionDetailsSettings>(m_Settings, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet<SessionDetailsSettingsInternal, SessionDetailsSettings>(ref m_Settings, value);
			}
		}

		public void Set(SessionDetailsInfo other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SessionId = other.SessionId;
				HostAddress = other.HostAddress;
				NumOpenPublicConnections = other.NumOpenPublicConnections;
				Settings = other.Settings;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionDetailsInfo);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SessionId);
			Helper.TryMarshalDispose(ref m_HostAddress);
			Helper.TryMarshalDispose(ref m_Settings);
		}
	}
}
