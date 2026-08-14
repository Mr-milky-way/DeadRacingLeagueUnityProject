namespace thelab.mvc
{
	public class View : Element
	{
	}
	public class View<T> : View where T : BaseApplication
	{
		public NotificationCallback OnEvent;

		public new T app => (T)base.app;

		public override void Notify(float p_delay, string p_event, params object[] p_data)
		{
			base.Notify(p_delay, p_event, p_data);
			RunOnce(p_delay, delegate
			{
				InvokeEvent(p_event, p_data);
			});
		}

		public override void Notify(string p_event, params object[] p_data)
		{
			base.Notify(p_event, p_data);
			InvokeEvent(p_event, p_data);
		}

		private void InvokeEvent(string p_notification, object[] p_data)
		{
			if (OnEvent != null)
			{
				NotificationEvent notificationEvent = new NotificationEvent();
				notificationEvent.target = this;
				notificationEvent.notification = p_notification;
				notificationEvent.data = p_data;
				OnEvent.Invoke(notificationEvent);
			}
		}
	}
}
