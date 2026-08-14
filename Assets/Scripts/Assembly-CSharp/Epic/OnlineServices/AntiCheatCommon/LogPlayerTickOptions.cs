using System;

namespace Epic.OnlineServices.AntiCheatCommon
{
	public class LogPlayerTickOptions
	{
		public IntPtr PlayerHandle { get; set; }

		public Vec3f PlayerPosition { get; set; }

		public Quat PlayerViewRotation { get; set; }

		public bool IsPlayerViewZoomed { get; set; }

		public float PlayerHealth { get; set; }

		public AntiCheatCommonPlayerMovementState PlayerMovementState { get; set; }
	}
}
