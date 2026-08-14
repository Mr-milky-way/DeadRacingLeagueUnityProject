using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct IOSCredentialsSystemAuthCredentialsOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_PresentationContextProviding;

		public IntPtr PresentationContextProviding
		{
			get
			{
				return m_PresentationContextProviding;
			}
			set
			{
				m_PresentationContextProviding = value;
			}
		}

		public void Set(IOSCredentialsSystemAuthCredentialsOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				PresentationContextProviding = other.PresentationContextProviding;
			}
		}

		public void Set(object other)
		{
			Set(other as IOSCredentialsSystemAuthCredentialsOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_PresentationContextProviding);
		}
	}
}
