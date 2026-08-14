using System;

namespace Epic.OnlineServices.AntiCheatCommon
{
	public class LogEventOptions
	{
		public IntPtr ClientHandle { get; set; }

		public uint EventId { get; set; }

		public LogEventParamPair[] Params { get; set; }
	}
}
