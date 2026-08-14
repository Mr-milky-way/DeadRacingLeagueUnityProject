namespace Epic.OnlineServices.Mods
{
	public class EnumerateModsCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public object ClientData { get; private set; }

		public ModEnumerationType Type { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(EnumerateModsCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				LocalUserId = other.Value.LocalUserId;
				ClientData = other.Value.ClientData;
				Type = other.Value.Type;
			}
		}

		public void Set(object other)
		{
			Set(other as EnumerateModsCallbackInfoInternal?);
		}
	}
}
