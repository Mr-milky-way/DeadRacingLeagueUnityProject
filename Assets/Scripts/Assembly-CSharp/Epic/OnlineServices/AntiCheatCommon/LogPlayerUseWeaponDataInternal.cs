using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogPlayerUseWeaponDataInternal : ISettable, IDisposable
	{
		private IntPtr m_PlayerHandle;

		private IntPtr m_PlayerPosition;

		private IntPtr m_PlayerViewRotation;

		private int m_IsPlayerViewZoomed;

		private int m_IsMeleeAttack;

		private IntPtr m_WeaponName;

		public IntPtr PlayerHandle
		{
			get
			{
				return m_PlayerHandle;
			}
			set
			{
				m_PlayerHandle = value;
			}
		}

		public Vec3f PlayerPosition
		{
			get
			{
				Helper.TryMarshalGet<Vec3fInternal, Vec3f>(m_PlayerPosition, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet<Vec3fInternal, Vec3f>(ref m_PlayerPosition, value);
			}
		}

		public Quat PlayerViewRotation
		{
			get
			{
				Helper.TryMarshalGet<QuatInternal, Quat>(m_PlayerViewRotation, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet<QuatInternal, Quat>(ref m_PlayerViewRotation, value);
			}
		}

		public bool IsPlayerViewZoomed
		{
			get
			{
				Helper.TryMarshalGet(m_IsPlayerViewZoomed, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_IsPlayerViewZoomed, value);
			}
		}

		public bool IsMeleeAttack
		{
			get
			{
				Helper.TryMarshalGet(m_IsMeleeAttack, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_IsMeleeAttack, value);
			}
		}

		public string WeaponName
		{
			get
			{
				Helper.TryMarshalGet(m_WeaponName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_WeaponName, value);
			}
		}

		public void Set(LogPlayerUseWeaponData other)
		{
			if (other != null)
			{
				PlayerHandle = other.PlayerHandle;
				PlayerPosition = other.PlayerPosition;
				PlayerViewRotation = other.PlayerViewRotation;
				IsPlayerViewZoomed = other.IsPlayerViewZoomed;
				IsMeleeAttack = other.IsMeleeAttack;
				WeaponName = other.WeaponName;
			}
		}

		public void Set(object other)
		{
			Set(other as LogPlayerUseWeaponData);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_PlayerHandle);
			Helper.TryMarshalDispose(ref m_PlayerPosition);
			Helper.TryMarshalDispose(ref m_PlayerViewRotation);
			Helper.TryMarshalDispose(ref m_WeaponName);
		}
	}
}
