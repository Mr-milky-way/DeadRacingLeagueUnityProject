using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatClient
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AddExternalIntegrityCatalogOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_PathToBinFile;

		public string PathToBinFile
		{
			set
			{
				Helper.TryMarshalSet(ref m_PathToBinFile, value);
			}
		}

		public void Set(AddExternalIntegrityCatalogOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				PathToBinFile = other.PathToBinFile;
			}
		}

		public void Set(object other)
		{
			Set(other as AddExternalIntegrityCatalogOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_PathToBinFile);
		}
	}
}
