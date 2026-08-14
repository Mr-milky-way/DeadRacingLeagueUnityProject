namespace Epic.OnlineServices.KWS
{
	public class QueryAgeGateCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public string CountryCode { get; private set; }

		public uint AgeOfConsent { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(QueryAgeGateCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				CountryCode = other.Value.CountryCode;
				AgeOfConsent = other.Value.AgeOfConsent;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryAgeGateCallbackInfoInternal?);
		}
	}
}
