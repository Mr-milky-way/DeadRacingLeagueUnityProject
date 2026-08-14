using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CreateDeviceIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_DeviceModel;

		public string DeviceModel
		{
			set
			{
				Helper.TryMarshalSet(ref m_DeviceModel, value);
			}
		}

		public void Set(CreateDeviceIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				DeviceModel = other.DeviceModel;
			}
		}

		public void Set(object other)
		{
			Set(other as CreateDeviceIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_DeviceModel);
		}
	}
}
