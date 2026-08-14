using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ActiveSessionInfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SessionName;

		private IntPtr m_LocalUserId;

		private OnlineSessionState m_State;

		private IntPtr m_SessionDetails;

		public string SessionName
		{
			get
			{
				Helper.TryMarshalGet(m_SessionName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_SessionName, value);
			}
		}

		public ProductUserId LocalUserId
		{
			get
			{
				Helper.TryMarshalGet(m_LocalUserId, out ProductUserId target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public OnlineSessionState State
		{
			get
			{
				return m_State;
			}
			set
			{
				m_State = value;
			}
		}

		public SessionDetailsInfo SessionDetails
		{
			get
			{
				Helper.TryMarshalGet<SessionDetailsInfoInternal, SessionDetailsInfo>(m_SessionDetails, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet<SessionDetailsInfoInternal, SessionDetailsInfo>(ref m_SessionDetails, value);
			}
		}

		public void Set(ActiveSessionInfo other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SessionName = other.SessionName;
				LocalUserId = other.LocalUserId;
				State = other.State;
				SessionDetails = other.SessionDetails;
			}
		}

		public void Set(object other)
		{
			Set(other as ActiveSessionInfo);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SessionName);
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_SessionDetails);
		}
	}
}
