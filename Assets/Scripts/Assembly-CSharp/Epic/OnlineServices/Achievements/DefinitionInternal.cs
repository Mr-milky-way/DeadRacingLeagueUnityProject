using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct DefinitionInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_AchievementId;

		private IntPtr m_DisplayName;

		private IntPtr m_Description;

		private IntPtr m_LockedDisplayName;

		private IntPtr m_LockedDescription;

		private IntPtr m_HiddenDescription;

		private IntPtr m_CompletionDescription;

		private IntPtr m_UnlockedIconId;

		private IntPtr m_LockedIconId;

		private int m_IsHidden;

		private int m_StatThresholdsCount;

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

		public string DisplayName
		{
			get
			{
				Helper.TryMarshalGet(m_DisplayName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_DisplayName, value);
			}
		}

		public string Description
		{
			get
			{
				Helper.TryMarshalGet(m_Description, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Description, value);
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

		public string HiddenDescription
		{
			get
			{
				Helper.TryMarshalGet(m_HiddenDescription, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_HiddenDescription, value);
			}
		}

		public string CompletionDescription
		{
			get
			{
				Helper.TryMarshalGet(m_CompletionDescription, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_CompletionDescription, value);
			}
		}

		public string UnlockedIconId
		{
			get
			{
				Helper.TryMarshalGet(m_UnlockedIconId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UnlockedIconId, value);
			}
		}

		public string LockedIconId
		{
			get
			{
				Helper.TryMarshalGet(m_LockedIconId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_LockedIconId, value);
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

		public void Set(Definition other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				AchievementId = other.AchievementId;
				DisplayName = other.DisplayName;
				Description = other.Description;
				LockedDisplayName = other.LockedDisplayName;
				LockedDescription = other.LockedDescription;
				HiddenDescription = other.HiddenDescription;
				CompletionDescription = other.CompletionDescription;
				UnlockedIconId = other.UnlockedIconId;
				LockedIconId = other.LockedIconId;
				IsHidden = other.IsHidden;
				StatThresholds = other.StatThresholds;
			}
		}

		public void Set(object other)
		{
			Set(other as Definition);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AchievementId);
			Helper.TryMarshalDispose(ref m_DisplayName);
			Helper.TryMarshalDispose(ref m_Description);
			Helper.TryMarshalDispose(ref m_LockedDisplayName);
			Helper.TryMarshalDispose(ref m_LockedDescription);
			Helper.TryMarshalDispose(ref m_HiddenDescription);
			Helper.TryMarshalDispose(ref m_CompletionDescription);
			Helper.TryMarshalDispose(ref m_UnlockedIconId);
			Helper.TryMarshalDispose(ref m_LockedIconId);
			Helper.TryMarshalDispose(ref m_StatThresholds);
		}
	}
}
