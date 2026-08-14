using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Mods
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct EnumerateModsCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;

		private IntPtr m_LocalUserId;

		private IntPtr m_ClientData;

		private ModEnumerationType m_Type;

		public Result ResultCode => m_ResultCode;

		public EpicAccountId LocalUserId
		{
			get
			{
				Helper.TryMarshalGet(m_LocalUserId, out EpicAccountId target);
				return target;
			}
		}

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public ModEnumerationType Type => m_Type;
	}
}
