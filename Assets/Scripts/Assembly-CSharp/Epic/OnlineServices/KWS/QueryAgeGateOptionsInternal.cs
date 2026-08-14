using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.KWS
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryAgeGateOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(QueryAgeGateOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryAgeGateOptions);
		}

		public void Dispose()
		{
		}
	}
}
