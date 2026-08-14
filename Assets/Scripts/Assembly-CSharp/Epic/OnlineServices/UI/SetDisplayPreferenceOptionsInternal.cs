using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UI
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SetDisplayPreferenceOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private NotificationLocation m_NotificationLocation;

		public NotificationLocation NotificationLocation
		{
			set
			{
				m_NotificationLocation = value;
			}
		}

		public void Set(SetDisplayPreferenceOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				NotificationLocation = other.NotificationLocation;
			}
		}

		public void Set(object other)
		{
			Set(other as SetDisplayPreferenceOptions);
		}

		public void Dispose()
		{
		}
	}
}
