namespace Epic.OnlineServices.UI
{
	public class OnDisplaySettingsUpdatedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public bool IsVisible { get; private set; }

		public bool IsExclusiveInput { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(OnDisplaySettingsUpdatedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				IsVisible = other.Value.IsVisible;
				IsExclusiveInput = other.Value.IsExclusiveInput;
			}
		}

		public void Set(object other)
		{
			Set(other as OnDisplaySettingsUpdatedCallbackInfoInternal?);
		}
	}
}
