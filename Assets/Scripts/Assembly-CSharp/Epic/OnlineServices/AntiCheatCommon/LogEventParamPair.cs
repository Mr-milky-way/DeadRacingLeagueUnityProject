namespace Epic.OnlineServices.AntiCheatCommon
{
	public class LogEventParamPair : ISettable
	{
		public LogEventParamPairParamValue ParamValue { get; set; }

		internal void Set(LogEventParamPairInternal? other)
		{
			if (other.HasValue)
			{
				ParamValue = other.Value.ParamValue;
			}
		}

		public void Set(object other)
		{
			Set(other as LogEventParamPairInternal?);
		}
	}
}
