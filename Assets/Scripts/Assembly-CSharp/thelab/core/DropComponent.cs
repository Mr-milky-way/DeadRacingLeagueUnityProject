using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class DropComponent : EventComponent
	{
		[HideInInspector]
		public RectTransform target;

		protected new void Awake()
		{
			List<UIEventType> list = new List<UIEventType>();
			if (!WillDispatch(UIEventType.Drop))
			{
				list.Add(UIEventType.Drop);
			}
			allowed = list.ToArray();
			if (base.callback != null)
			{
				base.callback.AddListener(OnUIEvent);
			}
		}

		private void OnUIEvent(UIEvent p_event)
		{
			if (p_event.type == UIEventType.Drop)
			{
				target = p_event.target.element.GetComponent<RectTransform>();
			}
		}
	}
}
