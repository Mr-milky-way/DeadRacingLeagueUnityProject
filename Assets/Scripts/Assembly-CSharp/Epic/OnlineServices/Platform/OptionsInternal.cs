using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Platform
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct OptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Reserved;

		private IntPtr m_ProductId;

		private IntPtr m_SandboxId;

		private ClientCredentialsInternal m_ClientCredentials;

		private int m_IsServer;

		private IntPtr m_EncryptionKey;

		private IntPtr m_OverrideCountryCode;

		private IntPtr m_OverrideLocaleCode;

		private IntPtr m_DeploymentId;

		private PlatformFlags m_Flags;

		private IntPtr m_CacheDirectory;

		private uint m_TickBudgetInMilliseconds;

		private IntPtr m_Reserved2;

		public IntPtr Reserved
		{
			set
			{
				m_Reserved = value;
			}
		}

		public string ProductId
		{
			set
			{
				Helper.TryMarshalSet(ref m_ProductId, value);
			}
		}

		public string SandboxId
		{
			set
			{
				Helper.TryMarshalSet(ref m_SandboxId, value);
			}
		}

		public ClientCredentials ClientCredentials
		{
			set
			{
				Helper.TryMarshalSet(ref m_ClientCredentials, value);
			}
		}

		public bool IsServer
		{
			set
			{
				Helper.TryMarshalSet(ref m_IsServer, value);
			}
		}

		public string EncryptionKey
		{
			set
			{
				Helper.TryMarshalSet(ref m_EncryptionKey, value);
			}
		}

		public string OverrideCountryCode
		{
			set
			{
				Helper.TryMarshalSet(ref m_OverrideCountryCode, value);
			}
		}

		public string OverrideLocaleCode
		{
			set
			{
				Helper.TryMarshalSet(ref m_OverrideLocaleCode, value);
			}
		}

		public string DeploymentId
		{
			set
			{
				Helper.TryMarshalSet(ref m_DeploymentId, value);
			}
		}

		public PlatformFlags Flags
		{
			set
			{
				m_Flags = value;
			}
		}

		public string CacheDirectory
		{
			set
			{
				Helper.TryMarshalSet(ref m_CacheDirectory, value);
			}
		}

		public uint TickBudgetInMilliseconds
		{
			set
			{
				m_TickBudgetInMilliseconds = value;
			}
		}

		public IntPtr Reserved2
		{
			set
			{
				m_Reserved2 = value;
			}
		}

		public void Set(Options other)
		{
			if (other != null)
			{
				m_ApiVersion = 11;
				Reserved = other.Reserved;
				ProductId = other.ProductId;
				SandboxId = other.SandboxId;
				ClientCredentials = other.ClientCredentials;
				IsServer = other.IsServer;
				EncryptionKey = other.EncryptionKey;
				OverrideCountryCode = other.OverrideCountryCode;
				OverrideLocaleCode = other.OverrideLocaleCode;
				DeploymentId = other.DeploymentId;
				Flags = other.Flags;
				CacheDirectory = other.CacheDirectory;
				TickBudgetInMilliseconds = other.TickBudgetInMilliseconds;
				Reserved2 = other.Reserved2;
			}
		}

		public void Set(object other)
		{
			Set(other as Options);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Reserved);
			Helper.TryMarshalDispose(ref m_ProductId);
			Helper.TryMarshalDispose(ref m_SandboxId);
			Helper.TryMarshalDispose(ref m_ClientCredentials);
			Helper.TryMarshalDispose(ref m_EncryptionKey);
			Helper.TryMarshalDispose(ref m_OverrideCountryCode);
			Helper.TryMarshalDispose(ref m_OverrideLocaleCode);
			Helper.TryMarshalDispose(ref m_DeploymentId);
			Helper.TryMarshalDispose(ref m_CacheDirectory);
			Helper.TryMarshalDispose(ref m_Reserved2);
		}
	}
}
