using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogPlayerUseAbilityOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_PlayerHandle;

		private uint m_AbilityId;

		private uint m_AbilityDurationMs;

		private uint m_AbilityCooldownMs;

		public IntPtr PlayerHandle
		{
			set
			{
				m_PlayerHandle = value;
			}
		}

		public uint AbilityId
		{
			set
			{
				m_AbilityId = value;
			}
		}

		public uint AbilityDurationMs
		{
			set
			{
				m_AbilityDurationMs = value;
			}
		}

		public uint AbilityCooldownMs
		{
			set
			{
				m_AbilityCooldownMs = value;
			}
		}

		public void Set(LogPlayerUseAbilityOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				PlayerHandle = other.PlayerHandle;
				AbilityId = other.AbilityId;
				AbilityDurationMs = other.AbilityDurationMs;
				AbilityCooldownMs = other.AbilityCooldownMs;
			}
		}

		public void Set(object other)
		{
			Set(other as LogPlayerUseAbilityOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_PlayerHandle);
		}
	}
}
