using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Explicit, Pack = 8)]
	internal struct LogEventParamPairParamValueInternal : ISettable, IDisposable
	{
		[FieldOffset(0)]
		private AntiCheatCommonEventParamType m_ParamValueType;

		[FieldOffset(8)]
		private IntPtr m_ClientHandle;

		[FieldOffset(8)]
		private IntPtr m_String;

		[FieldOffset(8)]
		private uint m_UInt32;

		[FieldOffset(8)]
		private int m_Int32;

		[FieldOffset(8)]
		private ulong m_UInt64;

		[FieldOffset(8)]
		private long m_Int64;

		[FieldOffset(8)]
		private Vec3fInternal m_Vec3f;

		[FieldOffset(8)]
		private QuatInternal m_Quat;

		public IntPtr? ClientHandle
		{
			get
			{
				Helper.TryMarshalGet(m_ClientHandle, out IntPtr? target, m_ParamValueType, AntiCheatCommonEventParamType.ClientHandle);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ClientHandle, value, ref m_ParamValueType, AntiCheatCommonEventParamType.ClientHandle, this);
			}
		}

		public string String
		{
			get
			{
				Helper.TryMarshalGet(m_String, out string target, m_ParamValueType, AntiCheatCommonEventParamType.String);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_String, value, ref m_ParamValueType, AntiCheatCommonEventParamType.String, this);
			}
		}

		public uint? UInt32
		{
			get
			{
				Helper.TryMarshalGet(m_UInt32, out uint? target, m_ParamValueType, AntiCheatCommonEventParamType.UInt32);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UInt32, value, ref m_ParamValueType, AntiCheatCommonEventParamType.UInt32, this);
			}
		}

		public int? Int32
		{
			get
			{
				Helper.TryMarshalGet(m_Int32, out int? target, m_ParamValueType, AntiCheatCommonEventParamType.Int32);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Int32, value, ref m_ParamValueType, AntiCheatCommonEventParamType.Int32, this);
			}
		}

		public ulong? UInt64
		{
			get
			{
				Helper.TryMarshalGet(m_UInt64, out ulong? target, m_ParamValueType, AntiCheatCommonEventParamType.UInt64);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UInt64, value, ref m_ParamValueType, AntiCheatCommonEventParamType.UInt64, this);
			}
		}

		public long? Int64
		{
			get
			{
				Helper.TryMarshalGet(m_Int64, out long? target, m_ParamValueType, AntiCheatCommonEventParamType.Int64);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Int64, value, ref m_ParamValueType, AntiCheatCommonEventParamType.Int64, this);
			}
		}

		public Vec3f Vec3f
		{
			get
			{
				Helper.TryMarshalGet((ISettable)m_Vec3f, out Vec3f target, m_ParamValueType, AntiCheatCommonEventParamType.Vector3f);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Vec3f, value, ref m_ParamValueType, AntiCheatCommonEventParamType.Vector3f, this);
			}
		}

		public Quat Quat
		{
			get
			{
				Helper.TryMarshalGet((ISettable)m_Quat, out Quat target, m_ParamValueType, AntiCheatCommonEventParamType.Quat);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Quat, value, ref m_ParamValueType, AntiCheatCommonEventParamType.Quat, this);
			}
		}

		public void Set(LogEventParamPairParamValue other)
		{
			if (other != null)
			{
				ClientHandle = other.ClientHandle;
				String = other.String;
				UInt32 = other.UInt32;
				Int32 = other.Int32;
				UInt64 = other.UInt64;
				Int64 = other.Int64;
				Vec3f = other.Vec3f;
				Quat = other.Quat;
			}
		}

		public void Set(object other)
		{
			Set(other as LogEventParamPairParamValue);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ClientHandle, m_ParamValueType, AntiCheatCommonEventParamType.ClientHandle);
			Helper.TryMarshalDispose(ref m_String, m_ParamValueType, AntiCheatCommonEventParamType.String);
			Helper.TryMarshalDispose(ref m_Vec3f);
			Helper.TryMarshalDispose(ref m_Quat);
		}
	}
}
