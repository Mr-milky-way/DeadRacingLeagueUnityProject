using System;

namespace Epic.OnlineServices.AntiCheatCommon
{
	public class SetClientDetailsOptions
	{
		public IntPtr ClientHandle { get; set; }

		public AntiCheatCommonClientFlags ClientFlags { get; set; }

		public AntiCheatCommonClientInput ClientInputMethod { get; set; }
	}
}
