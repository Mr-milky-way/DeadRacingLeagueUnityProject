using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UserInfo
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UserInfoDataInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_UserId;

		private IntPtr m_Country;

		private IntPtr m_DisplayName;

		private IntPtr m_PreferredLanguage;

		private IntPtr m_Nickname;

		public EpicAccountId UserId
		{
			get
			{
				Helper.TryMarshalGet(m_UserId, out EpicAccountId target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UserId, value);
			}
		}

		public string Country
		{
			get
			{
				Helper.TryMarshalGet(m_Country, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Country, value);
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

		public string PreferredLanguage
		{
			get
			{
				Helper.TryMarshalGet(m_PreferredLanguage, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_PreferredLanguage, value);
			}
		}

		public string Nickname
		{
			get
			{
				Helper.TryMarshalGet(m_Nickname, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Nickname, value);
			}
		}

		public void Set(UserInfoData other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				UserId = other.UserId;
				Country = other.Country;
				DisplayName = other.DisplayName;
				PreferredLanguage = other.PreferredLanguage;
				Nickname = other.Nickname;
			}
		}

		public void Set(object other)
		{
			Set(other as UserInfoData);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserId);
			Helper.TryMarshalDispose(ref m_Country);
			Helper.TryMarshalDispose(ref m_DisplayName);
			Helper.TryMarshalDispose(ref m_PreferredLanguage);
			Helper.TryMarshalDispose(ref m_Nickname);
		}
	}
}
