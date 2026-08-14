using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogPlayerTickOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_PlayerHandle;

		private IntPtr m_PlayerPosition;

		private IntPtr m_PlayerViewRotation;

		private int m_IsPlayerViewZoomed;

		private float m_PlayerHealth;

		private AntiCheatCommonPlayerMovementState m_PlayerMovementState;

		public IntPtr PlayerHandle
		{
			set
			{
				m_PlayerHandle = value;
			}
		}

		public Vec3f PlayerPosition
		{
			set
			{
				Helper.TryMarshalSet<Vec3fInternal, Vec3f>(ref m_PlayerPosition, value);
			}
		}

		public Quat PlayerViewRotation
		{
			set
			{
				Helper.TryMarshalSet<QuatInternal, Quat>(ref m_PlayerViewRotation, value);
			}
		}

		public bool IsPlayerViewZoomed
		{
			set
			{
				Helper.TryMarshalSet(ref m_IsPlayerViewZoomed, value);
			}
		}

		public float PlayerHealth
		{
			set
			{
				m_PlayerHealth = value;
			}
		}

		public AntiCheatCommonPlayerMovementState PlayerMovementState
		{
			set
			{
				m_PlayerMovementState = value;
			}
		}

		public void Set(LogPlayerTickOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				PlayerHandle = other.PlayerHandle;
				PlayerPosition = other.PlayerPosition;
				PlayerViewRotation = other.PlayerViewRotation;
				IsPlayerViewZoomed = other.IsPlayerViewZoomed;
				PlayerHealth = other.PlayerHealth;
				PlayerMovementState = other.PlayerMovementState;
			}
		}

		public void Set(object other)
		{
			Set(other as LogPlayerTickOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_PlayerHandle);
			Helper.TryMarshalDispose(ref m_PlayerPosition);
			Helper.TryMarshalDispose(ref m_PlayerViewRotation);
		}
	}
}
