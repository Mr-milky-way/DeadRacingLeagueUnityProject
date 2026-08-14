using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionModificationRemoveAttributeOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Key;

		public string Key
		{
			set
			{
				Helper.TryMarshalSet(ref m_Key, value);
			}
		}

		public void Set(SessionModificationRemoveAttributeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Key = other.Key;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionModificationRemoveAttributeOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Key);
		}
	}
}
