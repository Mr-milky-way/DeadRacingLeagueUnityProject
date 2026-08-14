namespace thelab.mvc
{
	public class NotificationView : NotificationView<BaseApplication>
	{
	}
	public class NotificationView<T> : View<T> where T : BaseApplication
	{
		public string notification;
	}
}
