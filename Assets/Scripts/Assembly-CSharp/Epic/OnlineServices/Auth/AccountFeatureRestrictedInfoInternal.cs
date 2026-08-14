using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AccountFeatureRestrictedInfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_VerificationURI;

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

		public void Set(AccountFeatureRestrictedInfo other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				VerificationURI = other.VerificationURI;
			}
		}

		public void Set(object other)
		{
			Set(other as AccountFeatureRestrictedInfo);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_VerificationURI);
		}
	}
}
