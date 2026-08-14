using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct StatInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Name;

		private long m_StartTime;

		private long m_EndTime;

		private int m_Value;

		public string Name
		{
			get
			{
				Helper.TryMarshalGet(m_Name, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Name, value);
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

		public int Value
		{
			get
			{
				return m_Value;
			}
			set
			{
				m_Value = value;
			}
		}

		public void Set(Stat other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Name = other.Name;
				StartTime = other.StartTime;
				EndTime = other.EndTime;
				Value = other.Value;
			}
		}

		public void Set(object other)
		{
			Set(other as Stat);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Name);
		}
	}
}
