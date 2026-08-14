using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CreateUserOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_ContinuanceToken;

		public ContinuanceToken ContinuanceToken
		{
			set
			{
				Helper.TryMarshalSet(ref m_ContinuanceToken, value);
			}
		}

		public void Set(CreateUserOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				ContinuanceToken = other.ContinuanceToken;
			}
		}

		public void Set(object other)
		{
			Set(other as CreateUserOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ContinuanceToken);
		}
	}
}
