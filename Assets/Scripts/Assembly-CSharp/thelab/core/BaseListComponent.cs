using UnityEngine;

namespace thelab.core
{
	public class BaseListComponent : MonoBehaviour
	{
		public ListEvent OnEvent;

		public virtual int Count => 0;

		protected virtual void OnLayout()
		{
		}

		protected void InvokeEvent(ListEvent.Type p_type, GameObject p_item = null)
		{
			if (OnEvent != null && OnEvent.GetPersistentEventCount() > 0)
			{
				ListEventData listEventData = new ListEventData();
				listEventData.type = p_type;
				listEventData.target = this;
				listEventData.item = p_item;
				OnEvent.Invoke(listEventData);
			}
		}
	}
}
