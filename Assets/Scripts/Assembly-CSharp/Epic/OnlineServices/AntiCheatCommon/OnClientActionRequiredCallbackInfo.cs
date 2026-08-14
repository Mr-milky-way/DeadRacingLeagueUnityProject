using System;

namespace Epic.OnlineServices.AntiCheatCommon
{
	public class OnClientActionRequiredCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public IntPtr ClientHandle { get; private set; }

		public AntiCheatCommonClientAction ClientAction { get; private set; }

		public AntiCheatCommonClientActionReason ActionReasonCode { get; private set; }

		public string ActionReasonDetailsString { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(OnClientActionRequiredCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				ClientHandle = other.Value.ClientHandle;
				ClientAction = other.Value.ClientAction;
				ActionReasonCode = other.Value.ActionReasonCode;
				ActionReasonDetailsString = other.Value.ActionReasonDetailsString;
			}
		}

		public void Set(object other)
		{
			Set(other as OnClientActionRequiredCallbackInfoInternal?);
		}
	}
}
