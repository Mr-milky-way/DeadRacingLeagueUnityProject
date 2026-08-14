using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.TitleStorage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryFileListCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;

		private IntPtr m_ClientData;

		private IntPtr m_LocalUserId;

		private uint m_FileCount;

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

		public uint FileCount => m_FileCount;
	}
}
