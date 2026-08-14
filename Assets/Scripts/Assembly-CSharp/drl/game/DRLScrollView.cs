using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class DRLScrollView : MonoBehaviour
	{
		public RectTransform container;

		public RectTransform viewrect;

		public float pageWidth;

		public int pageStep = 1;

		public int pageMargin;

		public int pageVisibleCount = 9;

		public float m_offsetXbyPage;

		[SerializeField]
		private Canvas m_canvas;

		[SerializeField]
		private int m_totalPages;

		private int m_currentPage;

		private Vector2 m_next_ap;

		public bool wrapAround;

		public bool isAnimated;

		public bool fadeAroundEdges;

		public AnimationCurve animationCurve;

		public float animationTime;

		private float m_percentage;

		private Vector2 m_currentAnchor;

		private Dictionary<int, Activity> m_viewport_fade_loops;

		private Activity m_viewport_fade_loop;

		public float offsetXbyPage
		{
			get
			{
				return m_offsetXbyPage;
			}
			set
			{
				m_offsetXbyPage = value;
			}
		}

		public Canvas canvas
		{
			get
			{
				if (!m_canvas)
				{
					return m_canvas = Hierarchy.FindReverse<Canvas>(base.transform);
				}
				return m_canvas;
			}
		}

		public int totalPages => m_totalPages;

		public int currentPage => m_currentPage;

		protected void Awake()
		{
			m_viewport_fade_loops = new Dictionary<int, Activity>();
		}

		public void SetPages(int p_total_pages)
		{
			m_totalPages = p_total_pages;
			m_next_ap = Vector2.zero;
			m_percentage = 0f;
			m_currentPage = 0;
			m_currentAnchor = Vector2.zero;
			RefreshElementsBoundingVisible(p_force: true);
		}

		public void RefreshPage(int p_page)
		{
			if (0 <= p_page && p_page < m_totalPages)
			{
				m_next_ap = new Vector2((0f - (pageWidth + offsetXbyPage)) * (float)p_page, 0f);
				m_percentage = 0f;
				m_currentPage = p_page;
				m_currentAnchor = container.anchoredPosition;
				RefreshElementsBoundingVisible();
			}
		}

		public bool NextPage()
		{
			int num = Mathf.Max(0, m_totalPages - pageMargin);
			if (num <= 0)
			{
				return false;
			}
			if (m_currentPage >= totalPages)
			{
				return false;
			}
			int num2 = m_currentPage + pageStep;
			num2 = ((!wrapAround) ? Mathf.Min(num, num2) : ((num2 < num) ? num2 : 0));
			RefreshPage(num2);
			return true;
		}

		public bool PreviousPage()
		{
			int num = Mathf.Max(0, m_totalPages - pageMargin);
			if (num <= 0)
			{
				return false;
			}
			if (m_currentPage <= 0)
			{
				return false;
			}
			int num2 = m_currentPage - pageStep;
			num2 = ((!wrapAround) ? Mathf.Max(0, num2) : ((num2 <= num) ? (m_totalPages - 1) : num2));
			RefreshPage(num2);
			return true;
		}

		protected void RefreshElementsBoundingVisible(bool p_force = false)
		{
			if (!fadeAroundEdges || !container)
			{
				return;
			}
			ListComponent l = Hierarchy.GetComponent<ListComponent>(container.gameObject);
			int p0 = currentPage;
			int p1 = p0 + pageStep;
			int ipp = pageVisibleCount;
			if (m_viewport_fade_loop != null)
			{
				m_viewport_fade_loop.Stop();
			}
			float progress = 0f;
			Activity viewport_fade_loop = Activity.Run(delegate(float t)
			{
				progress = (p_force ? 1f : Mathf.Clamp01(t / animationTime));
				for (int i = 0; i < l.Count; i++)
				{
					CanvasGroup canvasGroup = l.Get<CanvasGroup>(i);
					if ((bool)canvasGroup)
					{
						int num = i / ipp;
						bool flag = num >= p0 && num < p1;
						if (flag || (num >= p0 - 1 && num <= p1 + 1))
						{
							float b = (flag ? 1f : 0.25f);
							canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, b, progress);
							bool blocksRaycasts = (canvasGroup.interactable = flag);
							canvasGroup.blocksRaycasts = blocksRaycasts;
						}
					}
				}
				return progress < 1f;
			});
			m_viewport_fade_loop = viewport_fade_loop;
		}

		protected void Update()
		{
			if (isAnimated)
			{
				if (m_percentage < 1f)
				{
					m_percentage += Time.unscaledDeltaTime / animationTime;
					m_percentage = Mathf.Clamp01(m_percentage);
					container.anchoredPosition = Vector2.Lerp(m_currentAnchor, m_next_ap, animationCurve.Evaluate(m_percentage));
				}
			}
			else
			{
				container.anchoredPosition = m_next_ap;
			}
		}
	}
}
