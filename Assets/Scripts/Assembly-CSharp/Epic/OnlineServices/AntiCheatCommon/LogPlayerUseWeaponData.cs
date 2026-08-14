using System;

namespace Epic.OnlineServices.AntiCheatCommon
{
	public class LogPlayerUseWeaponData : ISettable
	{
		public IntPtr PlayerHandle { get; set; }

		public Vec3f PlayerPosition { get; set; }

		public Quat PlayerViewRotation { get; set; }

		public bool IsPlayerViewZoomed { get; set; }

		public bool IsMeleeAttack { get; set; }

		public string WeaponName { get; set; }

		internal void Set(LogPlayerUseWeaponDataInternal? other)
		{
			if (other.HasValue)
			{
				PlayerHandle = other.Value.PlayerHandle;
				PlayerPosition = other.Value.PlayerPosition;
				PlayerViewRotation = other.Value.PlayerViewRotation;
				IsPlayerViewZoomed = other.Value.IsPlayerViewZoomed;
				IsMeleeAttack = other.Value.IsMeleeAttack;
				WeaponName = other.Value.WeaponName;
			}
		}

		public void Set(object other)
		{
			Set(other as LogPlayerUseWeaponDataInternal?);
		}
	}
}
