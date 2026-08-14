using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class FocusResize : MonoBehaviour, IFocusHandler
	{
		[SerializeField]
		private LayoutGroup m_layout;

		private float m_transition;

		public Vector2 min;

		public Vector2 max;

		public float duration;

		public LayoutGroup layout
		{
			get
			{
				if (!m_layout)
				{
					return m_layout = Hierarchy.FindReverse<LayoutGroup>(base.transform);
				}
				return m_layout;
			}
		}

		public float transition
		{
			get
			{
				return m_transition;
			}
			set
			{
				float num = Mathf.Clamp01(value);
				m_transition = num;
				Refresh();
			}
		}

		protected void Start()
		{
		}

		public void OnFocus()
		{
			if (base.enabled)
			{
				Tween.Kill(this);
				Tween.Add(this, "transition", 1f, duration, Cubic.Out);
			}
		}

		public void OnUnfocus()
		{
			if (base.enabled)
			{
				Tween.Kill(this);
				Tween.Add(this, "transition", 0f, duration, Cubic.Out);
			}
		}

		private void Refresh()
		{
			Vector2 sizeDelta = Vector2.Lerp(min, max, transition);
			((RectTransform)base.transform).sizeDelta = sizeDelta;
			if ((bool)layout)
			{
				Reflection<object>.Invoke(layout, "SetDirty");
			}
		}
	}
}
