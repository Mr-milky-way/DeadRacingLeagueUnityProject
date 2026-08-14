using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct RegisterEventParamDefInternal : ISettable, IDisposable
	{
		private IntPtr m_ParamName;

		private AntiCheatCommonEventParamType m_ParamType;

		public string ParamName
		{
			get
			{
				Helper.TryMarshalGet(m_ParamName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ParamName, value);
			}
		}

		public AntiCheatCommonEventParamType ParamType
		{
			get
			{
				return m_ParamType;
			}
			set
			{
				m_ParamType = value;
			}
		}

		public void Set(RegisterEventParamDef other)
		{
			if (other != null)
			{
				ParamName = other.ParamName;
				ParamType = other.ParamType;
			}
		}

		public void Set(object other)
		{
			Set(other as RegisterEventParamDef);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ParamName);
		}
	}
}
