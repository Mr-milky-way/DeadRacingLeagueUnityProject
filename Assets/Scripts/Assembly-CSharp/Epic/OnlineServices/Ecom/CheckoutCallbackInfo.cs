namespace Epic.OnlineServices.Ecom
{
	public class CheckoutCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public string TransactionId { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(CheckoutCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				TransactionId = other.Value.TransactionId;
			}
		}

		public void Set(object other)
		{
			Set(other as CheckoutCallbackInfoInternal?);
		}
	}
}
