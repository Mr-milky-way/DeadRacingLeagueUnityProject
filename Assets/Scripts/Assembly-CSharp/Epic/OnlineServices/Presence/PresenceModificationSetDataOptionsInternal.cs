using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PresenceModificationSetDataOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private int m_RecordsCount;

		private IntPtr m_Records;

		public DataRecord[] Records
		{
			set
			{
				Helper.TryMarshalSet<DataRecordInternal, DataRecord>(ref m_Records, value, out m_RecordsCount);
			}
		}

		public void Set(PresenceModificationSetDataOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Records = other.Records;
			}
		}

		public void Set(object other)
		{
			Set(other as PresenceModificationSetDataOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Records);
		}
	}
}
