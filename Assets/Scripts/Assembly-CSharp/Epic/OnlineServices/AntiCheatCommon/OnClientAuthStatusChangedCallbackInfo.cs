using System;

namespace Epic.OnlineServices.AntiCheatCommon
{
	public class OnClientAuthStatusChangedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public IntPtr ClientHandle { get; private set; }

		public AntiCheatCommonClientAuthStatus ClientAuthStatus { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(OnClientAuthStatusChangedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				ClientHandle = other.Value.ClientHandle;
				ClientAuthStatus = other.Value.ClientAuthStatus;
			}
		}

		public void Set(object other)
		{
			Set(other as OnClientAuthStatusChangedCallbackInfoInternal?);
		}
	}
}
