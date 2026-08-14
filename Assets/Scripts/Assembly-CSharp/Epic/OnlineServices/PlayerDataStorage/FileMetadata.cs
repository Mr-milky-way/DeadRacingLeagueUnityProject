using System;

namespace Epic.OnlineServices.PlayerDataStorage
{
	public class FileMetadata : ISettable
	{
		public uint FileSizeBytes { get; set; }

		public string MD5Hash { get; set; }

		public string Filename { get; set; }

		public DateTimeOffset? LastModifiedTime { get; set; }

		public uint UnencryptedDataSizeBytes { get; set; }

		internal void Set(FileMetadataInternal? other)
		{
			if (other.HasValue)
			{
				FileSizeBytes = other.Value.FileSizeBytes;
				MD5Hash = other.Value.MD5Hash;
				Filename = other.Value.Filename;
				LastModifiedTime = other.Value.LastModifiedTime;
				UnencryptedDataSizeBytes = other.Value.UnencryptedDataSizeBytes;
			}
		}

		public void Set(object other)
		{
			Set(other as FileMetadataInternal?);
		}
	}
}
