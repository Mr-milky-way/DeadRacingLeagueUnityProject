using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct RegisterEventOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_EventId;

		private IntPtr m_EventName;

		private AntiCheatCommonEventType m_EventType;

		private uint m_ParamDefsCount;

		private IntPtr m_ParamDefs;

		public uint EventId
		{
			set
			{
				m_EventId = value;
			}
		}

		public string EventName
		{
			set
			{
				Helper.TryMarshalSet(ref m_EventName, value);
			}
		}

		public AntiCheatCommonEventType EventType
		{
			set
			{
				m_EventType = value;
			}
		}

		public RegisterEventParamDef[] ParamDefs
		{
			set
			{
				Helper.TryMarshalSet<RegisterEventParamDefInternal, RegisterEventParamDef>(ref m_ParamDefs, value, out m_ParamDefsCount);
			}
		}

		public void Set(RegisterEventOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				EventId = other.EventId;
				EventName = other.EventName;
				EventType = other.EventType;
				ParamDefs = other.ParamDefs;
			}
		}

		public void Set(object other)
		{
			Set(other as RegisterEventOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_EventName);
			Helper.TryMarshalDispose(ref m_ParamDefs);
		}
	}
}
