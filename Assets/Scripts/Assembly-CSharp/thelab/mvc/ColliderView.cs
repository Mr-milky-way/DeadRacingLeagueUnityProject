using UnityEngine;

namespace thelab.mvc
{
	public class ColliderView : ColliderView<BaseApplication>
	{
	}
	public class ColliderView<T> : NotificationView<T> where T : BaseApplication
	{
		private Collider m_collider;

		public bool enter = true;

		public bool exit;

		public bool stay;

		public Collider collider => m_collider = AssertLocal(m_collider);
	}
}
