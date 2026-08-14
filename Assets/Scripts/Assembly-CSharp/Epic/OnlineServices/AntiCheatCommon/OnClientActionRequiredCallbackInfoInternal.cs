using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct OnClientActionRequiredCallbackInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_ClientHandle;

		private AntiCheatCommonClientAction m_ClientAction;

		private AntiCheatCommonClientActionReason m_ActionReasonCode;

		private IntPtr m_ActionReasonDetailsString;

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public IntPtr ClientHandle => m_ClientHandle;

		public AntiCheatCommonClientAction ClientAction => m_ClientAction;

		public AntiCheatCommonClientActionReason ActionReasonCode => m_ActionReasonCode;

		public string ActionReasonDetailsString
		{
			get
			{
				Helper.TryMarshalGet(m_ActionReasonDetailsString, out string target);
				return target;
			}
		}
	}
}
