using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyStatByNameOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_TargetUserId;

		private IntPtr m_Name;

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public string Name
		{
			set
			{
				Helper.TryMarshalSet(ref m_Name, value);
			}
		}

		public void Set(CopyStatByNameOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				TargetUserId = other.TargetUserId;
				Name = other.Name;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyStatByNameOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_TargetUserId);
			Helper.TryMarshalDispose(ref m_Name);
		}
	}
}
