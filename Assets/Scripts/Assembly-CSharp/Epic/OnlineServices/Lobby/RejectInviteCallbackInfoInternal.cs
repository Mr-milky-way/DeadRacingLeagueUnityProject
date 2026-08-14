using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct RejectInviteCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;

		private IntPtr m_ClientData;

		private IntPtr m_InviteId;

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

		public string InviteId
		{
			get
			{
				Helper.TryMarshalGet(m_InviteId, out string target);
				return target;
			}
		}
	}
}
