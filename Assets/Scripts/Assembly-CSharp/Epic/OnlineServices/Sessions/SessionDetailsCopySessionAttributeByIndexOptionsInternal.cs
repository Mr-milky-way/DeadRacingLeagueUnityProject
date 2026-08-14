using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionDetailsCopySessionAttributeByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_AttrIndex;

		public uint AttrIndex
		{
			set
			{
				m_AttrIndex = value;
			}
		}

		public void Set(SessionDetailsCopySessionAttributeByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				AttrIndex = other.AttrIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionDetailsCopySessionAttributeByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}
