namespace Epic.OnlineServices.PlayerDataStorage
{
	public class WriteFileOptions
	{
		public ProductUserId LocalUserId { get; set; }

		public string Filename { get; set; }

		public uint ChunkLengthBytes { get; set; }

		public OnWriteFileDataCallback WriteFileDataCallback { get; set; }

		public OnFileTransferProgressCallback FileTransferProgressCallback { get; set; }
	}
}
