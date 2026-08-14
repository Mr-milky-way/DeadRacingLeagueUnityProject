using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class TutorialMask : MonoBehaviour
	{
		[SerializeField]
		private float m_x;

		[SerializeField]
		private float m_y;

		[SerializeField]
		private float m_width;

		[SerializeField]
		private float m_height;

		[SerializeField]
		private List<RectTransform> m_pieces;

		[SerializeField]
		internal bool m_init;

		public float x
		{
			get
			{
				return m_x;
			}
			set
			{
				m_x = value;
				Refresh();
			}
		}

		public float y
		{
			get
			{
				return m_y;
			}
			set
			{
				m_y = value;
				Refresh();
			}
		}

		public float width
		{
			get
			{
				return m_width;
			}
			set
			{
				m_width = value;
				Refresh();
			}
		}

		public float height
		{
			get
			{
				return m_height;
			}
			set
			{
				m_height = value;
				Refresh();
			}
		}

		private RectTransform m_center => m_pieces[2];

		internal void Init()
		{
			if (!m_init)
			{
				m_init = true;
				m_x = 0f;
				m_y = 0f;
				m_width = 100f;
				m_height = 100f;
				m_pieces = new List<RectTransform>();
				for (int i = 0; i < base.transform.childCount; i++)
				{
					m_pieces.Add(base.transform.GetChild(i).GetComponent<RectTransform>());
				}
				Refresh();
			}
		}

		public void Mask(RectTransform p_target, float p_size_scale = 1f)
		{
			if ((bool)p_target)
			{
				Transform parent = p_target.transform.parent;
				int siblingIndex = p_target.transform.GetSiblingIndex();
				p_target.SetParent(base.transform, worldPositionStays: true);
				Vector2 anchoredPosition = p_target.anchoredPosition;
				Vector2 sizeDelta = p_target.sizeDelta;
				Vector2 pivot = p_target.pivot;
				Vector2 vector = sizeDelta;
				vector.Scale(Vector2.one * p_size_scale);
				x = anchoredPosition.x - sizeDelta.x * pivot.x + sizeDelta.x * 0.5f;
				y = anchoredPosition.y - sizeDelta.y * pivot.y + sizeDelta.y * 0.5f;
				width = vector.x;
				height = vector.y;
				Debug.Log(anchoredPosition);
				p_target.SetParent(parent);
				p_target.SetSiblingIndex(siblingIndex);
			}
		}

		private void Refresh()
		{
			Rect rect = GetComponentInParent<RectTransform>().rect;
			float num = m_width * 0.5f;
			float num2 = m_height * 0.5f;
			float num3 = m_x - num;
			float num4 = m_y + num2;
			float num5 = Mathf.Max(0f, m_width);
			float num6 = Mathf.Max(0f, m_height);
			m_center.anchoredPosition = new Vector2(num3, num4);
			m_center.sizeDelta = new Vector2(num5, num6);
			float size = Mathf.Max(0f, 0f - num4);
			float size2 = Mathf.Max(0f, rect.height + num4 - num6);
			m_pieces[0].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
			RectTransform rectTransform = m_pieces[1];
			rectTransform.anchoredPosition = new Vector2(0f, num4);
			rectTransform.sizeDelta = new Vector2(Mathf.Max(0f, num3), num6);
			RectTransform rectTransform2 = m_pieces[3];
			rectTransform2.anchoredPosition = new Vector2(num3 + num5, num4);
			rectTransform2.sizeDelta = new Vector2(Mathf.Max(0f, rect.width - num3 - num5), num6);
			m_pieces[4].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size2);
		}
	}
}
