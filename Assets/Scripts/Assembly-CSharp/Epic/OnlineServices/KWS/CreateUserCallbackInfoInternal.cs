using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.KWS
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CreateUserCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;

		private IntPtr m_ClientData;

		private IntPtr m_LocalUserId;

		private IntPtr m_KWSUserId;

		private int m_IsMinor;

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

		public ProductUserId LocalUserId
		{
			get
			{
				Helper.TryMarshalGet(m_LocalUserId, out ProductUserId target);
				return target;
			}
		}

		public string KWSUserId
		{
			get
			{
				Helper.TryMarshalGet(m_KWSUserId, out string target);
				return target;
			}
		}

		public bool IsMinor
		{
			get
			{
				Helper.TryMarshalGet(m_IsMinor, out var target);
				return target;
			}
		}
	}
}
