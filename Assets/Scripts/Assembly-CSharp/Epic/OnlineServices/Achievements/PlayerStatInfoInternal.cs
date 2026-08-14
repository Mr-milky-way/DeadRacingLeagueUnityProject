using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PlayerStatInfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Name;

		private int m_CurrentValue;

		private int m_ThresholdValue;

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

		public int CurrentValue
		{
			get
			{
				return m_CurrentValue;
			}
			set
			{
				m_CurrentValue = value;
			}
		}

		public int ThresholdValue
		{
			get
			{
				return m_ThresholdValue;
			}
			set
			{
				m_ThresholdValue = value;
			}
		}

		public void Set(PlayerStatInfo other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Name = other.Name;
				CurrentValue = other.CurrentValue;
				ThresholdValue = other.ThresholdValue;
			}
		}

		public void Set(object other)
		{
			Set(other as PlayerStatInfo);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Name);
		}
	}
}
