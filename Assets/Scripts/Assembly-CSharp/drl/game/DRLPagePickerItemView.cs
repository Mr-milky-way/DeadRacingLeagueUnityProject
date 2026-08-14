using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLPagePickerItemView : UIElementView
	{
		public Text labelField;

		public Color hilight = Color.white;

		public Color normal = Colorf.RGBAToColor(1583243007u);

		public float duration;

		private DRLPagePickerView m_parent;

		[SerializeField]
		private bool m_selected;

		public string label
		{
			get
			{
				return labelField.text;
			}
			set
			{
				labelField.text = value;
			}
		}

		public float width
		{
			get
			{
				return ((RectTransform)base.transform).sizeDelta.x;
			}
			set
			{
				Vector2 sizeDelta = ((RectTransform)base.transform).sizeDelta;
				sizeDelta.x = value;
				((RectTransform)base.transform).sizeDelta = sizeDelta;
			}
		}

		public float height
		{
			get
			{
				return ((RectTransform)base.transform).sizeDelta.y;
			}
			set
			{
				Vector2 sizeDelta = ((RectTransform)base.transform).sizeDelta;
				sizeDelta.y = value;
				((RectTransform)base.transform).sizeDelta = sizeDelta;
			}
		}

		public Vector2 size
		{
			get
			{
				return new Vector2(width, height);
			}
			set
			{
				width = value.x;
				height = value.y;
			}
		}

		public DRLPagePickerView parent
		{
			get
			{
				if (!m_parent)
				{
					return m_parent = Hierarchy.FindReverse<DRLPagePickerView>(base.transform);
				}
				return m_parent;
			}
		}

		public bool selected
		{
			get
			{
				return m_selected;
			}
			set
			{
				m_selected = value;
				Color color = (m_selected ? hilight : normal);
				if (duration <= 0f)
				{
					labelField.color = color;
				}
				else
				{
					Tween.Add(labelField, "color", color, duration, 0f, Cubic.Out);
				}
			}
		}

		protected void Awake()
		{
			labelField.color = (m_selected ? hilight : normal);
		}

		protected override void OnState(string p_state)
		{
			base.OnState(p_state);
			if (p_state != null && p_state == "lclick" && (bool)parent)
			{
				parent.OnItemClick(this);
			}
		}

		public override void OnFocus()
		{
			base.OnFocus();
			if ((bool)parent)
			{
				parent.OnItemFocus(this);
			}
		}

		public override void OnUnfocus()
		{
			base.OnUnfocus();
			if ((bool)parent)
			{
				parent.OnItemUnfocus(this);
			}
		}
	}
}
