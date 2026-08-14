using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogEventOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_ClientHandle;

		private uint m_EventId;

		private uint m_ParamsCount;

		private IntPtr m_Params;

		public IntPtr ClientHandle
		{
			set
			{
				m_ClientHandle = value;
			}
		}

		public uint EventId
		{
			set
			{
				m_EventId = value;
			}
		}

		public LogEventParamPair[] Params
		{
			set
			{
				Helper.TryMarshalSet<LogEventParamPairInternal, LogEventParamPair>(ref m_Params, value, out m_ParamsCount);
			}
		}

		public void Set(LogEventOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				ClientHandle = other.ClientHandle;
				EventId = other.EventId;
				Params = other.Params;
			}
		}

		public void Set(object other)
		{
			Set(other as LogEventOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ClientHandle);
			Helper.TryMarshalDispose(ref m_Params);
		}
	}
}
