using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl
{
	public class OscilatorLight : MonoBehaviour
	{
		public List<Light> targets;

		public float start;

		public float offset;

		public float speed = 1f;

		public float[] intensity = new float[2] { 0f, 1f };

		protected void Update()
		{
			float num = 0f;
			float num2 = (float)Math.PI / 180f * offset;
			float num3 = start + Time.time * speed * 360f * ((float)Math.PI / 180f);
			for (int i = 0; i < targets.Count; i++)
			{
				Light light = targets[i];
				float num4 = 0f - Mathf.Cos(num3 + num);
				num4 = (num4 + 1f) * 0.5f;
				light.intensity = Mathf.Lerp(intensity[0], intensity[1], num4);
				num += num2;
			}
		}
	}
}
