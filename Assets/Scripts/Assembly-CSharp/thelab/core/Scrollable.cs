using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace thelab.core
{
	public class Scrollable : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IScrollHandler
	{
		public bool scroll;

		[SerializeField]
		private ScrollRect m_scrollrect;

		public ScrollRect scrollrect
		{
			get
			{
				if ((bool)m_scrollrect)
				{
					return m_scrollrect;
				}
				return m_scrollrect = Hierarchy.FindReverse<ScrollRect>(base.transform);
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if ((bool)scrollrect)
			{
				scrollrect.OnBeginDrag(eventData);
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if ((bool)scrollrect)
			{
				scrollrect.OnDrag(eventData);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if ((bool)scrollrect)
			{
				scrollrect.OnEndDrag(eventData);
			}
		}

		public void OnScroll(PointerEventData data)
		{
			if ((bool)scrollrect)
			{
				scrollrect.OnScroll(data);
				if (scroll && (bool)scrollrect.verticalScrollbar)
				{
					scrollrect.verticalScrollbar.value += data.scrollDelta.y * scrollrect.scrollSensitivity;
				}
			}
		}
	}
}
