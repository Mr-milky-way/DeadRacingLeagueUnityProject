using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LinkAccountCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;

		private IntPtr m_ClientData;

		private IntPtr m_LocalUserId;

		private IntPtr m_PinGrantInfo;

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

		public EpicAccountId LocalUserId
		{
			get
			{
				Helper.TryMarshalGet(m_LocalUserId, out EpicAccountId target);
				return target;
			}
		}

		public PinGrantInfo PinGrantInfo
		{
			get
			{
				Helper.TryMarshalGet<PinGrantInfoInternal, PinGrantInfo>(m_PinGrantInfo, out var target);
				return target;
			}
		}
	}
}
