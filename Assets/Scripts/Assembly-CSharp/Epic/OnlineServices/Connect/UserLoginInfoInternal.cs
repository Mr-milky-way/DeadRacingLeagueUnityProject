using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UserLoginInfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_DisplayName;

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

		public void Set(UserLoginInfo other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				DisplayName = other.DisplayName;
			}
		}

		public void Set(object other)
		{
			Set(other as UserLoginInfo);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_DisplayName);
		}
	}
}
