using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyFileMetadataByFilenameOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_Filename;

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

		public void Set(CopyFileMetadataByFilenameOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				Filename = other.Filename;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyFileMetadataByFilenameOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_Filename);
		}
	}
}
