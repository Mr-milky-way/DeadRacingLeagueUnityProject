using UnityEngine;

namespace drl.game
{
	public class SegmentStripeRenderer : MonoBehaviour
	{
		private LineRenderer m_renderer;

		[SerializeField]
		private Transform m_from;

		[SerializeField]
		private Transform m_to;

		[SerializeField]
		private Color[] m_colors = new Color[2]
		{
			Color.black,
			Color.white
		};

		[SerializeField]
		private float m_interval = 1f;

		private bool m_dirty;

		private Vector3[] m_positions;

		[SerializeField]
		private Texture2D m_pattern;

		private Color[] m_pattern_colors;

		public LineRenderer renderer
		{
			get
			{
				if (!m_renderer)
				{
					return m_renderer = GetComponent<LineRenderer>();
				}
				return m_renderer;
			}
		}

		public Transform from
		{
			get
			{
				return m_from;
			}
			set
			{
				m_from = value;
				Refresh();
			}
		}

		public Transform to
		{
			get
			{
				return m_to;
			}
			set
			{
				m_to = value;
				Refresh();
			}
		}

		public Color[] colors
		{
			get
			{
				return m_colors;
			}
			set
			{
				m_colors = value;
				Refresh();
			}
		}

		public float interval
		{
			get
			{
				return m_interval;
			}
			set
			{
				m_interval = value;
				Refresh();
			}
		}

		protected void Awake()
		{
			m_dirty = true;
		}

		protected void OnTransformChildrenChanged()
		{
			m_dirty = true;
		}

		protected void LateUpdate()
		{
			if (m_dirty)
			{
				Refresh();
				m_dirty = false;
			}
		}

		public void Refresh()
		{
			LineRenderer lineRenderer = renderer;
			if (!lineRenderer)
			{
				lineRenderer.positionCount = 0;
				return;
			}
			if (!to)
			{
				lineRenderer.positionCount = 0;
				return;
			}
			if (!from)
			{
				lineRenderer.positionCount = 0;
				return;
			}
			int num = 20;
			if (m_positions == null)
			{
				m_positions = new Vector3[0];
			}
			if (m_positions.Length != num)
			{
				m_positions = new Vector3[num];
			}
			Vector3 position = from.position;
			Vector3 position2 = to.position;
			for (int i = 0; i < num; i++)
			{
				float t = (float)i / (float)(num - 1);
				m_positions[i] = Vector3.Lerp(position, position2, t);
			}
			if (m_positions.Length != lineRenderer.positionCount)
			{
				lineRenderer.positionCount = m_positions.Length;
			}
			lineRenderer.SetPositions(m_positions);
			Material sharedMaterial = lineRenderer.sharedMaterial;
			if (!m_pattern)
			{
				m_pattern = new Texture2D(2048, 1, TextureFormat.ARGB32, mipChain: false);
				m_pattern.hideFlags = HideFlags.HideAndDontSave;
				m_pattern.wrapMode = TextureWrapMode.Repeat;
				m_pattern.filterMode = FilterMode.Point;
				sharedMaterial.SetTexture("_MainTex", m_pattern);
			}
			if (m_colors == null)
			{
				m_colors = new Color[0];
			}
			if (m_colors.Length == 0)
			{
				m_colors = new Color[2]
				{
					Color.black,
					Color.white
				};
			}
			float num2 = Vector3.Distance(position, position2);
			sharedMaterial.SetTextureScale("_MainTex", new Vector2(1f / Mathf.Max(num2, 1E-05f), 1f));
			float num3 = Mathf.Max(interval, 0.001f);
			num2 /= num3;
			if ((bool)m_pattern)
			{
				if (m_pattern_colors == null)
				{
					m_pattern_colors = new Color[0];
				}
				if (m_pattern_colors.Length != m_pattern.width)
				{
					m_pattern_colors = new Color[m_pattern.width];
				}
				float a = 1f / (float)m_pattern.width;
				for (int j = 0; j < m_pattern_colors.Length; j++)
				{
					float num4 = (float)j / (float)(m_pattern_colors.Length - 1);
					float num5 = 1f;
					num5 = num4;
					num5 = Mathf.Clamp01(num5);
					num5 = Mathf.Max(a, num5);
					num5 *= num2;
					int num6 = Mathf.FloorToInt(num5);
					Color color = m_colors[num6 % m_colors.Length];
					m_pattern_colors[j] = color;
				}
				m_pattern.SetPixels(m_pattern_colors);
				m_pattern.Apply();
			}
		}
	}
}
