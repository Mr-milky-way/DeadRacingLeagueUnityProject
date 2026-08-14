using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PresenceModificationDeleteDataOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private int m_RecordsCount;

		private IntPtr m_Records;

		public PresenceModificationDataRecordId[] Records
		{
			set
			{
				Helper.TryMarshalSet<PresenceModificationDataRecordIdInternal, PresenceModificationDataRecordId>(ref m_Records, value, out m_RecordsCount);
			}
		}

		public void Set(PresenceModificationDeleteDataOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Records = other.Records;
			}
		}

		public void Set(object other)
		{
			Set(other as PresenceModificationDeleteDataOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Records);
		}
	}
}
