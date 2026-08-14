using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.TitleStorage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct FileMetadataInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_FileSizeBytes;

		private IntPtr m_MD5Hash;

		private IntPtr m_Filename;

		private uint m_UnencryptedDataSizeBytes;

		public uint FileSizeBytes
		{
			get
			{
				return m_FileSizeBytes;
			}
			set
			{
				m_FileSizeBytes = value;
			}
		}

		public string MD5Hash
		{
			get
			{
				Helper.TryMarshalGet(m_MD5Hash, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_MD5Hash, value);
			}
		}

		public string Filename
		{
			get
			{
				Helper.TryMarshalGet(m_Filename, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Filename, value);
			}
		}

		public uint UnencryptedDataSizeBytes
		{
			get
			{
				return m_UnencryptedDataSizeBytes;
			}
			set
			{
				m_UnencryptedDataSizeBytes = value;
			}
		}

		public void Set(FileMetadata other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				FileSizeBytes = other.FileSizeBytes;
				MD5Hash = other.MD5Hash;
				Filename = other.Filename;
				UnencryptedDataSizeBytes = other.UnencryptedDataSizeBytes;
			}
		}

		public void Set(object other)
		{
			Set(other as FileMetadata);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_MD5Hash);
			Helper.TryMarshalDispose(ref m_Filename);
		}
	}
}
