namespace Epic.OnlineServices.PlayerDataStorage
{
	public class WriteFileDataCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public string Filename { get; private set; }

		public uint DataBufferLengthBytes { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(WriteFileDataCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				Filename = other.Value.Filename;
				DataBufferLengthBytes = other.Value.DataBufferLengthBytes;
			}
		}

		public void Set(object other)
		{
			Set(other as WriteFileDataCallbackInfoInternal?);
		}
	}
}
