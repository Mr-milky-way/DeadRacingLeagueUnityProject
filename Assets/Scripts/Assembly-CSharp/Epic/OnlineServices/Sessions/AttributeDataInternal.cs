using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AttributeDataInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Key;

		private AttributeDataValueInternal m_Value;

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

		public AttributeDataValue Value
		{
			get
			{
				Helper.TryMarshalGet<AttributeDataValueInternal, AttributeDataValue>(m_Value, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Value, value);
			}
		}

		public void Set(AttributeData other)
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
			Set(other as AttributeData);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Key);
			Helper.TryMarshalDispose(ref m_Value);
		}
	}
}
