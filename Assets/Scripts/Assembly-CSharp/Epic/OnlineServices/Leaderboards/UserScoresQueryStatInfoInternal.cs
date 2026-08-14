using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UserScoresQueryStatInfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_StatName;

		private LeaderboardAggregation m_Aggregation;

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

		public void Set(UserScoresQueryStatInfo other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				StatName = other.StatName;
				Aggregation = other.Aggregation;
			}
		}

		public void Set(object other)
		{
			Set(other as UserScoresQueryStatInfo);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_StatName);
		}
	}
}
