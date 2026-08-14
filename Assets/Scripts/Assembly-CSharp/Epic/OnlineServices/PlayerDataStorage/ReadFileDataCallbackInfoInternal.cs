using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ReadFileDataCallbackInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_LocalUserId;

		private IntPtr m_Filename;

		private uint m_TotalFileSizeBytes;

		private int m_IsLastChunk;

		private uint m_DataChunkLengthBytes;

		private IntPtr m_DataChunk;

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

		public uint TotalFileSizeBytes => m_TotalFileSizeBytes;

		public bool IsLastChunk
		{
			get
			{
				Helper.TryMarshalGet(m_IsLastChunk, out var target);
				return target;
			}
		}

		public byte[] DataChunk
		{
			get
			{
				Helper.TryMarshalGet(m_DataChunk, out byte[] target, m_DataChunkLengthBytes);
				return target;
			}
		}
	}
}
