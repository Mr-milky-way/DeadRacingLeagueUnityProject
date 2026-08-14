using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CredentialsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Id;

		private IntPtr m_Token;

		private LoginCredentialType m_Type;

		private IntPtr m_SystemAuthCredentialsOptions;

		private ExternalCredentialType m_ExternalType;

		public string Id
		{
			get
			{
				Helper.TryMarshalGet(m_Id, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Id, value);
			}
		}

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

		public LoginCredentialType Type
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

		public IntPtr SystemAuthCredentialsOptions
		{
			get
			{
				return m_SystemAuthCredentialsOptions;
			}
			set
			{
				m_SystemAuthCredentialsOptions = value;
			}
		}

		public ExternalCredentialType ExternalType
		{
			get
			{
				return m_ExternalType;
			}
			set
			{
				m_ExternalType = value;
			}
		}

		public void Set(Credentials other)
		{
			if (other != null)
			{
				m_ApiVersion = 3;
				Id = other.Id;
				Token = other.Token;
				Type = other.Type;
				SystemAuthCredentialsOptions = other.SystemAuthCredentialsOptions;
				ExternalType = other.ExternalType;
			}
		}

		public void Set(object other)
		{
			Set(other as Credentials);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Id);
			Helper.TryMarshalDispose(ref m_Token);
			Helper.TryMarshalDispose(ref m_SystemAuthCredentialsOptions);
		}
	}
}
