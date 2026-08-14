using UnityEngine;

namespace thelab.core
{
	[RequireComponent(typeof(LineRenderer))]
	public class SplineRenderer : MonoBehaviour
	{
		[SerializeField]
		private SplineComponent m_spline;

		[SerializeField]
		private LineRenderer m_renderer;

		private int m_refresh_idx;

		public SplineComponent spline
		{
			get
			{
				if (!this)
				{
					return null;
				}
				if ((bool)m_spline)
				{
					return m_spline;
				}
				return m_spline = GetComponent<SplineComponent>();
			}
			set
			{
				m_spline = value;
				Refresh();
			}
		}

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

		public Color color
		{
			get
			{
				Material material = (renderer ? renderer.sharedMaterial : null);
				if (!material)
				{
					return Color.clear;
				}
				if (material.HasProperty("_Color"))
				{
					return material.GetColor("_Color");
				}
				if (material.HasProperty("_Tint"))
				{
					return material.GetColor("_Tint");
				}
				if (material.HasProperty("_TintColor"))
				{
					return material.GetColor("_TintColor");
				}
				return Color.clear;
			}
			set
			{
				Material material = (renderer ? renderer.sharedMaterial : null);
				if ((bool)material)
				{
					if (material.HasProperty("_Color"))
					{
						material.SetColor("_Color", value);
					}
					else if (material.HasProperty("_Tint"))
					{
						material.SetColor("_Tint", value);
					}
					else if (material.HasProperty("_TintColor"))
					{
						material.SetColor("_TintColor", value);
					}
				}
			}
		}

		public float alpha
		{
			get
			{
				return color.a;
			}
			set
			{
				Color color = this.color;
				color.a = Mathf.Clamp01(value);
				this.color = color;
			}
		}

		public void SetGradientColors(int p_start, params Color[] p_colors)
		{
			if (!renderer)
			{
				return;
			}
			LineRenderer lineRenderer = renderer;
			Gradient colorGradient = lineRenderer.colorGradient;
			GradientColorKey[] colorKeys = colorGradient.colorKeys;
			GradientAlphaKey[] alphaKeys = colorGradient.alphaKeys;
			int num = Mathf.Min(p_colors.Length, colorKeys.Length);
			for (int i = p_start; i < num; i++)
			{
				if (i >= 0)
				{
					if (i >= num)
					{
						break;
					}
					GradientColorKey gradientColorKey = colorKeys[i];
					gradientColorKey.color = p_colors[i];
					colorKeys[i] = gradientColorKey;
				}
			}
			colorGradient.SetKeys(colorKeys, alphaKeys);
			lineRenderer.colorGradient = colorGradient;
		}

		public void SetWidth(float p_start, float p_end, float p_thickness)
		{
			LineRenderer lineRenderer = renderer;
			if ((bool)lineRenderer)
			{
				lineRenderer.widthMultiplier = p_thickness;
				lineRenderer.startWidth = p_start;
				lineRenderer.endWidth = p_end;
			}
		}

		public void Clear()
		{
			renderer.positionCount = 0;
		}

		public void Refresh()
		{
			LineRenderer lineRenderer = renderer;
			if (!lineRenderer)
			{
				return;
			}
			lineRenderer.positionCount = 0;
			if ((bool)spline)
			{
				int num = spline.positions.values.Length * 60;
				if (lineRenderer.positionCount != num)
				{
					lineRenderer.positionCount = num;
				}
				float num2 = ((num <= 0) ? 0f : (1f / (float)(num - 1)));
				float num3 = 0f;
				for (int i = 0; i < num; i++)
				{
					lineRenderer.SetPosition(i, spline.positions.GetNormalized(num3));
					num3 += num2;
				}
			}
		}

		private void LateUpdate()
		{
			if (!base.enabled || !base.gameObject || !spline)
			{
				return;
			}
			bool flag = false;
			Transform transform = base.transform;
			if (transform.childCount > 0)
			{
				for (int i = 0; i < 10; i++)
				{
					if (m_refresh_idx < 0)
					{
						break;
					}
					if (m_refresh_idx >= transform.childCount)
					{
						break;
					}
					if (transform.GetChild(m_refresh_idx).hasChanged)
					{
						flag = true;
						break;
					}
					m_refresh_idx = (m_refresh_idx + 1) % transform.childCount;
				}
			}
			if (flag)
			{
				Refresh();
				spline.hasChanged = false;
			}
		}
	}
}
