using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UpdateSessionOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SessionModificationHandle;

		public SessionModification SessionModificationHandle
		{
			set
			{
				Helper.TryMarshalSet(ref m_SessionModificationHandle, value);
			}
		}

		public void Set(UpdateSessionOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SessionModificationHandle = other.SessionModificationHandle;
			}
		}

		public void Set(object other)
		{
			Set(other as UpdateSessionOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SessionModificationHandle);
		}
	}
}
