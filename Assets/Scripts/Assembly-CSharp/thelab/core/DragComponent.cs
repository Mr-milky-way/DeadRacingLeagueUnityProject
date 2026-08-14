using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace thelab.core
{
	public class DragComponent : EventComponent
	{
		public float alphaOriginal = 1f;

		public float alphaCopy = 1f;

		public Vector3 position;

		public bool move;

		[HideInInspector]
		public RectTransform drop;

		private Canvas m_canvas;

		private RectTransform m_canvas_rt;

		private RectTransform m_rt;

		private RectTransform m_copy;

		private CanvasGroup m_original;

		private float m_original_alpha;

		private bool m_original_interactable;

		private bool m_original_blockraycast;

		protected new void Awake()
		{
			List<UIEventType> list = new List<UIEventType>();
			if (!WillDispatch(UIEventType.DragStart))
			{
				list.Add(UIEventType.DragStart);
			}
			if (!WillDispatch(UIEventType.DragEnd))
			{
				list.Add(UIEventType.DragEnd);
			}
			if (!WillDispatch(UIEventType.DragUpdate))
			{
				list.Add(UIEventType.DragUpdate);
			}
			if (!WillDispatch(UIEventType.Drop))
			{
				list.Add(UIEventType.Drop);
			}
			allowed = list.ToArray();
			if (base.callback != null)
			{
				base.callback.AddListener(OnUIEvent);
			}
			Transform parent = base.transform;
			while ((bool)parent)
			{
				m_canvas = parent.GetComponentInParent<Canvas>();
				if ((bool)m_canvas)
				{
					m_canvas_rt = m_canvas.GetComponent<RectTransform>();
					break;
				}
				parent = base.transform.parent;
			}
			m_rt = GetComponent<RectTransform>();
		}

		private void OnUIEvent(UIEvent p_event)
		{
			switch (p_event.type)
			{
			case UIEventType.DragStart:
				OnDragStart();
				break;
			case UIEventType.DragEnd:
				OnDragEnd();
				break;
			case UIEventType.DragUpdate:
				OnDragUpdate(p_event);
				break;
			case UIEventType.Drop:
				drop = p_event.target.element.GetComponent<RectTransform>();
				break;
			case UIEventType.DragOver:
				break;
			}
		}

		private void OnDragStart()
		{
			GameObject gameObject = Object.Instantiate(base.gameObject);
			gameObject.name = "$drag";
			gameObject.hideFlags = HideFlags.HideInHierarchy;
			Object component = gameObject.GetComponent<DragComponent>();
			if ((bool)component)
			{
				Object.Destroy(component);
			}
			component = gameObject.GetComponent<EventComponent>();
			if ((bool)component)
			{
				Object.Destroy(component);
			}
			CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
			m_copy = gameObject.GetComponent<RectTransform>();
			if (!canvasGroup)
			{
				canvasGroup = gameObject.AddComponent<CanvasGroup>();
			}
			canvasGroup.alpha = alphaCopy;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
			canvasGroup = GetComponent<CanvasGroup>();
			if ((bool)canvasGroup)
			{
				m_original = canvasGroup;
				m_original_alpha = canvasGroup.alpha;
				m_original_interactable = canvasGroup.interactable;
				m_original_blockraycast = canvasGroup.blocksRaycasts;
			}
			else
			{
				canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
			}
			canvasGroup.alpha = alphaOriginal;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
			m_copy.SetParent(m_canvas.transform, worldPositionStays: true);
			m_copy.localScale = Vector3.one;
			m_copy.position = base.gameObject.transform.position;
		}

		private void OnDragUpdate(UIEvent p_event)
		{
			PointerEventData pointerEventData = p_event.target.data;
			Vector3 mousePosition = Input.mousePosition;
			Vector2 localPoint = Vector2.zero;
			RectTransform canvas_rt = m_canvas_rt;
			RectTransform rt = m_rt;
			Vector2 localPoint2 = Vector2.zero;
			Camera pressEventCamera = pointerEventData.pressEventCamera;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, pointerEventData.pressPosition, pressEventCamera, out localPoint2);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas_rt, mousePosition, pressEventCamera, out localPoint);
			m_copy.transform.localPosition = localPoint - localPoint2;
			if (move)
			{
				m_copy.SetParent(base.transform.parent, worldPositionStays: true);
				position = m_copy.transform.localPosition;
				m_copy.SetParent(m_canvas.transform, worldPositionStays: true);
			}
		}

		private void OnDragEnd()
		{
			if ((bool)m_copy)
			{
				Object.Destroy(m_copy.gameObject);
			}
			if (move)
			{
				base.transform.localPosition = position;
			}
			if ((bool)m_original)
			{
				m_original.alpha = m_original_alpha;
				m_original.blocksRaycasts = m_original_blockraycast;
				m_original.interactable = m_original_interactable;
			}
			else
			{
				m_original = GetComponent<CanvasGroup>();
				if ((bool)m_original)
				{
					Object.Destroy(m_original);
				}
			}
			m_original = null;
		}
	}
}
