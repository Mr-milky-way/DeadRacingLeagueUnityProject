using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PageQueryInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private int m_StartIndex;

		private int m_MaxCount;

		public int StartIndex
		{
			get
			{
				return m_StartIndex;
			}
			set
			{
				m_StartIndex = value;
			}
		}

		public int MaxCount
		{
			get
			{
				return m_MaxCount;
			}
			set
			{
				m_MaxCount = value;
			}
		}

		public void Set(PageQuery other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				StartIndex = other.StartIndex;
				MaxCount = other.MaxCount;
			}
		}

		public void Set(object other)
		{
			Set(other as PageQuery);
		}

		public void Dispose()
		{
		}
	}
}
