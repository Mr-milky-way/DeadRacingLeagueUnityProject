using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct OnClientAuthStatusChangedCallbackInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_ClientHandle;

		private AntiCheatCommonClientAuthStatus m_ClientAuthStatus;

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

		public AntiCheatCommonClientAuthStatus ClientAuthStatus => m_ClientAuthStatus;
	}
}
