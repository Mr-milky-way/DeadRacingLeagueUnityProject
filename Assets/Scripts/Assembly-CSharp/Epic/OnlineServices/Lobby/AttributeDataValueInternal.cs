using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Explicit, Pack = 8)]
	internal struct AttributeDataValueInternal : ISettable, IDisposable
	{
		[FieldOffset(0)]
		private long m_AsInt64;

		[FieldOffset(0)]
		private double m_AsDouble;

		[FieldOffset(0)]
		private int m_AsBool;

		[FieldOffset(0)]
		private IntPtr m_AsUtf8;

		[FieldOffset(8)]
		private AttributeType m_ValueType;

		public long? AsInt64
		{
			get
			{
				Helper.TryMarshalGet(m_AsInt64, out long? target, m_ValueType, AttributeType.Int64);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AsInt64, value, ref m_ValueType, AttributeType.Int64, this);
			}
		}

		public double? AsDouble
		{
			get
			{
				Helper.TryMarshalGet(m_AsDouble, out double? target, m_ValueType, AttributeType.Double);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AsDouble, value, ref m_ValueType, AttributeType.Double, this);
			}
		}

		public bool? AsBool
		{
			get
			{
				Helper.TryMarshalGet(m_AsBool, out bool? target, m_ValueType, AttributeType.Boolean);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AsBool, value, ref m_ValueType, AttributeType.Boolean, this);
			}
		}

		public string AsUtf8
		{
			get
			{
				Helper.TryMarshalGet(m_AsUtf8, out string target, m_ValueType, AttributeType.String);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AsUtf8, value, ref m_ValueType, AttributeType.String, this);
			}
		}

		public void Set(AttributeDataValue other)
		{
			if (other != null)
			{
				AsInt64 = other.AsInt64;
				AsDouble = other.AsDouble;
				AsBool = other.AsBool;
				AsUtf8 = other.AsUtf8;
			}
		}

		public void Set(object other)
		{
			Set(other as AttributeDataValue);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AsUtf8, m_ValueType, AttributeType.String);
		}
	}
}
