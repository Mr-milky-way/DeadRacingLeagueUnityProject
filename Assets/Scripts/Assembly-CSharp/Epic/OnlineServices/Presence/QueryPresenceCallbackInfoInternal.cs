using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryPresenceCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;

		private IntPtr m_ClientData;

		private IntPtr m_LocalUserId;

		private IntPtr m_TargetUserId;

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

		public EpicAccountId TargetUserId
		{
			get
			{
				Helper.TryMarshalGet(m_TargetUserId, out EpicAccountId target);
				return target;
			}
		}
	}
}
