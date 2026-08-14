using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UI
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SetToggleFriendsKeyOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private KeyCombination m_KeyCombination;

		public KeyCombination KeyCombination
		{
			set
			{
				m_KeyCombination = value;
			}
		}

		public void Set(SetToggleFriendsKeyOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				KeyCombination = other.KeyCombination;
			}
		}

		public void Set(object other)
		{
			Set(other as SetToggleFriendsKeyOptions);
		}

		public void Dispose()
		{
		}
	}
}
