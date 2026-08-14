using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SetPortRangeOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private ushort m_Port;

		private ushort m_MaxAdditionalPortsToTry;

		public ushort Port
		{
			set
			{
				m_Port = value;
			}
		}

		public ushort MaxAdditionalPortsToTry
		{
			set
			{
				m_MaxAdditionalPortsToTry = value;
			}
		}

		public void Set(SetPortRangeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Port = other.Port;
				MaxAdditionalPortsToTry = other.MaxAdditionalPortsToTry;
			}
		}

		public void Set(object other)
		{
			Set(other as SetPortRangeOptions);
		}

		public void Dispose()
		{
		}
	}
}
