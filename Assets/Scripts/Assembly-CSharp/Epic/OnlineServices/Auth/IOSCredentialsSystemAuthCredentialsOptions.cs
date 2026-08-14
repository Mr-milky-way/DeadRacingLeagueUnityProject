using System;

namespace Epic.OnlineServices.Auth
{
	public class IOSCredentialsSystemAuthCredentialsOptions : ISettable
	{
		public IntPtr PresentationContextProviding { get; set; }

		internal void Set(IOSCredentialsSystemAuthCredentialsOptionsInternal? other)
		{
			if (other.HasValue)
			{
				PresentationContextProviding = other.Value.PresentationContextProviding;
			}
		}

		public void Set(object other)
		{
			Set(other as IOSCredentialsSystemAuthCredentialsOptionsInternal?);
		}
	}
}
