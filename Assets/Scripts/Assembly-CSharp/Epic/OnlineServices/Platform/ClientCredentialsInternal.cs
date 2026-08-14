using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Platform
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ClientCredentialsInternal : ISettable, IDisposable
	{
		private IntPtr m_ClientId;

		private IntPtr m_ClientSecret;

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

		public string ClientSecret
		{
			get
			{
				Helper.TryMarshalGet(m_ClientSecret, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ClientSecret, value);
			}
		}

		public void Set(ClientCredentials other)
		{
			if (other != null)
			{
				ClientId = other.ClientId;
				ClientSecret = other.ClientSecret;
			}
		}

		public void Set(object other)
		{
			Set(other as ClientCredentials);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ClientId);
			Helper.TryMarshalDispose(ref m_ClientSecret);
		}
	}
}
