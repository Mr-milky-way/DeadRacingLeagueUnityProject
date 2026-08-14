namespace Epic.OnlineServices.P2P
{
	public class OnQueryNATTypeCompleteInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public NATType NATType { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(OnQueryNATTypeCompleteInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				NATType = other.Value.NATType;
			}
		}

		public void Set(object other)
		{
			Set(other as OnQueryNATTypeCompleteInfoInternal?);
		}
	}
}
