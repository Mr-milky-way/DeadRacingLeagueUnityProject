using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PresenceModificationDataRecordIdInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Key;

		public string Key
		{
			get
			{
				Helper.TryMarshalGet(m_Key, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Key, value);
			}
		}

		public void Set(PresenceModificationDataRecordId other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Key = other.Key;
			}
		}

		public void Set(object other)
		{
			Set(other as PresenceModificationDataRecordId);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Key);
		}
	}
}
