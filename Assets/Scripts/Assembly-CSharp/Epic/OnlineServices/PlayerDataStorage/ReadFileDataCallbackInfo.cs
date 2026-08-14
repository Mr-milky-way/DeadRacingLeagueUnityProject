namespace Epic.OnlineServices.PlayerDataStorage
{
	public class ReadFileDataCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public string Filename { get; private set; }

		public uint TotalFileSizeBytes { get; private set; }

		public bool IsLastChunk { get; private set; }

		public byte[] DataChunk { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(ReadFileDataCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				Filename = other.Value.Filename;
				TotalFileSizeBytes = other.Value.TotalFileSizeBytes;
				IsLastChunk = other.Value.IsLastChunk;
				DataChunk = other.Value.DataChunk;
			}
		}

		public void Set(object other)
		{
			Set(other as ReadFileDataCallbackInfoInternal?);
		}
	}
}
