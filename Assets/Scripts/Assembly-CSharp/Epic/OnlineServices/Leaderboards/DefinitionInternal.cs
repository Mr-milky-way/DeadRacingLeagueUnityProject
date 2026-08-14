using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct DefinitionInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LeaderboardId;

		private IntPtr m_StatName;

		private LeaderboardAggregation m_Aggregation;

		private long m_StartTime;

		private long m_EndTime;

		public string LeaderboardId
		{
			get
			{
				Helper.TryMarshalGet(m_LeaderboardId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_LeaderboardId, value);
			}
		}

		public string StatName
		{
			get
			{
				Helper.TryMarshalGet(m_StatName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_StatName, value);
			}
		}

		public LeaderboardAggregation Aggregation
		{
			get
			{
				return m_Aggregation;
			}
			set
			{
				m_Aggregation = value;
			}
		}

		public DateTimeOffset? StartTime
		{
			get
			{
				Helper.TryMarshalGet(m_StartTime, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_StartTime, value);
			}
		}

		public DateTimeOffset? EndTime
		{
			get
			{
				Helper.TryMarshalGet(m_EndTime, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_EndTime, value);
			}
		}

		public void Set(Definition other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LeaderboardId = other.LeaderboardId;
				StatName = other.StatName;
				Aggregation = other.Aggregation;
				StartTime = other.StartTime;
				EndTime = other.EndTime;
			}
		}

		public void Set(object other)
		{
			Set(other as Definition);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LeaderboardId);
			Helper.TryMarshalDispose(ref m_StatName);
		}
	}
}
