using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.KWS
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PermissionStatusInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Name;

		private KWSPermissionStatus m_Status;

		public string Name
		{
			get
			{
				Helper.TryMarshalGet(m_Name, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Name, value);
			}
		}

		public KWSPermissionStatus Status
		{
			get
			{
				return m_Status;
			}
			set
			{
				m_Status = value;
			}
		}

		public void Set(PermissionStatus other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Name = other.Name;
				Status = other.Status;
			}
		}

		public void Set(object other)
		{
			Set(other as PermissionStatus);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Name);
		}
	}
}
