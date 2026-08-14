using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sanctions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryActivePlayerSanctionsCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;

		private IntPtr m_ClientData;

		private IntPtr m_TargetUserId;

		private IntPtr m_LocalUserId;

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

		public ProductUserId TargetUserId
		{
			get
			{
				Helper.TryMarshalGet(m_TargetUserId, out ProductUserId target);
				return target;
			}
		}

		public ProductUserId LocalUserId
		{
			get
			{
				Helper.TryMarshalGet(m_LocalUserId, out ProductUserId target);
				return target;
			}
		}
	}
}
