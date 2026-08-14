using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.KWS
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryAgeGateCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;

		private IntPtr m_ClientData;

		private IntPtr m_CountryCode;

		private uint m_AgeOfConsent;

		public Result ResultCode => m_ResultCode;

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public string CountryCode
		{
			get
			{
				Helper.TryMarshalGet(m_CountryCode, out string target);
				return target;
			}
		}

		public uint AgeOfConsent => m_AgeOfConsent;
	}
}
