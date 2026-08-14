using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ReadFileOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_Filename;

		private uint m_ReadChunkLengthBytes;

		private IntPtr m_ReadFileDataCallback;

		private IntPtr m_FileTransferProgressCallback;

		private static OnReadFileDataCallbackInternal s_ReadFileDataCallback;

		private static OnFileTransferProgressCallbackInternal s_FileTransferProgressCallback;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string Filename
		{
			set
			{
				Helper.TryMarshalSet(ref m_Filename, value);
			}
		}

		public uint ReadChunkLengthBytes
		{
			set
			{
				m_ReadChunkLengthBytes = value;
			}
		}

		public static OnReadFileDataCallbackInternal ReadFileDataCallback
		{
			get
			{
				if (s_ReadFileDataCallback == null)
				{
					s_ReadFileDataCallback = PlayerDataStorageInterface.OnReadFileDataCallbackInternalImplementation;
				}
				return s_ReadFileDataCallback;
			}
		}

		public static OnFileTransferProgressCallbackInternal FileTransferProgressCallback
		{
			get
			{
				if (s_FileTransferProgressCallback == null)
				{
					s_FileTransferProgressCallback = PlayerDataStorageInterface.OnFileTransferProgressCallbackInternalImplementation;
				}
				return s_FileTransferProgressCallback;
			}
		}

		public void Set(ReadFileOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				Filename = other.Filename;
				ReadChunkLengthBytes = other.ReadChunkLengthBytes;
				m_ReadFileDataCallback = ((other.ReadFileDataCallback != null) ? Marshal.GetFunctionPointerForDelegate(ReadFileDataCallback) : IntPtr.Zero);
				m_FileTransferProgressCallback = ((other.FileTransferProgressCallback != null) ? Marshal.GetFunctionPointerForDelegate(FileTransferProgressCallback) : IntPtr.Zero);
			}
		}

		public void Set(object other)
		{
			Set(other as ReadFileOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_Filename);
			Helper.TryMarshalDispose(ref m_ReadFileDataCallback);
			Helper.TryMarshalDispose(ref m_FileTransferProgressCallback);
		}
	}
}
