using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct DeletePersistentAuthOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_RefreshToken;

		public string RefreshToken
		{
			set
			{
				Helper.TryMarshalSet(ref m_RefreshToken, value);
			}
		}

		public void Set(DeletePersistentAuthOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				RefreshToken = other.RefreshToken;
			}
		}

		public void Set(object other)
		{
			Set(other as DeletePersistentAuthOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_RefreshToken);
		}
	}
}
