using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbySearchRemoveParameterOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Key;

		private ComparisonOp m_ComparisonOp;

		public string Key
		{
			set
			{
				Helper.TryMarshalSet(ref m_Key, value);
			}
		}

		public ComparisonOp ComparisonOp
		{
			set
			{
				m_ComparisonOp = value;
			}
		}

		public void Set(LobbySearchRemoveParameterOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Key = other.Key;
				ComparisonOp = other.ComparisonOp;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbySearchRemoveParameterOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Key);
		}
	}
}
