using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.PlayerDataStorage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct DuplicateFileOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_SourceFilename;

		private IntPtr m_DestinationFilename;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string SourceFilename
		{
			set
			{
				Helper.TryMarshalSet(ref m_SourceFilename, value);
			}
		}

		public string DestinationFilename
		{
			set
			{
				Helper.TryMarshalSet(ref m_DestinationFilename, value);
			}
		}

		public void Set(DuplicateFileOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				SourceFilename = other.SourceFilename;
				DestinationFilename = other.DestinationFilename;
			}
		}

		public void Set(object other)
		{
			Set(other as DuplicateFileOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_SourceFilename);
			Helper.TryMarshalDispose(ref m_DestinationFilename);
		}
	}
}
