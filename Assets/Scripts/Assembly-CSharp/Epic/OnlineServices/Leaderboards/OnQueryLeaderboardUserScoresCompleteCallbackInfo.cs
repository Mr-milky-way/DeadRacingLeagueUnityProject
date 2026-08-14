namespace Epic.OnlineServices.Leaderboards
{
	public class OnQueryLeaderboardUserScoresCompleteCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(OnQueryLeaderboardUserScoresCompleteCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
			}
		}

		public void Set(object other)
		{
			Set(other as OnQueryLeaderboardUserScoresCompleteCallbackInfoInternal?);
		}
	}
}
