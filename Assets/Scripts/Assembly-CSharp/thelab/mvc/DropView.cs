using UnityEngine.EventSystems;

namespace thelab.mvc
{
	public class DropView<T> : DragView where T : BaseApplication
	{
		public new T app => (T)base.app;
	}
	public class DropView : NotificationView, IDropHandler, IEventSystemHandler
	{
		private void Start()
		{
		}

		public void OnDrop(PointerEventData e)
		{
			Notify(notification + "@drop", e);
		}
	}
}
