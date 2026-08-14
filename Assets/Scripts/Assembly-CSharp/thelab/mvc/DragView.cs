using UnityEngine;
using UnityEngine.EventSystems;

namespace thelab.mvc
{
	public class DragView<T> : DragView where T : BaseApplication
	{
		public new T app => (T)base.app;
	}
	public class DragView : NotificationView, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
	{
		public bool drag;

		public Vector2 position;

		public GameObject target;

		public bool usePreview = true;

		public bool hide;

		public float alpha = 0.5f;

		public GameObject preview;

		private RectTransform m_canvas;

		private CanvasGroup m_hide_group;

		private float m_initial_alpha;

		private Vector2 m_delta;

		private void Start()
		{
			drag = false;
			position = default(Vector2);
			m_delta = default(Vector2);
			Transform parent = base.transform.parent;
			Canvas canvas = null;
			while ((bool)parent)
			{
				canvas = parent.GetComponent<Canvas>();
				if ((bool)canvas)
				{
					break;
				}
				parent = parent.parent;
			}
			if ((bool)canvas)
			{
				m_canvas = canvas.GetComponent<RectTransform>();
			}
		}

		public void OnDrag(PointerEventData e)
		{
			if (drag)
			{
				Notify(notification + "@drag", e);
				position = e.position;
				if ((bool)preview)
				{
					preview.GetComponent<RectTransform>().position = position + m_delta;
				}
			}
		}

		public void OnBeginDrag(PointerEventData e)
		{
			Notify(notification + "@drag-starts", e);
			target = e.pointerDrag;
			drag = true;
			if (!target)
			{
				return;
			}
			Vector2 size = target.GetComponent<RectTransform>().rect.size;
			preview = Object.Instantiate(target);
			preview.name = "drag-preview";
			RectTransform component = preview.GetComponent<RectTransform>();
			DragView component2 = preview.GetComponent<DragView>();
			if ((bool)component2)
			{
				component2.enabled = false;
			}
			component.SetParent(m_canvas, worldPositionStays: true);
			component.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Abs(size.x));
			component.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(size.y));
			CanvasGroup canvasGroup = preview.GetComponent<CanvasGroup>();
			if (!canvasGroup)
			{
				canvasGroup = preview.AddComponent<CanvasGroup>();
			}
			canvasGroup.alpha = alpha;
			canvasGroup.blocksRaycasts = false;
			if (hide)
			{
				canvasGroup = target.GetComponent<CanvasGroup>();
				if (!canvasGroup)
				{
					canvasGroup = target.AddComponent<CanvasGroup>();
				}
				m_hide_group = canvasGroup;
				m_initial_alpha = m_hide_group.alpha;
				m_hide_group.alpha = 0f;
			}
		}

		public void OnEndDrag(PointerEventData e)
		{
			EndDrag();
		}

		public void EndDrag()
		{
			if ((bool)preview)
			{
				Object.Destroy(preview);
				preview = null;
			}
			ShowTarget();
			if (drag)
			{
				drag = false;
				Notify(notification + "@drag-end");
			}
		}

		private void ShowTarget()
		{
			if (!hide)
			{
				return;
			}
			CanvasGroup component = target.GetComponent<CanvasGroup>();
			if (!component)
			{
				if ((bool)m_hide_group)
				{
					m_hide_group.alpha = 1f;
					Object.Destroy(m_hide_group);
				}
			}
			else
			{
				component.alpha = m_initial_alpha;
			}
		}

		private void Update()
		{
			if (drag && Input.GetKeyDown(KeyCode.Escape))
			{
				EndDrag();
			}
		}
	}
}
