namespace thelab.core
{
	public class ScreenEvent
	{
		public ScreenEventType type;

		public UIScreen target;

		public ScreenEvent(ScreenEventType p_type, UIScreen p_target)
		{
			type = p_type;
			target = p_target;
		}
	}
}
