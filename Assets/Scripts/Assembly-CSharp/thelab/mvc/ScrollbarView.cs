using UnityEngine;
using UnityEngine.UI;

namespace thelab.mvc
{
	[ExecuteInEditMode]
	public class ScrollbarView : UIElementView
	{
		public Scrollbar scrollbar;

		public RectTransform content;

		public RectTransform viewport;

		public float minSize = 0.05f;

		public float maxSize = 0.95f;

		public float offsetRatio
		{
			get
			{
				if (!scrollbar)
				{
					return 0f;
				}
				if (!viewport)
				{
					return 0f;
				}
				if (!content)
				{
					return 0f;
				}
				int num;
				float f;
				if (scrollbar.direction != Scrollbar.Direction.TopToBottom)
				{
					num = ((scrollbar.direction == Scrollbar.Direction.BottomToTop) ? 1 : 0);
					if (num == 0)
					{
						f = viewport.sizeDelta.x;
						goto IL_007d;
					}
				}
				else
				{
					num = 1;
				}
				f = viewport.sizeDelta.y;
				goto IL_007d;
				IL_007d:
				float num2 = Mathf.Abs(f);
				float num3 = Mathf.Abs((num != 0) ? content.sizeDelta.y : content.sizeDelta.x);
				if (!(num2 <= 0f))
				{
					return num3 / num2;
				}
				return 0f;
			}
		}

		public float offset
		{
			get
			{
				int num;
				float f;
				if (scrollbar.direction != Scrollbar.Direction.TopToBottom)
				{
					num = ((scrollbar.direction == Scrollbar.Direction.BottomToTop) ? 1 : 0);
					if (num == 0)
					{
						f = viewport.sizeDelta.x;
						goto IL_0044;
					}
				}
				else
				{
					num = 1;
				}
				f = viewport.sizeDelta.y;
				goto IL_0044;
				IL_0044:
				float num2 = Mathf.Abs(f);
				float num3 = Mathf.Abs((num != 0) ? content.sizeDelta.y : content.sizeDelta.x);
				return Mathf.Max(0f, num2 - num3);
			}
		}

		protected void Awake()
		{
			scrollbar = GetComponent<Scrollbar>();
			if ((bool)scrollbar)
			{
				scrollbar.onValueChanged.AddListener(OnChange);
			}
		}

		protected virtual void OnChange(float v)
		{
			if ((bool)content)
			{
				Vector2 anchoredPosition = content.anchoredPosition;
				anchoredPosition.x = scrollbar.value * offset;
			}
			Notify(notification + "@change");
		}

		protected override void OnState(string p_state)
		{
			if (p_state != null)
			{
				_ = p_state == "scroll";
			}
		}

		private void LateUpdate()
		{
			if (!content || !viewport)
			{
				return;
			}
			if (!scrollbar)
			{
				scrollbar = GetComponent<Scrollbar>();
			}
			if ((bool)scrollbar)
			{
				if (!Application.isPlaying && (bool)content)
				{
					Vector2 anchoredPosition = content.anchoredPosition;
					anchoredPosition.x = scrollbar.value * offset;
				}
				bool flag = !Application.isPlaying;
				if (content.hasChanged)
				{
					flag = true;
				}
				if (viewport.hasChanged)
				{
					flag = true;
				}
				if (flag)
				{
					float t = ((offsetRatio <= 0f) ? 0f : (1f / offsetRatio));
					scrollbar.size = Mathf.Lerp(minSize, maxSize, t);
				}
			}
		}
	}
}
