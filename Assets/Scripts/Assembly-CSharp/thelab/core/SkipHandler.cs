namespace thelab.core
{
	public class SkipHandler
	{
		private Flow mFlow;

		public bool Skip { get; private set; }

		public void Listen(Flow targetFlow)
		{
			mFlow = targetFlow;
			FNSkip.OnSkipStart += OnSkipStartHandler;
			FNSkip.OnSkipStop += OnSkipStopHandler;
		}

		private void OnSkipStartHandler()
		{
			Skip = true;
			DoSkip();
		}

		private void DoSkip()
		{
			while (Skip)
			{
				if ((bool)mFlow)
				{
					mFlow.SendMessage("Update");
				}
				else
				{
					Skip = false;
				}
			}
			mFlow = null;
		}

		public void OnSkipStopHandler()
		{
			FNSkip.OnSkipStart -= OnSkipStartHandler;
			FNSkip.OnSkipStop -= OnSkipStopHandler;
			Skip = false;
		}
	}
}
