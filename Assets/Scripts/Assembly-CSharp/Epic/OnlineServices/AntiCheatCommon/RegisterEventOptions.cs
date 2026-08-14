namespace Epic.OnlineServices.AntiCheatCommon
{
	public class RegisterEventOptions
	{
		public uint EventId { get; set; }

		public string EventName { get; set; }

		public AntiCheatCommonEventType EventType { get; set; }

		public RegisterEventParamDef[] ParamDefs { get; set; }
	}
}
