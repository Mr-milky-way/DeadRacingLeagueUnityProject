using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct TokenInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_App;

		private IntPtr m_ClientId;

		private IntPtr m_AccountId;

		private IntPtr m_AccessToken;

		private double m_ExpiresIn;

		private IntPtr m_ExpiresAt;

		private AuthTokenType m_AuthType;

		private IntPtr m_RefreshToken;

		private double m_RefreshExpiresIn;

		private IntPtr m_RefreshExpiresAt;

		public string App
		{
			get
			{
				Helper.TryMarshalGet(m_App, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_App, value);
			}
		}

		public string ClientId
		{
			get
			{
				Helper.TryMarshalGet(m_ClientId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ClientId, value);
			}
		}

		public EpicAccountId AccountId
		{
			get
			{
				Helper.TryMarshalGet(m_AccountId, out EpicAccountId target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AccountId, value);
			}
		}

		public string AccessToken
		{
			get
			{
				Helper.TryMarshalGet(m_AccessToken, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AccessToken, value);
			}
		}

		public double ExpiresIn
		{
			get
			{
				return m_ExpiresIn;
			}
			set
			{
				m_ExpiresIn = value;
			}
		}

		public string ExpiresAt
		{
			get
			{
				Helper.TryMarshalGet(m_ExpiresAt, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ExpiresAt, value);
			}
		}

		public AuthTokenType AuthType
		{
			get
			{
				return m_AuthType;
			}
			set
			{
				m_AuthType = value;
			}
		}

		public string RefreshToken
		{
			get
			{
				Helper.TryMarshalGet(m_RefreshToken, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_RefreshToken, value);
			}
		}

		public double RefreshExpiresIn
		{
			get
			{
				return m_RefreshExpiresIn;
			}
			set
			{
				m_RefreshExpiresIn = value;
			}
		}

		public string RefreshExpiresAt
		{
			get
			{
				Helper.TryMarshalGet(m_RefreshExpiresAt, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_RefreshExpiresAt, value);
			}
		}

		public void Set(Token other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				App = other.App;
				ClientId = other.ClientId;
				AccountId = other.AccountId;
				AccessToken = other.AccessToken;
				ExpiresIn = other.ExpiresIn;
				ExpiresAt = other.ExpiresAt;
				AuthType = other.AuthType;
				RefreshToken = other.RefreshToken;
				RefreshExpiresIn = other.RefreshExpiresIn;
				RefreshExpiresAt = other.RefreshExpiresAt;
			}
		}

		public void Set(object other)
		{
			Set(other as Token);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_App);
			Helper.TryMarshalDispose(ref m_ClientId);
			Helper.TryMarshalDispose(ref m_AccountId);
			Helper.TryMarshalDispose(ref m_AccessToken);
			Helper.TryMarshalDispose(ref m_ExpiresAt);
			Helper.TryMarshalDispose(ref m_RefreshToken);
			Helper.TryMarshalDispose(ref m_RefreshExpiresAt);
		}
	}
}
