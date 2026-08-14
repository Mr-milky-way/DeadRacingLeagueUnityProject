namespace Epic.OnlineServices.AntiCheatCommon
{
	public class RegisterEventParamDef : ISettable
	{
		public string ParamName { get; set; }

		public AntiCheatCommonEventParamType ParamType { get; set; }

		internal void Set(RegisterEventParamDefInternal? other)
		{
			if (other.HasValue)
			{
				ParamName = other.Value.ParamName;
				ParamType = other.Value.ParamType;
			}
		}

		public void Set(object other)
		{
			Set(other as RegisterEventParamDefInternal?);
		}
	}
}
