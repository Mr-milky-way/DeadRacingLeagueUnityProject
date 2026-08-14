using UnityEngine;

namespace thelab.mvc
{
	public class CollisionView : CollisionView<BaseApplication>
	{
	}
	public class CollisionView<T> : ColliderView<T> where T : BaseApplication
	{
		private void OnCollisionEnter(Collision p_collider)
		{
			if (enter)
			{
				Notify(notification + "@collision.enter", p_collider);
			}
		}

		private void OnCollisionExit(Collision p_collider)
		{
			if (exit)
			{
				Notify(notification + "@collision.exit", p_collider);
			}
		}

		private void OnCollisionStay(Collision p_collider)
		{
			if (stay)
			{
				Notify(notification + "@collision.stay", p_collider);
			}
		}
	}
}
