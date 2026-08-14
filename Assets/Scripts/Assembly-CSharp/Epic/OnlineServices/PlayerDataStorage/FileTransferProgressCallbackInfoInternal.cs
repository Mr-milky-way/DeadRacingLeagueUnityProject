using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct FileTransferProgressCallbackInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_LocalUserId;

		private IntPtr m_Filename;

		private uint m_BytesTransferred;

		private uint m_TotalFileSizeBytes;

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

		public string Filename
		{
			get
			{
				Helper.TryMarshalGet(m_Filename, out string target);
				return target;
			}
		}

		public uint BytesTransferred => m_BytesTransferred;

		public uint TotalFileSizeBytes => m_TotalFileSizeBytes;
	}
}
