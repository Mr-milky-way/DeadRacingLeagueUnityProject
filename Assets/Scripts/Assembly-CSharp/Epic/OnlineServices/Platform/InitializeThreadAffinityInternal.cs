using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Platform
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct InitializeThreadAffinityInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private ulong m_NetworkWork;

		private ulong m_StorageIo;

		private ulong m_WebSocketIo;

		private ulong m_P2PIo;

		private ulong m_HttpRequestIo;

		public ulong NetworkWork
		{
			get
			{
				return m_NetworkWork;
			}
			set
			{
				m_NetworkWork = value;
			}
		}

		public ulong StorageIo
		{
			get
			{
				return m_StorageIo;
			}
			set
			{
				m_StorageIo = value;
			}
		}

		public ulong WebSocketIo
		{
			get
			{
				return m_WebSocketIo;
			}
			set
			{
				m_WebSocketIo = value;
			}
		}

		public ulong P2PIo
		{
			get
			{
				return m_P2PIo;
			}
			set
			{
				m_P2PIo = value;
			}
		}

		public ulong HttpRequestIo
		{
			get
			{
				return m_HttpRequestIo;
			}
			set
			{
				m_HttpRequestIo = value;
			}
		}

		public void Set(InitializeThreadAffinity other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				NetworkWork = other.NetworkWork;
				StorageIo = other.StorageIo;
				WebSocketIo = other.WebSocketIo;
				P2PIo = other.P2PIo;
				HttpRequestIo = other.HttpRequestIo;
			}
		}

		public void Set(object other)
		{
			Set(other as InitializeThreadAffinity);
		}

		public void Dispose()
		{
		}
	}
}
