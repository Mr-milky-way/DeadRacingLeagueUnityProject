using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct IngestDataInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_StatName;

		private int m_IngestAmount;

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

		public int IngestAmount
		{
			get
			{
				return m_IngestAmount;
			}
			set
			{
				m_IngestAmount = value;
			}
		}

		public void Set(IngestData other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				StatName = other.StatName;
				IngestAmount = other.IngestAmount;
			}
		}

		public void Set(object other)
		{
			Set(other as IngestData);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_StatName);
		}
	}
}
