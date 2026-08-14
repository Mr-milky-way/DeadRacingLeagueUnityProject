using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SetPresenceOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_PresenceModificationHandle;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public PresenceModification PresenceModificationHandle
		{
			set
			{
				Helper.TryMarshalSet(ref m_PresenceModificationHandle, value);
			}
		}

		public void Set(SetPresenceOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				PresenceModificationHandle = other.PresenceModificationHandle;
			}
		}

		public void Set(object other)
		{
			Set(other as SetPresenceOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_PresenceModificationHandle);
		}
	}
}
