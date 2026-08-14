using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	[RequireComponent(typeof(EventComponent))]
	public class MousePan : MonoBehaviour
	{
		public EventComponent events;

		public RectTransform target;

		protected Vector2 m_mouse_down;

		protected Vector2 m_target_down;

		protected bool m_isdown;

		protected CanvasScaler m_canvas;

		protected void Awake()
		{
			events = GetComponent<EventComponent>();
			events.allowed = new UIEventType[1];
			events.callback.AddListener(OnEvent);
			Transform parent = base.transform;
			while ((bool)parent)
			{
				m_canvas = parent.GetComponent<CanvasScaler>();
				if (!m_canvas)
				{
					parent = parent.parent;
					continue;
				}
				break;
			}
		}

		protected Vector2 GetMousePosition()
		{
			Vector2 vector = (m_canvas ? m_canvas.referenceResolution : new Vector2(Screen.width, Screen.height));
			Vector2 vector2 = Input.mousePosition;
			vector2.x /= Screen.width;
			vector2.y /= Screen.height;
			return new Vector2(vector2.x * vector.x, vector2.y * vector.y);
		}

		protected void OnEvent(UIEvent p_event)
		{
			if ((bool)target && p_event.type == UIEventType.Down)
			{
				m_mouse_down = GetMousePosition();
				m_target_down = target.anchoredPosition;
				m_isdown = true;
			}
		}

		protected void Update()
		{
			if (m_isdown)
			{
				if (Input.GetKeyUp(KeyCode.Mouse0))
				{
					m_isdown = false;
					return;
				}
				Vector2 vector = GetMousePosition() - m_mouse_down;
				target.anchoredPosition = m_target_down + vector;
			}
		}
	}
}
