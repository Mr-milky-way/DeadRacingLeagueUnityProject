using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Mods
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ModIdentifierInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_NamespaceId;

		private IntPtr m_ItemId;

		private IntPtr m_ArtifactId;

		private IntPtr m_Title;

		private IntPtr m_Version;

		public string NamespaceId
		{
			get
			{
				Helper.TryMarshalGet(m_NamespaceId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_NamespaceId, value);
			}
		}

		public string ItemId
		{
			get
			{
				Helper.TryMarshalGet(m_ItemId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ItemId, value);
			}
		}

		public string ArtifactId
		{
			get
			{
				Helper.TryMarshalGet(m_ArtifactId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ArtifactId, value);
			}
		}

		public string Title
		{
			get
			{
				Helper.TryMarshalGet(m_Title, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Title, value);
			}
		}

		public string Version
		{
			get
			{
				Helper.TryMarshalGet(m_Version, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Version, value);
			}
		}

		public void Set(ModIdentifier other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				NamespaceId = other.NamespaceId;
				ItemId = other.ItemId;
				ArtifactId = other.ArtifactId;
				Title = other.Title;
				Version = other.Version;
			}
		}

		public void Set(object other)
		{
			Set(other as ModIdentifier);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_NamespaceId);
			Helper.TryMarshalDispose(ref m_ItemId);
			Helper.TryMarshalDispose(ref m_ArtifactId);
			Helper.TryMarshalDispose(ref m_Title);
			Helper.TryMarshalDispose(ref m_Version);
		}
	}
}
