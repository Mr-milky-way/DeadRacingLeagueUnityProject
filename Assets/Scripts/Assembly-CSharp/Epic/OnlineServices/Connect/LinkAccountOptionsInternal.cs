using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LinkAccountOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_ContinuanceToken;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public ContinuanceToken ContinuanceToken
		{
			set
			{
				Helper.TryMarshalSet(ref m_ContinuanceToken, value);
			}
		}

		public void Set(LinkAccountOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				ContinuanceToken = other.ContinuanceToken;
			}
		}

		public void Set(object other)
		{
			Set(other as LinkAccountOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_ContinuanceToken);
		}
	}
}
