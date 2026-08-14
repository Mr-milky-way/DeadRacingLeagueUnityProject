using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SocketIdInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 33)]
		private byte[] m_SocketName;

		public string SocketName
		{
			get
			{
				Helper.TryMarshalGet(m_SocketName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_SocketName, value, 33);
			}
		}

		public void Set(SocketId other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SocketName = other.SocketName;
			}
		}

		public void Set(object other)
		{
			Set(other as SocketId);
		}

		public void Dispose()
		{
		}
	}
}
