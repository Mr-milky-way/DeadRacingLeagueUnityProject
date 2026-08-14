namespace Epic.OnlineServices.Mods
{
	public class UpdateModCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public object ClientData { get; private set; }

		public ModIdentifier Mod { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(UpdateModCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				LocalUserId = other.Value.LocalUserId;
				ClientData = other.Value.ClientData;
				Mod = other.Value.Mod;
			}
		}

		public void Set(object other)
		{
			Set(other as UpdateModCallbackInfoInternal?);
		}
	}
}
