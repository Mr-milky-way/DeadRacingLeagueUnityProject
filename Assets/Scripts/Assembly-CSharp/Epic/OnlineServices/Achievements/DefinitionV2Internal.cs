using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct DefinitionV2Internal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_AchievementId;

		private IntPtr m_UnlockedDisplayName;

		private IntPtr m_UnlockedDescription;

		private IntPtr m_LockedDisplayName;

		private IntPtr m_LockedDescription;

		private IntPtr m_FlavorText;

		private IntPtr m_UnlockedIconURL;

		private IntPtr m_LockedIconURL;

		private int m_IsHidden;

		private uint m_StatThresholdsCount;

		private IntPtr m_StatThresholds;

		public string AchievementId
		{
			get
			{
				Helper.TryMarshalGet(m_AchievementId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AchievementId, value);
			}
		}

		public string UnlockedDisplayName
		{
			get
			{
				Helper.TryMarshalGet(m_UnlockedDisplayName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UnlockedDisplayName, value);
			}
		}

		public string UnlockedDescription
		{
			get
			{
				Helper.TryMarshalGet(m_UnlockedDescription, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UnlockedDescription, value);
			}
		}

		public string LockedDisplayName
		{
			get
			{
				Helper.TryMarshalGet(m_LockedDisplayName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_LockedDisplayName, value);
			}
		}

		public string LockedDescription
		{
			get
			{
				Helper.TryMarshalGet(m_LockedDescription, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_LockedDescription, value);
			}
		}

		public string FlavorText
		{
			get
			{
				Helper.TryMarshalGet(m_FlavorText, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_FlavorText, value);
			}
		}

		public string UnlockedIconURL
		{
			get
			{
				Helper.TryMarshalGet(m_UnlockedIconURL, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UnlockedIconURL, value);
			}
		}

		public string LockedIconURL
		{
			get
			{
				Helper.TryMarshalGet(m_LockedIconURL, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_LockedIconURL, value);
			}
		}

		public bool IsHidden
		{
			get
			{
				Helper.TryMarshalGet(m_IsHidden, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_IsHidden, value);
			}
		}

		public StatThresholds[] StatThresholds
		{
			get
			{
				Helper.TryMarshalGet<StatThresholdsInternal, StatThresholds>(m_StatThresholds, out var target, m_StatThresholdsCount);
				return target;
			}
			set
			{
				Helper.TryMarshalSet<StatThresholdsInternal, StatThresholds>(ref m_StatThresholds, value, out m_StatThresholdsCount);
			}
		}

		public void Set(DefinitionV2 other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				AchievementId = other.AchievementId;
				UnlockedDisplayName = other.UnlockedDisplayName;
				UnlockedDescription = other.UnlockedDescription;
				LockedDisplayName = other.LockedDisplayName;
				LockedDescription = other.LockedDescription;
				FlavorText = other.FlavorText;
				UnlockedIconURL = other.UnlockedIconURL;
				LockedIconURL = other.LockedIconURL;
				IsHidden = other.IsHidden;
				StatThresholds = other.StatThresholds;
			}
		}

		public void Set(object other)
		{
			Set(other as DefinitionV2);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AchievementId);
			Helper.TryMarshalDispose(ref m_UnlockedDisplayName);
			Helper.TryMarshalDispose(ref m_UnlockedDescription);
			Helper.TryMarshalDispose(ref m_LockedDisplayName);
			Helper.TryMarshalDispose(ref m_LockedDescription);
			Helper.TryMarshalDispose(ref m_FlavorText);
			Helper.TryMarshalDispose(ref m_UnlockedIconURL);
			Helper.TryMarshalDispose(ref m_LockedIconURL);
			Helper.TryMarshalDispose(ref m_StatThresholds);
		}
	}
}
