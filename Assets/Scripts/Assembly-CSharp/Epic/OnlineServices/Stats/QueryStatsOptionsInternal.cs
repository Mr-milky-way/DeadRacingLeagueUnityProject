using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryStatsOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private long m_StartTime;

		private long m_EndTime;

		private IntPtr m_StatNames;

		private uint m_StatNamesCount;

		private IntPtr m_TargetUserId;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public DateTimeOffset? StartTime
		{
			set
			{
				Helper.TryMarshalSet(ref m_StartTime, value);
			}
		}

		public DateTimeOffset? EndTime
		{
			set
			{
				Helper.TryMarshalSet(ref m_EndTime, value);
			}
		}

		public string[] StatNames
		{
			set
			{
				Helper.TryMarshalSet(ref m_StatNames, value, out m_StatNamesCount, true);
			}
		}

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public void Set(QueryStatsOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 3;
				LocalUserId = other.LocalUserId;
				StartTime = other.StartTime;
				EndTime = other.EndTime;
				StatNames = other.StatNames;
				TargetUserId = other.TargetUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryStatsOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_StatNames);
			Helper.TryMarshalDispose(ref m_TargetUserId);
		}
	}
}
