namespace Epic.OnlineServices.Lobby
{
	public class AttributeDataValue : ISettable
	{
		private long? m_AsInt64;

		private double? m_AsDouble;

		private bool? m_AsBool;

		private string m_AsUtf8;

		private AttributeType m_ValueType;

		public long? AsInt64
		{
			get
			{
				Helper.TryMarshalGet(m_AsInt64, out var target, m_ValueType, AttributeType.Int64);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AsInt64, value, ref m_ValueType, AttributeType.Int64);
			}
		}

		public double? AsDouble
		{
			get
			{
				Helper.TryMarshalGet(m_AsDouble, out var target, m_ValueType, AttributeType.Double);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AsDouble, value, ref m_ValueType, AttributeType.Double);
			}
		}

		public bool? AsBool
		{
			get
			{
				Helper.TryMarshalGet(m_AsBool, out var target, m_ValueType, AttributeType.Boolean);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AsBool, value, ref m_ValueType, AttributeType.Boolean);
			}
		}

		public string AsUtf8
		{
			get
			{
				Helper.TryMarshalGet(m_AsUtf8, out var target, m_ValueType, AttributeType.String);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AsUtf8, value, ref m_ValueType, AttributeType.String);
			}
		}

		public AttributeType ValueType
		{
			get
			{
				return m_ValueType;
			}
			private set
			{
				m_ValueType = value;
			}
		}

		public static implicit operator AttributeDataValue(long value)
		{
			return new AttributeDataValue
			{
				AsInt64 = value
			};
		}

		public static implicit operator AttributeDataValue(double value)
		{
			return new AttributeDataValue
			{
				AsDouble = value
			};
		}

		public static implicit operator AttributeDataValue(bool value)
		{
			return new AttributeDataValue
			{
				AsBool = value
			};
		}

		public static implicit operator AttributeDataValue(string value)
		{
			return new AttributeDataValue
			{
				AsUtf8 = value
			};
		}

		internal void Set(AttributeDataValueInternal? other)
		{
			if (other.HasValue)
			{
				AsInt64 = other.Value.AsInt64;
				AsDouble = other.Value.AsDouble;
				AsBool = other.Value.AsBool;
				AsUtf8 = other.Value.AsUtf8;
			}
		}

		public void Set(object other)
		{
			Set(other as AttributeDataValueInternal?);
		}
	}
}
