using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PinGrantInfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_UserCode;

		private IntPtr m_VerificationURI;

		private int m_ExpiresIn;

		private IntPtr m_VerificationURIComplete;

		public string UserCode
		{
			get
			{
				Helper.TryMarshalGet(m_UserCode, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UserCode, value);
			}
		}

		public string VerificationURI
		{
			get
			{
				Helper.TryMarshalGet(m_VerificationURI, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_VerificationURI, value);
			}
		}

		public int ExpiresIn
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

		public string VerificationURIComplete
		{
			get
			{
				Helper.TryMarshalGet(m_VerificationURIComplete, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_VerificationURIComplete, value);
			}
		}

		public void Set(PinGrantInfo other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				UserCode = other.UserCode;
				VerificationURI = other.VerificationURI;
				ExpiresIn = other.ExpiresIn;
				VerificationURIComplete = other.VerificationURIComplete;
			}
		}

		public void Set(object other)
		{
			Set(other as PinGrantInfo);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserCode);
			Helper.TryMarshalDispose(ref m_VerificationURI);
			Helper.TryMarshalDispose(ref m_VerificationURIComplete);
		}
	}
}
