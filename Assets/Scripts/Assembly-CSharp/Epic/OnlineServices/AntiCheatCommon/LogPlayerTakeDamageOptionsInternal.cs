using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogPlayerTakeDamageOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_VictimPlayerHandle;

		private IntPtr m_VictimPlayerPosition;

		private IntPtr m_VictimPlayerViewRotation;

		private IntPtr m_AttackerPlayerHandle;

		private IntPtr m_AttackerPlayerPosition;

		private IntPtr m_AttackerPlayerViewRotation;

		private int m_IsHitscanAttack;

		private int m_HasLineOfSight;

		private int m_IsCriticalHit;

		private uint m_HitBoneId;

		private float m_DamageTaken;

		private float m_HealthRemaining;

		private AntiCheatCommonPlayerTakeDamageSource m_DamageSource;

		private AntiCheatCommonPlayerTakeDamageType m_DamageType;

		private AntiCheatCommonPlayerTakeDamageResult m_DamageResult;

		private IntPtr m_PlayerUseWeaponData;

		private uint m_TimeSincePlayerUseWeaponMs;

		public IntPtr VictimPlayerHandle
		{
			set
			{
				m_VictimPlayerHandle = value;
			}
		}

		public Vec3f VictimPlayerPosition
		{
			set
			{
				Helper.TryMarshalSet<Vec3fInternal, Vec3f>(ref m_VictimPlayerPosition, value);
			}
		}

		public Quat VictimPlayerViewRotation
		{
			set
			{
				Helper.TryMarshalSet<QuatInternal, Quat>(ref m_VictimPlayerViewRotation, value);
			}
		}

		public IntPtr AttackerPlayerHandle
		{
			set
			{
				m_AttackerPlayerHandle = value;
			}
		}

		public Vec3f AttackerPlayerPosition
		{
			set
			{
				Helper.TryMarshalSet<Vec3fInternal, Vec3f>(ref m_AttackerPlayerPosition, value);
			}
		}

		public Quat AttackerPlayerViewRotation
		{
			set
			{
				Helper.TryMarshalSet<QuatInternal, Quat>(ref m_AttackerPlayerViewRotation, value);
			}
		}

		public bool IsHitscanAttack
		{
			set
			{
				Helper.TryMarshalSet(ref m_IsHitscanAttack, value);
			}
		}

		public bool HasLineOfSight
		{
			set
			{
				Helper.TryMarshalSet(ref m_HasLineOfSight, value);
			}
		}

		public bool IsCriticalHit
		{
			set
			{
				Helper.TryMarshalSet(ref m_IsCriticalHit, value);
			}
		}

		public uint HitBoneId
		{
			set
			{
				m_HitBoneId = value;
			}
		}

		public float DamageTaken
		{
			set
			{
				m_DamageTaken = value;
			}
		}

		public float HealthRemaining
		{
			set
			{
				m_HealthRemaining = value;
			}
		}

		public AntiCheatCommonPlayerTakeDamageSource DamageSource
		{
			set
			{
				m_DamageSource = value;
			}
		}

		public AntiCheatCommonPlayerTakeDamageType DamageType
		{
			set
			{
				m_DamageType = value;
			}
		}

		public AntiCheatCommonPlayerTakeDamageResult DamageResult
		{
			set
			{
				m_DamageResult = value;
			}
		}

		public LogPlayerUseWeaponData PlayerUseWeaponData
		{
			set
			{
				Helper.TryMarshalSet<LogPlayerUseWeaponDataInternal, LogPlayerUseWeaponData>(ref m_PlayerUseWeaponData, value);
			}
		}

		public uint TimeSincePlayerUseWeaponMs
		{
			set
			{
				m_TimeSincePlayerUseWeaponMs = value;
			}
		}

		public void Set(LogPlayerTakeDamageOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				VictimPlayerHandle = other.VictimPlayerHandle;
				VictimPlayerPosition = other.VictimPlayerPosition;
				VictimPlayerViewRotation = other.VictimPlayerViewRotation;
				AttackerPlayerHandle = other.AttackerPlayerHandle;
				AttackerPlayerPosition = other.AttackerPlayerPosition;
				AttackerPlayerViewRotation = other.AttackerPlayerViewRotation;
				IsHitscanAttack = other.IsHitscanAttack;
				HasLineOfSight = other.HasLineOfSight;
				IsCriticalHit = other.IsCriticalHit;
				HitBoneId = other.HitBoneId;
				DamageTaken = other.DamageTaken;
				HealthRemaining = other.HealthRemaining;
				DamageSource = other.DamageSource;
				DamageType = other.DamageType;
				DamageResult = other.DamageResult;
				PlayerUseWeaponData = other.PlayerUseWeaponData;
				TimeSincePlayerUseWeaponMs = other.TimeSincePlayerUseWeaponMs;
			}
		}

		public void Set(object other)
		{
			Set(other as LogPlayerTakeDamageOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_VictimPlayerHandle);
			Helper.TryMarshalDispose(ref m_VictimPlayerPosition);
			Helper.TryMarshalDispose(ref m_VictimPlayerViewRotation);
			Helper.TryMarshalDispose(ref m_AttackerPlayerHandle);
			Helper.TryMarshalDispose(ref m_AttackerPlayerPosition);
			Helper.TryMarshalDispose(ref m_AttackerPlayerViewRotation);
			Helper.TryMarshalDispose(ref m_PlayerUseWeaponData);
		}
	}
}
