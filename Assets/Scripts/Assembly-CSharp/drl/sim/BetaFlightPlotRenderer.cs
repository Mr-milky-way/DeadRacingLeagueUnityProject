using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.sim
{
	public class BetaFlightPlotRenderer : MonoBehaviour
	{
		public PlotRenderer[] renderers;

		public PlotRenderer throttle;

		public RectTransform grid;

		public Text[] fields;

		private List<float> m_range = new List<float>();

		private float m_threshold = 2000f;

		public float Threshold
		{
			get
			{
				if (m_range.Count < 1)
				{
					return 510f;
				}
				float num = m_range[0];
				for (int i = 1; i < m_range.Count; i++)
				{
					num = Mathf.Max(num, m_range[i]);
				}
				return Mathf.Ceil((num - 10f) / 500f) * 500f + 10f;
			}
		}

		public void Plot(int p_id, float p_rc_rate, float p_super_rate, float p_expo, string p_unit)
		{
			while (m_range.Count <= p_id)
			{
				m_range.Add(0f);
			}
			PlotRenderer plotRenderer = Reflection<object>.Get(renderers, p_id);
			if ((bool)plotRenderer)
			{
				Text text = Reflection<object>.Get(fields, p_id);
				float min = BetaflightRates.GetMin(p_super_rate, p_rc_rate, p_expo);
				float max = BetaflightRates.GetMax(p_super_rate, p_rc_rate, p_expo);
				float value = Mathf.Max(Mathf.Abs(min), Mathf.Abs(max));
				m_range[p_id] = value;
				if (m_threshold != Threshold)
				{
					UpdateThresholds(Threshold);
				}
				float num = plotRenderer.count - 1;
				float num2 = 0f;
				float num3 = ((num <= 0f) ? 0f : (1f / num));
				for (int i = 0; i < plotRenderer.count; i++)
				{
					float num4 = Mathf.Lerp(-1f, 1f, num2);
					float rate = BetaflightRates.GetRate(num4, p_super_rate, p_rc_rate, p_expo);
					plotRenderer.Plot(i, num4, rate, 0f);
					num2 += num3;
				}
				if (!string.IsNullOrEmpty(p_unit) && (bool)text)
				{
					text.text = Mathf.Round(max) + " " + p_unit;
				}
				plotRenderer.ResetCanvas();
			}
		}

		public void PlotThrottle(float p_mid, float p_expo)
		{
			throttle.SetRanges(0f, 1f, 1f, 0f);
			float num = throttle.count - 1;
			float num2 = 0f;
			float num3 = ((num <= 0f) ? 0f : (1f / num));
			for (int i = 0; i < throttle.count; i++)
			{
				float num4 = Mathf.Lerp(0f, 1f, num2);
				float p_y = BetaflightRates.GetThrottle(num4, p_expo, p_mid);
				throttle.Plot(i, num4, p_y, 0f);
				num2 += num3;
			}
			throttle.ResetCanvas();
		}

		private void UpdateThresholds(float p_threshold)
		{
			if (!(p_threshold < 510f))
			{
				m_threshold = p_threshold;
				for (int i = 0; i < renderers.Length; i++)
				{
					Tween.Add(renderers[i], "SymetricLimit", p_threshold, 0.4f, Cubic.Out);
				}
				Tween.Add(grid, "localScale", new Vector3(1f, 500f / p_threshold, 1f), 0.4f, Cubic.Out);
			}
		}
	}
}
