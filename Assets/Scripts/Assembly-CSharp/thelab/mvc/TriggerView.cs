using UnityEngine;

namespace thelab.mvc
{
	public class TriggerView : TriggerView<BaseApplication>
	{
	}
	public class TriggerView<T> : ColliderView<T> where T : BaseApplication
	{
		public void OnTriggerEnter(Collider p_collider)
		{
			if (enter)
			{
				Notify(notification + "@trigger.enter", p_collider);
			}
		}

		public void OnTriggerExit(Collider p_collider)
		{
			if (exit)
			{
				Notify(notification + "@trigger.exit", p_collider);
			}
		}

		private void OnTriggerStay(Collider p_collider)
		{
			if (stay)
			{
				Notify(notification + "@trigger.stay", p_collider);
			}
		}
	}
}
