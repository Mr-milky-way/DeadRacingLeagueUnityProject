using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SendInviteOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SessionName;

		private IntPtr m_LocalUserId;

		private IntPtr m_TargetUserId;

		public string SessionName
		{
			set
			{
				Helper.TryMarshalSet(ref m_SessionName, value);
			}
		}

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public void Set(SendInviteOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SessionName = other.SessionName;
				LocalUserId = other.LocalUserId;
				TargetUserId = other.TargetUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as SendInviteOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SessionName);
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_TargetUserId);
		}
	}
}
