using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LinkAccountOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private LinkAccountFlags m_LinkAccountFlags;

		private IntPtr m_ContinuanceToken;

		private IntPtr m_LocalUserId;

		public LinkAccountFlags LinkAccountFlags
		{
			set
			{
				m_LinkAccountFlags = value;
			}
		}

		public ContinuanceToken ContinuanceToken
		{
			set
			{
				Helper.TryMarshalSet(ref m_ContinuanceToken, value);
			}
		}

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public void Set(LinkAccountOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LinkAccountFlags = other.LinkAccountFlags;
				ContinuanceToken = other.ContinuanceToken;
				LocalUserId = other.LocalUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as LinkAccountOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ContinuanceToken);
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}
