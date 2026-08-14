using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Platform
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AndroidInitializeOptionsSystemInitializeOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Reserved;

		private IntPtr m_OptionalInternalDirectory;

		private IntPtr m_OptionalExternalDirectory;

		public IntPtr Reserved
		{
			get
			{
				return m_Reserved;
			}
			set
			{
				m_Reserved = value;
			}
		}

		public string OptionalInternalDirectory
		{
			get
			{
				Helper.TryMarshalGet(m_OptionalInternalDirectory, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_OptionalInternalDirectory, value);
			}
		}

		public string OptionalExternalDirectory
		{
			get
			{
				Helper.TryMarshalGet(m_OptionalExternalDirectory, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_OptionalExternalDirectory, value);
			}
		}

		public void Set(AndroidInitializeOptionsSystemInitializeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				Reserved = other.Reserved;
				OptionalInternalDirectory = other.OptionalInternalDirectory;
				OptionalExternalDirectory = other.OptionalExternalDirectory;
			}
		}

		public void Set(object other)
		{
			Set(other as AndroidInitializeOptionsSystemInitializeOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Reserved);
			Helper.TryMarshalDispose(ref m_OptionalInternalDirectory);
			Helper.TryMarshalDispose(ref m_OptionalExternalDirectory);
		}
	}
}
