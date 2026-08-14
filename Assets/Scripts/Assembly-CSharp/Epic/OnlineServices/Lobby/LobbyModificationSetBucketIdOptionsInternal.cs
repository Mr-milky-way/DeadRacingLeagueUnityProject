using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyModificationSetBucketIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_BucketId;

		public string BucketId
		{
			set
			{
				Helper.TryMarshalSet(ref m_BucketId, value);
			}
		}

		public void Set(LobbyModificationSetBucketIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				BucketId = other.BucketId;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyModificationSetBucketIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_BucketId);
		}
	}
}
