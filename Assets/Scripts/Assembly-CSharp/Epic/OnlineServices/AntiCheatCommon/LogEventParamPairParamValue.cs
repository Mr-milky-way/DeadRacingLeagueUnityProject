using System;

namespace Epic.OnlineServices.AntiCheatCommon
{
	public class LogEventParamPairParamValue : ISettable
	{
		private AntiCheatCommonEventParamType m_ParamValueType;

		private IntPtr? m_ClientHandle;

		private string m_String;

		private uint? m_UInt32;

		private int? m_Int32;

		private ulong? m_UInt64;

		private long? m_Int64;

		private Vec3f m_Vec3f;

		private Quat m_Quat;

		public AntiCheatCommonEventParamType ParamValueType
		{
			get
			{
				return m_ParamValueType;
			}
			private set
			{
				m_ParamValueType = value;
			}
		}

		public IntPtr? ClientHandle
		{
			get
			{
				Helper.TryMarshalGet(m_ClientHandle, out var target, m_ParamValueType, AntiCheatCommonEventParamType.ClientHandle);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ClientHandle, value, ref m_ParamValueType, AntiCheatCommonEventParamType.ClientHandle);
			}
		}

		public string String
		{
			get
			{
				Helper.TryMarshalGet(m_String, out var target, m_ParamValueType, AntiCheatCommonEventParamType.String);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_String, value, ref m_ParamValueType, AntiCheatCommonEventParamType.String);
			}
		}

		public uint? UInt32
		{
			get
			{
				Helper.TryMarshalGet(m_UInt32, out var target, m_ParamValueType, AntiCheatCommonEventParamType.UInt32);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UInt32, value, ref m_ParamValueType, AntiCheatCommonEventParamType.UInt32);
			}
		}

		public int? Int32
		{
			get
			{
				Helper.TryMarshalGet(m_Int32, out var target, m_ParamValueType, AntiCheatCommonEventParamType.Int32);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Int32, value, ref m_ParamValueType, AntiCheatCommonEventParamType.Int32);
			}
		}

		public ulong? UInt64
		{
			get
			{
				Helper.TryMarshalGet(m_UInt64, out var target, m_ParamValueType, AntiCheatCommonEventParamType.UInt64);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UInt64, value, ref m_ParamValueType, AntiCheatCommonEventParamType.UInt64);
			}
		}

		public long? Int64
		{
			get
			{
				Helper.TryMarshalGet(m_Int64, out var target, m_ParamValueType, AntiCheatCommonEventParamType.Int64);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Int64, value, ref m_ParamValueType, AntiCheatCommonEventParamType.Int64);
			}
		}

		public Vec3f Vec3f
		{
			get
			{
				Helper.TryMarshalGet(m_Vec3f, out var target, m_ParamValueType, AntiCheatCommonEventParamType.Vector3f);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Vec3f, value, ref m_ParamValueType, AntiCheatCommonEventParamType.Vector3f);
			}
		}

		public Quat Quat
		{
			get
			{
				Helper.TryMarshalGet(m_Quat, out var target, m_ParamValueType, AntiCheatCommonEventParamType.Quat);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Quat, value, ref m_ParamValueType, AntiCheatCommonEventParamType.Quat);
			}
		}

		public static implicit operator LogEventParamPairParamValue(IntPtr value)
		{
			return new LogEventParamPairParamValue
			{
				ClientHandle = value
			};
		}

		public static implicit operator LogEventParamPairParamValue(string value)
		{
			return new LogEventParamPairParamValue
			{
				String = value
			};
		}

		public static implicit operator LogEventParamPairParamValue(uint value)
		{
			return new LogEventParamPairParamValue
			{
				UInt32 = value
			};
		}

		public static implicit operator LogEventParamPairParamValue(int value)
		{
			return new LogEventParamPairParamValue
			{
				Int32 = value
			};
		}

		public static implicit operator LogEventParamPairParamValue(ulong value)
		{
			return new LogEventParamPairParamValue
			{
				UInt64 = value
			};
		}

		public static implicit operator LogEventParamPairParamValue(long value)
		{
			return new LogEventParamPairParamValue
			{
				Int64 = value
			};
		}

		public static implicit operator LogEventParamPairParamValue(Vec3f value)
		{
			return new LogEventParamPairParamValue
			{
				Vec3f = value
			};
		}

		public static implicit operator LogEventParamPairParamValue(Quat value)
		{
			return new LogEventParamPairParamValue
			{
				Quat = value
			};
		}

		internal void Set(LogEventParamPairParamValueInternal? other)
		{
			if (other.HasValue)
			{
				ClientHandle = other.Value.ClientHandle;
				String = other.Value.String;
				UInt32 = other.Value.UInt32;
				Int32 = other.Value.Int32;
				UInt64 = other.Value.UInt64;
				Int64 = other.Value.Int64;
				Vec3f = other.Value.Vec3f;
				Quat = other.Value.Quat;
			}
		}

		public void Set(object other)
		{
			Set(other as LogEventParamPairParamValueInternal?);
		}
	}
}
