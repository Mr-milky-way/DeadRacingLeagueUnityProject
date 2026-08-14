using UnityEngine;

namespace thelab.core
{
	public class LineGradientSlider : MonoBehaviour
	{
		[SerializeField]
		private Renderer m_renderer;

		public Gradient gradient;

		public bool clamp = true;

		public Vector3 alpha = new Vector3(0f, 1f, 0f);

		public Vector2 alphaOffset = new Vector2(0.02f, 0.02f);

		public Color colorLeft = Color.red;

		public Color colorRight = Color.blue;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_ratio;

		private GradientAlphaKey[] m_gak;

		private GradientColorKey[] m_gck;

		public Renderer renderer
		{
			get
			{
				return m_renderer;
			}
			set
			{
				m_renderer = value;
				Refresh();
			}
		}

		public float ratio
		{
			get
			{
				return m_ratio;
			}
			set
			{
				if (!(Mathf.Abs(m_ratio - value) <= 0.001f))
				{
					SetRatio(value);
				}
			}
		}

		protected void Awake()
		{
			ratio = m_ratio;
			Refresh();
		}

		protected void Start()
		{
		}

		public void SetRatio(float v)
		{
			m_ratio = v;
			Refresh();
		}

		internal void Refresh()
		{
			if (!base.enabled)
			{
				return;
			}
			if (m_gak == null)
			{
				m_gak = new GradientAlphaKey[3];
			}
			if (m_gck == null)
			{
				m_gck = new GradientColorKey[2];
			}
			float num = m_ratio;
			if (!clamp)
			{
				num = Mathf.Lerp(0f - alphaOffset.y, 1f + alphaOffset.x, num);
			}
			m_gak[0].alpha = alpha.x;
			m_gak[1].alpha = alpha.y;
			m_gak[2].alpha = alpha.z;
			m_gak[0].time = Mathf.Max(0f, num - alphaOffset.x);
			m_gak[1].time = num;
			m_gak[2].time = Mathf.Min(1f, num + alphaOffset.y);
			m_gck[0].time = num - 0.001f;
			m_gck[1].time = num + 0.001f;
			m_gck[0].color = colorLeft;
			m_gck[1].color = colorRight;
			if (gradient == null)
			{
				gradient = new Gradient();
			}
			gradient.SetKeys(m_gck, m_gak);
			if ((bool)renderer)
			{
				LineRenderer lineRenderer = ((renderer is LineRenderer) ? (renderer as LineRenderer) : null);
				TrailRenderer trailRenderer = ((renderer is TrailRenderer) ? (renderer as TrailRenderer) : null);
				if ((bool)trailRenderer)
				{
					trailRenderer.colorGradient = gradient;
				}
				if ((bool)lineRenderer)
				{
					lineRenderer.colorGradient = gradient;
				}
			}
		}
	}
}
