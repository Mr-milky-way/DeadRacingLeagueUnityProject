using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SetRelayControlOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private RelayControl m_RelayControl;

		public RelayControl RelayControl
		{
			set
			{
				m_RelayControl = value;
			}
		}

		public void Set(SetRelayControlOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				RelayControl = other.RelayControl;
			}
		}

		public void Set(object other)
		{
			Set(other as SetRelayControlOptions);
		}

		public void Dispose()
		{
		}
	}
}
