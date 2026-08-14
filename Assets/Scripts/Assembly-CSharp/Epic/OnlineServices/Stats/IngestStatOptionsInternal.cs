using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct IngestStatOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_Stats;

		private uint m_StatsCount;

		private IntPtr m_TargetUserId;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public IngestData[] Stats
		{
			set
			{
				Helper.TryMarshalSet<IngestDataInternal, IngestData>(ref m_Stats, value, out m_StatsCount);
			}
		}

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public void Set(IngestStatOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 3;
				LocalUserId = other.LocalUserId;
				Stats = other.Stats;
				TargetUserId = other.TargetUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as IngestStatOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_Stats);
			Helper.TryMarshalDispose(ref m_TargetUserId);
		}
	}
}
