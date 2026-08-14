using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct StatThresholdsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Name;

		private int m_Threshold;

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

		public int Threshold
		{
			get
			{
				return m_Threshold;
			}
			set
			{
				m_Threshold = value;
			}
		}

		public void Set(StatThresholds other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Name = other.Name;
				Threshold = other.Threshold;
			}
		}

		public void Set(object other)
		{
			Set(other as StatThresholds);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Name);
		}
	}
}
