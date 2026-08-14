using System;

namespace Epic.OnlineServices.AntiCheatCommon
{
	public class OnMessageToClientCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public IntPtr ClientHandle { get; private set; }

		public byte[] MessageData { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(OnMessageToClientCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				ClientHandle = other.Value.ClientHandle;
				MessageData = other.Value.MessageData;
			}
		}

		public void Set(object other)
		{
			Set(other as OnMessageToClientCallbackInfoInternal?);
		}
	}
}
