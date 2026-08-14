using UnityEngine;

namespace thelab.core
{
	public class PlotRenderer : MonoBehaviour
	{
		[SerializeField]
		private LineRenderer m_renderer;

		[SerializeField]
		private RectTransform m_rectTransform;

		public Rect ranges = new Rect(0f, 0f, 1f, 1f);

		public Rect canvas = new Rect(0f, 0f, 100f, 100f);

		public int count = 100;

		private Vector3[] m_positions;

		private Vector3[] m_buffer;

		protected bool m_dirty;

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

		public RectTransform rectTransform
		{
			get
			{
				if (!m_rectTransform)
				{
					return m_rectTransform = GetComponent<RectTransform>();
				}
				return m_rectTransform;
			}
		}

		public float alpha
		{
			get
			{
				LineRenderer lineRenderer = renderer;
				if (!lineRenderer)
				{
					return 0f;
				}
				return Mathf.Max(lineRenderer.startColor.a, lineRenderer.endColor.a);
			}
			set
			{
				LineRenderer lineRenderer = renderer;
				if ((bool)lineRenderer)
				{
					Color startColor = lineRenderer.startColor;
					startColor.a = value;
					lineRenderer.startColor = startColor;
					startColor = lineRenderer.endColor;
					startColor.a = value;
					lineRenderer.endColor = startColor;
				}
			}
		}

		public Vector3[] positions
		{
			get
			{
				LineRenderer lineRenderer = renderer;
				if (!lineRenderer)
				{
					return m_positions = new Vector3[0];
				}
				if (lineRenderer.positionCount == count && m_positions != null && m_positions.Length == count && m_buffer != null && m_buffer.Length == count)
				{
					return m_positions;
				}
				Vector3[] array = m_positions;
				m_positions = new Vector3[count];
				m_buffer = new Vector3[count];
				int num = ((array != null) ? Mathf.Min(array.Length, m_positions.Length) : 0);
				for (int i = 0; i < num; i++)
				{
					m_positions[i] = array[i];
				}
				lineRenderer.positionCount = count;
				return m_positions;
			}
		}

		public float SymetricLimit
		{
			get
			{
				return ranges.yMax;
			}
			set
			{
				ranges.xMin = -1f;
				ranges.xMax = 1f;
				ranges.yMin = 0f - value;
				ranges.yMax = value;
				m_dirty = true;
			}
		}

		public void Fade(float p_transition, float p_duration, float p_delay)
		{
			Tween.Kill(this);
			if (p_duration <= 0f)
			{
				alpha = p_transition;
			}
			else
			{
				Tween.Add(this, "alpha", p_transition, p_duration, p_delay, Cubic.Out);
			}
		}

		public void Fade(float p_transition, float p_duration)
		{
			Fade(p_transition, p_duration, 0f);
		}

		public void Fade(float p_transition)
		{
			Fade(p_transition, 0.5f, 0f);
		}

		public void SetRanges(float p_xmin, float p_xmax, float p_ymin, float p_ymax)
		{
			ranges.xMin = Mathf.Min(p_xmin, p_xmax);
			ranges.xMax = Mathf.Max(p_xmin, p_xmax);
			ranges.yMin = Mathf.Min(p_ymin, p_ymax);
			ranges.yMax = Mathf.Max(p_ymin, p_ymax);
			m_dirty = true;
		}

		public void SetCanvas(float p_x, float p_y, float p_width, float p_height)
		{
			canvas = new Rect(p_x, p_y, p_width, p_height);
			m_dirty = true;
		}

		public void SetCanvas(float p_width, float p_height)
		{
			SetCanvas(0f, 0f, p_width, p_height);
		}

		public void ResetCanvas()
		{
			Rect rect = rectTransform.rect;
			SetCanvas((0f - rect.width) / 2f, (0f - rect.height) / 2f, rect.width, rect.height);
		}

		public void Plot(float p_x, float p_y, float p_z)
		{
			float num = positions.Length;
			if (!(num <= 0f))
			{
				float xMin = ranges.xMin;
				float num2 = ranges.xMax - xMin;
				num2 = ((num2 <= 0f) ? 0f : (1f / num2));
				int p_pos = (int)(Mathf.Clamp01((p_x - xMin) * num2) * (num - 1f));
				Plot(p_pos, p_x, p_y, p_z);
			}
		}

		public void Plot(int p_pos, float p_x, float p_y, float p_z)
		{
			Vector3[] array = positions;
			if (array.Length != 0)
			{
				int num = ((p_pos >= 0) ? ((p_pos >= array.Length) ? (array.Length - 1) : p_pos) : 0);
				Vector3 vector = array[num];
				vector.x = p_x;
				vector.y = p_y;
				vector.z = p_z;
				array[num] = vector;
				m_dirty = true;
			}
		}

		public void Draw()
		{
			LineRenderer lineRenderer = renderer;
			if ((bool)lineRenderer && count > 1)
			{
				Vector3[] array = positions;
				Vector3 vector = default(Vector3);
				float yMin = ranges.yMin;
				float num = ranges.yMax - yMin;
				num = ((num <= 0f) ? 0f : (1f / num));
				float xMin = ranges.xMin;
				float num2 = ranges.xMax - xMin;
				num2 = ((num2 <= 0f) ? 0f : (1f / num2));
				Vector3[] buffer = m_buffer;
				int num3 = Mathf.Min(buffer.Length, array.Length, count);
				for (int i = 0; i < num3; i++)
				{
					vector = array[i];
					vector.x = Mathf.Clamp01((vector.x - xMin) * num2);
					vector.y = Mathf.Clamp01((vector.y - yMin) * num);
					vector.x = canvas.xMin + canvas.width * vector.x;
					vector.y = canvas.yMin + canvas.height * vector.y;
					buffer[i] = vector;
				}
				lineRenderer.SetPositions(buffer);
			}
		}

		protected void LateUpdate()
		{
			if ((bool)base.gameObject && base.gameObject.activeInHierarchy && base.enabled && m_dirty)
			{
				Draw();
				m_dirty = false;
			}
		}
	}
}
