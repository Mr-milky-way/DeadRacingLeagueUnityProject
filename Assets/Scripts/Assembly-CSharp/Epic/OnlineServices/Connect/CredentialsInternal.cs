using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CredentialsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Token;

		private ExternalCredentialType m_Type;

		public string Token
		{
			get
			{
				Helper.TryMarshalGet(m_Token, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Token, value);
			}
		}

		public ExternalCredentialType Type
		{
			get
			{
				return m_Type;
			}
			set
			{
				m_Type = value;
			}
		}

		public void Set(Credentials other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Token = other.Token;
				Type = other.Type;
			}
		}

		public void Set(object other)
		{
			Set(other as Credentials);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Token);
		}
	}
}
