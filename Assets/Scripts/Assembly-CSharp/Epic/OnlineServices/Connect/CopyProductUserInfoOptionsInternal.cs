using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyProductUserInfoOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_TargetUserId;

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public void Set(CopyProductUserInfoOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				TargetUserId = other.TargetUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyProductUserInfoOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_TargetUserId);
		}
	}
}
