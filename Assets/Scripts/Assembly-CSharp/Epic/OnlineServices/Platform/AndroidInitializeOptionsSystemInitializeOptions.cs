using System;

namespace Epic.OnlineServices.Platform
{
	public class AndroidInitializeOptionsSystemInitializeOptions : ISettable
	{
		public IntPtr Reserved { get; set; }

		public string OptionalInternalDirectory { get; set; }

		public string OptionalExternalDirectory { get; set; }

		internal void Set(AndroidInitializeOptionsSystemInitializeOptionsInternal? other)
		{
			if (other.HasValue)
			{
				Reserved = other.Value.Reserved;
				OptionalInternalDirectory = other.Value.OptionalInternalDirectory;
				OptionalExternalDirectory = other.Value.OptionalExternalDirectory;
			}
		}

		public void Set(object other)
		{
			Set(other as AndroidInitializeOptionsSystemInitializeOptionsInternal?);
		}
	}
}
