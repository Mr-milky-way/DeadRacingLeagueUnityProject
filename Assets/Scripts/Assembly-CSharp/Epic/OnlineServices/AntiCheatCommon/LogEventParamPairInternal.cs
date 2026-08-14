using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogEventParamPairInternal : ISettable, IDisposable
	{
		private LogEventParamPairParamValueInternal m_ParamValue;

		public LogEventParamPairParamValue ParamValue
		{
			get
			{
				Helper.TryMarshalGet<LogEventParamPairParamValueInternal, LogEventParamPairParamValue>(m_ParamValue, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ParamValue, value);
			}
		}

		public void Set(LogEventParamPair other)
		{
			if (other != null)
			{
				ParamValue = other.ParamValue;
			}
		}

		public void Set(object other)
		{
			Set(other as LogEventParamPair);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ParamValue);
		}
	}
}
