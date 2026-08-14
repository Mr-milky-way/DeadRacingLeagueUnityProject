using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab
{
	public static class AnimationCurveExtension
	{
		public class AnimationCurveLUT
		{
			public AnimationCurve curve;

			public int samples;

			public float[] values;

			public float min;

			public float max;

			public AnimationCurveLUT(AnimationCurve p_curve, int p_samples)
			{
				curve = p_curve;
				if (curve != null)
				{
					samples = p_samples;
					values = new float[p_samples];
					Keyframe[] keys = curve.keys;
					min = ((keys.Length == 0) ? 0f : keys[0].time);
					max = ((keys.Length == 0) ? 0f : keys[0].time);
					for (int i = 1; i < keys.Length; i++)
					{
						min = Mathf.Min(keys[i].time, min);
						max = Mathf.Max(keys[i].time, max);
					}
					for (int j = 0; j < p_samples; j++)
					{
						values[j] = p_curve.Evaluate(Mathf.Lerp(min, max, (float)j / (float)p_samples));
					}
				}
			}

			public float Evaluate(float p_time)
			{
				if (values.Length == 0)
				{
					return 0f;
				}
				float num = max - min;
				float num2 = ((num <= 0f) ? 0f : ((p_time - min) / num));
				float num3 = values.Length;
				int num4 = Mathf.FloorToInt(num2 * (num3 - 1f));
				if (num4 < 0)
				{
					return values[0];
				}
				if (num4 >= values.Length)
				{
					return values[values.Length - 1];
				}
				return values[num4];
			}

			public void Clear()
			{
				Array.Resize(ref values, 0);
				values = null;
				curve = null;
			}
		}

		private static Dictionary<AnimationCurve, AnimationCurveLUT> m_cache;

		public static Dictionary<AnimationCurve, AnimationCurveLUT> cache
		{
			get
			{
				if (m_cache != null)
				{
					return m_cache;
				}
				return m_cache = new Dictionary<AnimationCurve, AnimationCurveLUT>();
			}
		}

		public static float Evaluate(this AnimationCurve p_curve, float p_time, bool p_cached)
		{
			if (!p_cached)
			{
				return p_curve.Evaluate(float.IsNaN(p_time) ? 0f : p_time);
			}
			return (cache.ContainsKey(p_curve) ? cache[p_curve] : null)?.Evaluate(float.IsNaN(p_time) ? 0f : p_time) ?? p_curve.Evaluate(float.IsNaN(p_time) ? 0f : p_time);
		}

		public static AnimationCurveLUT Cache(this AnimationCurve p_curve, int p_samples, bool p_force = true)
		{
			if (p_force || !cache.ContainsKey(p_curve))
			{
				p_curve.ClearCache();
				AnimationCurveLUT animationCurveLUT = new AnimationCurveLUT(p_curve, p_samples);
				cache.Add(p_curve, animationCurveLUT);
				return animationCurveLUT;
			}
			return cache[p_curve];
		}

		public static void ClearCache(this AnimationCurve p_curve)
		{
			AnimationCurveLUT animationCurveLUT = null;
			if (cache.ContainsKey(p_curve))
			{
				animationCurveLUT = cache[p_curve];
				cache.Remove(p_curve);
			}
			animationCurveLUT?.Clear();
		}
	}
}
