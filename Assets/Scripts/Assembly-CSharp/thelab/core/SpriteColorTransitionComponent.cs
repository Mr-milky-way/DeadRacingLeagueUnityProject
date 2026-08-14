using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace thelab.core
{
	public class SpriteColorTransitionComponent : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		private Color[] m_target_colors;

		private bool selected;

		public Graphic[] targetsForColorChange;

		public Image targetForSpriteChange;

		public SpriteColorTransitionComponent[] onSelectClear;

		public Color normalColor = Color.white;

		public Color hilightColor = new Color(0.7f, 0.7f, 0.7f, 1f);

		public Color pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

		public Sprite normalSprint;

		public Sprite pressedSprite;

		public float duration = 0.1f;

		public bool multiply = true;

		protected void Awake()
		{
			m_target_colors = new Color[targetsForColorChange.Length];
			for (int i = 0; i < targetsForColorChange.Length; i++)
			{
				m_target_colors[i] = targetsForColorChange[i].color;
			}
			ApplyColor(normalColor, 0f);
		}

		private void ApplyColor(Color p_color, float p_duration)
		{
			for (int i = 0; i < targetsForColorChange.Length; i++)
			{
				Color color = (multiply ? (m_target_colors[i] * p_color) : p_color);
				if (p_duration <= 0f)
				{
					targetsForColorChange[i].color = color;
				}
				else
				{
					Tween.Add(targetsForColorChange[i], "color", color, 0f, p_duration, Cubic.Out);
				}
			}
		}

		private void ChangeSprite(Sprite toSprite)
		{
			if (!(targetForSpriteChange == null) && !(toSprite == null))
			{
				targetForSpriteChange.sprite = toSprite;
			}
		}

		private void ClearOthers()
		{
			for (int i = 0; i < onSelectClear.Length; i++)
			{
				onSelectClear[i].Clear();
			}
		}

		public void Clear()
		{
			selected = false;
			ApplyColor(normalColor, duration);
			ChangeSprite(normalSprint);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			ApplyColor(pressedColor, duration);
			ChangeSprite(pressedSprite);
			selected = true;
			ClearOthers();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			ApplyColor(hilightColor, duration);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!selected)
			{
				ApplyColor(normalColor, duration);
			}
		}
	}
}
