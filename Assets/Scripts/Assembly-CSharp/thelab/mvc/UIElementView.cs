using UnityEngine;
using UnityEngine.EventSystems;
using drl.game;
using thelab.core;

namespace thelab.mvc
{
	public class UIElementView<T> : UIElementView where T : BaseApplication
	{
		public new T app => (T)base.app;
	}
	public class UIElementView : NotificationView, IPointerDownHandler, IEventSystemHandler, IPointerClickHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, ISubmitHandler, IFocusHandler, IScrollHandler, IDeselectHandler
	{
		public bool down;

		public bool over;

		public float hold;

		public Vector2 scroll;

		public PointerEventData data;

		[SerializeField]
		private bool m_interactable = true;

		private Canvas m_canvas;

		private RectTransform m_rectTransform;

		public bool interactable
		{
			get
			{
				return m_interactable;
			}
			set
			{
				bool flag = true;
				Tag[] componentsInChildren = base.transform.GetComponentsInChildren<Tag>(includeInactive: true);
				Tag[] array = componentsInChildren;
				foreach (Tag tag in array)
				{
					if (tag.label == "disabledOverlay")
					{
						tag.gameObject.SetActive(!value);
						flag = false;
					}
				}
				if (flag)
				{
					CanvasGroup component = GetComponent<CanvasGroup>();
					if ((bool)component)
					{
						component.alpha = (value ? 1f : 0.25f);
						bool blocksRaycasts = (component.interactable = value);
						component.blocksRaycasts = blocksRaycasts;
					}
				}
				array = componentsInChildren;
				foreach (Tag tag2 in array)
				{
					if (tag2.label == "focus")
					{
						tag2.gameObject.SetActive(value);
					}
				}
				m_interactable = value;
			}
		}

		public Canvas canvas
		{
			get
			{
				if ((bool)m_canvas)
				{
					return m_canvas;
				}
				RectTransform rectTransform = this.rectTransform;
				if (!rectTransform)
				{
					return null;
				}
				return m_canvas = Hierarchy.FindReverse<Canvas>(rectTransform);
			}
		}

		public RectTransform rectTransform
		{
			get
			{
				if (!m_rectTransform)
				{
					return m_rectTransform = (RectTransform)base.transform;
				}
				return m_rectTransform;
			}
		}

		private void Awake()
		{
			hold = 0f;
			scroll = Vector2.zero;
			down = false;
			over = false;
			interactable = m_interactable;
		}

		public void OnPointerDown(PointerEventData p_event_data)
		{
			if (base.isActiveAndEnabled)
			{
				down = true;
				hold = 0f;
				data = p_event_data;
				OnState("down");
				Notify(notification + "@down");
			}
		}

		public void OnPointerClick(PointerEventData p_event_data)
		{
			if (base.isActiveAndEnabled && interactable && !DRLUINavigationSystem.IsTyping)
			{
				switch (p_event_data.button)
				{
				case PointerEventData.InputButton.Left:
					OnState("lclick");
					break;
				case PointerEventData.InputButton.Right:
					OnState("rclick");
					break;
				case PointerEventData.InputButton.Middle:
					OnState("mclick");
					break;
				}
				data = p_event_data;
				Notify(notification + "@click");
			}
		}

		public void OnPointerUp(PointerEventData p_event_data)
		{
			if (base.isActiveAndEnabled)
			{
				down = false;
				OnState("up");
				data = p_event_data;
				Notify(notification + "@up");
				hold = 0f;
			}
		}

		public void OnPointerEnter(PointerEventData p_event_data)
		{
			if (base.isActiveAndEnabled)
			{
				over = true;
				OnState("over");
				data = p_event_data;
				Notify(notification + "@over");
			}
		}

		public void OnPointerExit(PointerEventData p_event_data)
		{
			if (base.isActiveAndEnabled)
			{
				over = false;
				OnState("out");
				data = p_event_data;
				Notify(notification + "@out");
			}
		}

		protected virtual void OnState(string p_state)
		{
		}

		public void OnSubmit(BaseEventData p_event_data)
		{
			if (base.isActiveAndEnabled && interactable)
			{
				OnState("submit");
				Notify(notification + "@submit");
			}
		}

		private void Update()
		{
			if (base.isActiveAndEnabled && interactable && down)
			{
				OnState("hold");
				Notify(notification + "@hold");
				hold += Time.unscaledDeltaTime;
			}
		}

		public void OnScroll(PointerEventData p_event_data)
		{
			if (base.isActiveAndEnabled)
			{
				if (p_event_data != null)
				{
					scroll = p_event_data.scrollDelta;
				}
				OnState("scroll");
				data = p_event_data;
				Notify(notification + "@scroll", p_event_data);
				scroll = Vector2.zero;
			}
		}

		public virtual void OnSelect(BaseEventData p_event_data)
		{
			if (base.isActiveAndEnabled && interactable)
			{
				OnState("select");
				Notify(notification + "@select");
			}
		}

		public virtual void OnDeselect(BaseEventData p_event_data)
		{
			if (base.isActiveAndEnabled && interactable)
			{
				OnState("deselect");
				Notify(notification + "@deselect");
			}
		}

		public virtual void OnFocus()
		{
			OnState("focus");
			Notify(notification + "@focus");
		}

		public virtual void OnUnfocus()
		{
			OnState("unfocus");
			Notify(notification + "@unfocus");
		}

		public Vector2 GetMousePosition(RectTransform p_container = null)
		{
			Vector2 localPoint = Input.mousePosition;
			RectTransform rectTransform = (p_container ? p_container : this.rectTransform);
			if (!rectTransform)
			{
				return localPoint;
			}
			Canvas canvas = this.canvas;
			if (!canvas)
			{
				return localPoint;
			}
			Camera worldCamera = canvas.worldCamera;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, localPoint, worldCamera, out localPoint);
			return localPoint;
		}
	}
}
