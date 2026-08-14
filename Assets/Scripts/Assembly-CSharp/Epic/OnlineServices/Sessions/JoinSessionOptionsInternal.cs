using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct JoinSessionOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SessionName;

		private IntPtr m_SessionHandle;

		private IntPtr m_LocalUserId;

		private int m_PresenceEnabled;

		public string SessionName
		{
			set
			{
				Helper.TryMarshalSet(ref m_SessionName, value);
			}
		}

		public SessionDetails SessionHandle
		{
			set
			{
				Helper.TryMarshalSet(ref m_SessionHandle, value);
			}
		}

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public bool PresenceEnabled
		{
			set
			{
				Helper.TryMarshalSet(ref m_PresenceEnabled, value);
			}
		}

		public void Set(JoinSessionOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				SessionName = other.SessionName;
				SessionHandle = other.SessionHandle;
				LocalUserId = other.LocalUserId;
				PresenceEnabled = other.PresenceEnabled;
			}
		}

		public void Set(object other)
		{
			Set(other as JoinSessionOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SessionName);
			Helper.TryMarshalDispose(ref m_SessionHandle);
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}
