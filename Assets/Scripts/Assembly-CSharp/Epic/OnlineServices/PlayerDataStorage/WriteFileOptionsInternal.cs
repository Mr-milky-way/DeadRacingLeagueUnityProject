using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct WriteFileOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_Filename;

		private uint m_ChunkLengthBytes;

		private IntPtr m_WriteFileDataCallback;

		private IntPtr m_FileTransferProgressCallback;

		private static OnWriteFileDataCallbackInternal s_WriteFileDataCallback;

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

		public uint ChunkLengthBytes
		{
			set
			{
				m_ChunkLengthBytes = value;
			}
		}

		public static OnWriteFileDataCallbackInternal WriteFileDataCallback
		{
			get
			{
				if (s_WriteFileDataCallback == null)
				{
					s_WriteFileDataCallback = PlayerDataStorageInterface.OnWriteFileDataCallbackInternalImplementation;
				}
				return s_WriteFileDataCallback;
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

		public void Set(WriteFileOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				Filename = other.Filename;
				ChunkLengthBytes = other.ChunkLengthBytes;
				m_WriteFileDataCallback = ((other.WriteFileDataCallback != null) ? Marshal.GetFunctionPointerForDelegate(WriteFileDataCallback) : IntPtr.Zero);
				m_FileTransferProgressCallback = ((other.FileTransferProgressCallback != null) ? Marshal.GetFunctionPointerForDelegate(FileTransferProgressCallback) : IntPtr.Zero);
			}
		}

		public void Set(object other)
		{
			Set(other as WriteFileOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_Filename);
			Helper.TryMarshalDispose(ref m_WriteFileDataCallback);
			Helper.TryMarshalDispose(ref m_FileTransferProgressCallback);
		}
	}
}
