using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PresenceModificationSetStatusOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private Status m_Status;

		public Status Status
		{
			set
			{
				m_Status = value;
			}
		}

		public void Set(PresenceModificationSetStatusOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Status = other.Status;
			}
		}

		public void Set(object other)
		{
			Set(other as PresenceModificationSetStatusOptions);
		}

		public void Dispose()
		{
		}
	}
}
