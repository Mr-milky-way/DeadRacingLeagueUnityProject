using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Reports
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SendPlayerBehaviorReportCompleteCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;

		private IntPtr m_ClientData;

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
	}
}
