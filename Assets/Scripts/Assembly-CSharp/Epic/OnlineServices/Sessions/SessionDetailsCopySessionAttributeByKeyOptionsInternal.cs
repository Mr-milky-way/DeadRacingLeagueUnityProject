using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionDetailsCopySessionAttributeByKeyOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_AttrKey;

		public string AttrKey
		{
			set
			{
				Helper.TryMarshalSet(ref m_AttrKey, value);
			}
		}

		public void Set(SessionDetailsCopySessionAttributeByKeyOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				AttrKey = other.AttrKey;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionDetailsCopySessionAttributeByKeyOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AttrKey);
		}
	}
}
