using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Metrics
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct EndPlayerSessionOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private EndPlayerSessionOptionsAccountIdInternal m_AccountId;

		public EndPlayerSessionOptionsAccountId AccountId
		{
			set
			{
				Helper.TryMarshalSet(ref m_AccountId, value);
			}
		}

		public void Set(EndPlayerSessionOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				AccountId = other.AccountId;
			}
		}

		public void Set(object other)
		{
			Set(other as EndPlayerSessionOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AccountId);
		}
	}
}
