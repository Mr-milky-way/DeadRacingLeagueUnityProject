using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UpdateSessionCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;

		private IntPtr m_ClientData;

		private IntPtr m_SessionName;

		private IntPtr m_SessionId;

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

		public string SessionName
		{
			get
			{
				Helper.TryMarshalGet(m_SessionName, out string target);
				return target;
			}
		}

		public string SessionId
		{
			get
			{
				Helper.TryMarshalGet(m_SessionId, out string target);
				return target;
			}
		}
	}
}
