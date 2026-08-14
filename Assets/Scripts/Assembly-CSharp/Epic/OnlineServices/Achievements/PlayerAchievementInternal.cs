using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PlayerAchievementInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_AchievementId;

		private double m_Progress;

		private long m_UnlockTime;

		private int m_StatInfoCount;

		private IntPtr m_StatInfo;

		private IntPtr m_DisplayName;

		private IntPtr m_Description;

		private IntPtr m_IconURL;

		private IntPtr m_FlavorText;

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

		public double Progress
		{
			get
			{
				return m_Progress;
			}
			set
			{
				m_Progress = value;
			}
		}

		public DateTimeOffset? UnlockTime
		{
			get
			{
				Helper.TryMarshalGet(m_UnlockTime, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UnlockTime, value);
			}
		}

		public PlayerStatInfo[] StatInfo
		{
			get
			{
				Helper.TryMarshalGet<PlayerStatInfoInternal, PlayerStatInfo>(m_StatInfo, out var target, m_StatInfoCount);
				return target;
			}
			set
			{
				Helper.TryMarshalSet<PlayerStatInfoInternal, PlayerStatInfo>(ref m_StatInfo, value, out m_StatInfoCount);
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

		public string IconURL
		{
			get
			{
				Helper.TryMarshalGet(m_IconURL, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_IconURL, value);
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

		public void Set(PlayerAchievement other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				AchievementId = other.AchievementId;
				Progress = other.Progress;
				UnlockTime = other.UnlockTime;
				StatInfo = other.StatInfo;
				DisplayName = other.DisplayName;
				Description = other.Description;
				IconURL = other.IconURL;
				FlavorText = other.FlavorText;
			}
		}

		public void Set(object other)
		{
			Set(other as PlayerAchievement);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AchievementId);
			Helper.TryMarshalDispose(ref m_StatInfo);
			Helper.TryMarshalDispose(ref m_DisplayName);
			Helper.TryMarshalDispose(ref m_Description);
			Helper.TryMarshalDispose(ref m_IconURL);
			Helper.TryMarshalDispose(ref m_FlavorText);
		}
	}
}
