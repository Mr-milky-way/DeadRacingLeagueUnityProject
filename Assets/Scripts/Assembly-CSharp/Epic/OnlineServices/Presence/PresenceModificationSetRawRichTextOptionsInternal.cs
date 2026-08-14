using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PresenceModificationSetRawRichTextOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_RichText;

		public string RichText
		{
			set
			{
				Helper.TryMarshalSet(ref m_RichText, value);
			}
		}

		public void Set(PresenceModificationSetRawRichTextOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				RichText = other.RichText;
			}
		}

		public void Set(object other)
		{
			Set(other as PresenceModificationSetRawRichTextOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_RichText);
		}
	}
}
