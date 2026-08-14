using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace thelab.core
{
	public class EventComponent : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerClickHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IMoveHandler, IDropHandler, IEndDragHandler, IScrollHandler
	{
		public RectTransform container;

		private Canvas m_canvas;

		public UIEventType[] allowed = new UIEventType[1] { UIEventType.Click };

		public bool down;

		public bool over;

		public bool overRect;

		public bool drag;

		public float hold;

		public float stay;

		public Vector2 dragStartPosition;

		public Vector2 dragEndPosition;

		public Vector2 dragDirection;

		public float dragFactor;

		public Vector2 delta;

		public AxisEventData axis;

		public PointerEventData data;

		public GameObject element;

		public float multiClickTime = 0.1f;

		private Dictionary<PointerEventData.InputButton, int> m_click_count;

		private Vector2 m_last_move;

		[SerializeField]
		private EventComponentCallback m_callback;

		public Canvas canvas
		{
			get
			{
				if ((bool)m_canvas)
				{
					return m_canvas;
				}
				if (!container)
				{
					return m_canvas;
				}
				return m_canvas = Hierarchy.FindReverse<Canvas>(container);
			}
		}

		public Vector2 dragOffset => dragEndPosition - dragStartPosition;

		public Rect dragRect
		{
			get
			{
				Vector2 vector = dragStartPosition;
				Vector2 mousePosition = GetMousePosition();
				Vector2 vector2 = default(Vector2);
				vector2.x = Mathf.Min(vector.x, mousePosition.x);
				vector2.y = Mathf.Min(vector.y, mousePosition.y);
				Vector2 vector3 = default(Vector2);
				vector3.x = Mathf.Max(vector.x, mousePosition.x);
				vector3.y = Mathf.Max(vector.y, mousePosition.y);
				Vector2 position = vector2;
				Vector2 size = mousePosition - vector;
				size.x = Mathf.Abs(size.x);
				size.y = Mathf.Abs(size.y);
				return new Rect(position, size);
			}
		}

		public EventComponentCallback callback
		{
			get
			{
				if (m_callback != null)
				{
					return m_callback;
				}
				return m_callback = new EventComponentCallback();
			}
		}

		public bool WillDispatch(UIEventType p_type)
		{
			if (!this)
			{
				return false;
			}
			if (!base.gameObject)
			{
				return false;
			}
			if (!base.enabled)
			{
				return false;
			}
			return Array.IndexOf(allowed, p_type) >= 0;
		}

		protected void Awake()
		{
			m_click_count = new Dictionary<PointerEventData.InputButton, int>();
			for (int i = 0; i < 3; i++)
			{
				m_click_count[(PointerEventData.InputButton)i] = 0;
			}
		}

		public void Dispatch(UIEventType p_type)
		{
			if (callback != null && WillDispatch(p_type))
			{
				UIEvent uIEvent = new UIEvent();
				uIEvent.target = this;
				uIEvent.type = p_type;
				callback.Invoke(uIEvent);
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			data = eventData;
			down = true;
			hold = 0f;
			Dispatch(UIEventType.Down);
			Activity.Run(OnHoldUpdate);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			data = eventData;
			down = false;
			Dispatch(UIEventType.Click);
			PointerEventData.InputButton bt = data.button;
			int c = m_click_count[bt];
			m_click_count[bt]++;
			if (c != 0)
			{
				return;
			}
			Activity.Run(delegate(float t)
			{
				if (t < multiClickTime)
				{
					return true;
				}
				c = m_click_count[bt];
				m_click_count[bt] = 0;
				if (c <= 1)
				{
					return false;
				}
				data.clickCount = c;
				Dispatch(UIEventType.MultiClick);
				return false;
			});
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			data = eventData;
			down = false;
			Dispatch(UIEventType.Up);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			data = eventData;
			over = true;
			overRect = true;
			stay = 0f;
			m_last_move = GetMousePosition();
			Dispatch(UIEventType.Enter);
			if ((bool)eventData.pointerDrag)
			{
				element = eventData.pointerDrag;
				Dispatch(UIEventType.DragOver);
				EventComponent component = element.GetComponent<EventComponent>();
				if ((bool)component)
				{
					component.element = base.gameObject;
					component.Dispatch(UIEventType.DragOver);
				}
			}
			Activity.Run(OnStayUpdate);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			data = eventData;
			over = false;
			Dispatch(UIEventType.Exit);
		}

		public void OnMove(AxisEventData eventData)
		{
			axis = eventData;
			Dispatch(UIEventType.AxisMove);
		}

		public void OnDrag(PointerEventData eventData)
		{
			data = eventData;
			DragStart();
			dragEndPosition = GetMousePosition();
			Vector2 vector = dragEndPosition;
			Vector2 vector2 = dragStartPosition;
			if (dragDirection.magnitude <= 0f)
			{
				dragDirection = dragOffset.normalized;
				dragDirection.x = Mathf.Abs(dragDirection.x);
				dragDirection.y = Mathf.Abs(dragDirection.y);
			}
			Vector2 lhs = vector - vector2;
			dragFactor = Vector2.Dot(lhs, dragDirection);
			Dispatch(UIEventType.DragUpdate);
		}

		public void OnDrop(PointerEventData eventData)
		{
			data = eventData;
			DragEnd();
			element = eventData.pointerDrag;
			Dispatch(UIEventType.Drop);
			EventComponent component = element.GetComponent<EventComponent>();
			if ((bool)component)
			{
				component.element = base.gameObject;
				component.Dispatch(UIEventType.Drop);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			data = eventData;
			DragEnd();
		}

		private void DragStart()
		{
			if (!drag)
			{
				dragStartPosition = (dragEndPosition = GetMousePosition());
				dragDirection = Vector2.zero;
				dragFactor = 0f;
				Dispatch(UIEventType.DragStart);
			}
			drag = true;
		}

		private void DragEnd()
		{
			dragEndPosition = GetMousePosition();
			if (drag)
			{
				Dispatch(UIEventType.DragEnd);
			}
			drag = false;
			dragStartPosition = (dragEndPosition = Vector2.zero);
		}

		private bool OnHoldUpdate(float t)
		{
			if (down)
			{
				Dispatch(UIEventType.Hold);
				hold += Time.deltaTime;
			}
			return down;
		}

		private bool OnStayUpdate(float t)
		{
			if (over)
			{
				Vector2 mousePosition = GetMousePosition();
				delta = mousePosition - m_last_move;
				m_last_move = GetMousePosition();
				if (delta.sqrMagnitude > 1f)
				{
					Dispatch(UIEventType.Move);
					delta = Vector2.zero;
				}
				Dispatch(UIEventType.Stay);
				stay += Time.deltaTime;
			}
			return over;
		}

		public void OnScroll(PointerEventData eventData)
		{
			data = eventData;
			Dispatch(UIEventType.Scroll);
		}

		public Vector2 GetMousePosition()
		{
			Vector2 localPoint = Input.mousePosition;
			if (!container)
			{
				return localPoint;
			}
			Canvas canvas = this.canvas;
			if (!canvas)
			{
				return localPoint;
			}
			Camera worldCamera = canvas.worldCamera;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(container, localPoint, worldCamera, out localPoint);
			return localPoint;
		}
	}
}
