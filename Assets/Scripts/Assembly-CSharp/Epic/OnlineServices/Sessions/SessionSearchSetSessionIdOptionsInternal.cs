using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionSearchSetSessionIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SessionId;

		public string SessionId
		{
			set
			{
				Helper.TryMarshalSet(ref m_SessionId, value);
			}
		}

		public void Set(SessionSearchSetSessionIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SessionId = other.SessionId;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionSearchSetSessionIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SessionId);
		}
	}
}
