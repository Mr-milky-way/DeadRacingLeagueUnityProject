using UnityEngine;
using UnityEngine.UI;
using thelab.core;

[RequireComponent(typeof(CanvasGroup))]
public class Container : Hierarchy
{
	private float m_alpha;

	private Canvas m_canvas;

	private GraphicRaycaster m_raycaster;

	private CanvasGroup m_group;

	private RectTransform m_rect_transform;

	private Canvas m_main_canvas;

	public Vector2 mouse
	{
		get
		{
			Vector2 localPoint = Vector2.zero;
			Camera cam = (canvas ? canvas.worldCamera : null);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, cam, out localPoint);
			return localPoint;
		}
	}

	public bool mouseEnabled
	{
		get
		{
			if (!group)
			{
				return true;
			}
			if (!group.interactable)
			{
				return group.blocksRaycasts;
			}
			return true;
		}
		set
		{
			if ((bool)group)
			{
				CanvasGroup canvasGroup = group;
				bool interactable = (group.blocksRaycasts = value);
				canvasGroup.interactable = interactable;
			}
		}
	}

	public Vector2 position
	{
		get
		{
			return rectTransform.anchoredPosition;
		}
		set
		{
			rectTransform.anchoredPosition = value;
		}
	}

	public float x
	{
		get
		{
			return position.x;
		}
		set
		{
			Vector2 vector = position;
			vector.x = value;
			position = vector;
		}
	}

	public float y
	{
		get
		{
			return position.y;
		}
		set
		{
			Vector2 vector = position;
			vector.y = value;
			position = vector;
		}
	}

	public float rotation
	{
		get
		{
			return rectTransform.localEulerAngles.z;
		}
		set
		{
			Vector3 localEulerAngles = rectTransform.localEulerAngles;
			localEulerAngles.z = value;
			rectTransform.localEulerAngles = localEulerAngles;
		}
	}

	public Vector2 size
	{
		get
		{
			return rectTransform.sizeDelta;
		}
		set
		{
			rectTransform.sizeDelta = value;
		}
	}

	public float width
	{
		get
		{
			return size.x;
		}
		set
		{
			Vector2 vector = size;
			vector.x = value;
			size = vector;
		}
	}

	public float height
	{
		get
		{
			return size.y;
		}
		set
		{
			Vector2 vector = size;
			vector.y = value;
			size = vector;
		}
	}

	public bool visible
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			base.gameObject.SetActive(value);
		}
	}

	public float alpha
	{
		get
		{
			return m_alpha;
		}
		set
		{
			if ((bool)group)
			{
				group.alpha = (m_alpha = value);
				CanvasGroup canvasGroup = group;
				bool interactable = (group.blocksRaycasts = m_alpha > 0f);
				canvasGroup.interactable = interactable;
			}
			if ((bool)canvas)
			{
				canvas.enabled = alpha > 0f;
			}
			if ((bool)raycaster)
			{
				raycaster.enabled = alpha > 0f;
			}
			mouseEnabled = alpha >= 0f;
		}
	}

	public Canvas canvas
	{
		get
		{
			if (!m_canvas)
			{
				return m_canvas = GetComponent<Canvas>();
			}
			return m_canvas;
		}
	}

	public GraphicRaycaster raycaster
	{
		get
		{
			if (!m_raycaster)
			{
				return m_raycaster = GetComponent<GraphicRaycaster>();
			}
			return m_raycaster;
		}
	}

	public CanvasGroup group
	{
		get
		{
			if ((bool)m_group)
			{
				return m_group;
			}
			if (!this)
			{
				return m_group;
			}
			m_group = GetComponent<CanvasGroup>();
			m_alpha = m_group.alpha;
			return m_group;
		}
	}

	public RectTransform rectTransform
	{
		get
		{
			if (!m_rect_transform)
			{
				return m_rect_transform = GetComponent<RectTransform>();
			}
			return m_rect_transform;
		}
	}

	public Canvas mainCanvas
	{
		get
		{
			if ((bool)m_main_canvas)
			{
				return m_main_canvas;
			}
			return m_main_canvas = FindReverse<Canvas>();
		}
	}

	private void OnTransformParentChanged()
	{
		m_canvas = null;
	}
}
