using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct DataRecordInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Key;

		private IntPtr m_Value;

		public string Key
		{
			get
			{
				Helper.TryMarshalGet(m_Key, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Key, value);
			}
		}

		public string Value
		{
			get
			{
				Helper.TryMarshalGet(m_Value, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Value, value);
			}
		}

		public void Set(DataRecord other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Key = other.Key;
				Value = other.Value;
			}
		}

		public void Set(object other)
		{
			Set(other as DataRecord);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Key);
			Helper.TryMarshalDispose(ref m_Value);
		}
	}
}
