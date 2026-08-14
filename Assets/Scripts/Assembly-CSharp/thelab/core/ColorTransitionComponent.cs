using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace thelab.core
{
	public class ColorTransitionComponent : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerClickHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IFocusHandler
	{
		public Selectable widget;

		public Graphic[] targets;

		private Color[] m_target_colors;

		public Color normalColor = Color.white;

		public Color hilightColor = new Color(0.7f, 0.7f, 0.7f, 1f);

		public Color pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

		public Color disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);

		public float duration = 0.1f;

		public bool multiply = true;

		private Color m_current_color;

		protected void Awake()
		{
			if (!widget)
			{
				widget = GetComponent<Selectable>();
			}
			m_target_colors = new Color[targets.Length];
			m_current_color = normalColor;
			for (int i = 0; i < targets.Length; i++)
			{
				if ((bool)targets[i])
				{
					m_target_colors[i] = targets[i].color;
				}
			}
		}

		protected void Start()
		{
			Invoke("Refresh", 1f / 60f);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			ApplyColor(pressedColor, duration);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			ApplyColor(hilightColor, duration);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			ApplyColor(normalColor, duration);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			ApplyColor(hilightColor, duration);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			Color p_color = normalColor;
			if ((bool)UINavigation.focus && UINavigation.focus.gameObject == base.gameObject)
			{
				p_color = hilightColor;
			}
			ApplyColor(p_color, duration);
		}

		public void OnSelect(BaseEventData eventData)
		{
			ApplyColor(hilightColor, duration);
		}

		public void OnDeselect(BaseEventData eventData)
		{
			ApplyColor(normalColor, duration);
		}

		public void OnEnable()
		{
			Refresh();
		}

		public void OnDisable()
		{
			Refresh();
		}

		public void Refresh()
		{
			bool flag = !widget || widget.interactable;
			if (!base.enabled)
			{
				flag = false;
			}
			ApplyColor(flag ? m_current_color : disabledColor, 0f, p_force: true);
		}

		private void ApplyColor(Color p_color, float p_duration, bool p_force = false)
		{
			if (!base.enabled)
			{
				return;
			}
			if (m_target_colors == null)
			{
				m_target_colors = new Color[0];
			}
			bool flag = !widget || widget.interactable;
			if (!p_force && (bool)widget && !flag)
			{
				return;
			}
			m_current_color = p_color;
			for (int i = 0; i < targets.Length; i++)
			{
				if (i >= m_target_colors.Length)
				{
					continue;
				}
				Color color = (multiply ? (m_target_colors[i] * p_color) : p_color);
				if (p_duration <= 0f)
				{
					if ((bool)targets[i])
					{
						targets[i].color = color;
					}
				}
				else if ((bool)targets[i])
				{
					Tween.Kill(targets[i]);
					Tween.Add(targets[i], "color", color, p_duration, 0f, Cubic.Out);
				}
			}
		}

		public void OnFocus()
		{
			ApplyColor(hilightColor, duration);
		}

		public void OnUnfocus()
		{
			ApplyColor(normalColor, duration);
		}
	}
}
