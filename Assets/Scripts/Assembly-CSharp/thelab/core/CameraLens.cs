using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class CameraLens : MonoBehaviour
	{
		private static float[] fov_range = new float[8] { 170f, 160f, 130f, 115f, 85f, 50f, 40f, 24f };

		private static float[] lens_range = new float[8] { 1.8f, 2.1f, 2.5f, 2.8f, 3.6f, 6f, 8f, 12f };

		public List<Camera> targets;

		public bool hasCamera
		{
			get
			{
				if (targets != null)
				{
					return targets.Count >= 1;
				}
				return false;
			}
		}

		public float aspect
		{
			get
			{
				if (!hasCamera)
				{
					return 0f;
				}
				return targets[0].aspect;
			}
		}

		public float vfov
		{
			get
			{
				if (!hasCamera)
				{
					return 0f;
				}
				return targets[0].fieldOfView;
			}
			set
			{
				if (hasCamera)
				{
					targets[0].fieldOfView = value;
				}
				for (int i = 0; i < targets.Count; i++)
				{
					targets[i].fieldOfView = value;
				}
			}
		}

		public float hfov
		{
			get
			{
				return V2HFov(vfov, aspect);
			}
			set
			{
				if (hasCamera)
				{
					vfov = H2VFov(value, aspect);
				}
			}
		}

		public float lens => H2Lens(hfov);

		public static float V2HFov(float p_fov, float p_aspect)
		{
			return Mathf.Atan(Mathf.Tan(p_fov * ((float)Math.PI / 180f) * 0.5f) * p_aspect) * 2f * 57.29578f;
		}

		public static float V2HFov(float p_fov)
		{
			float p_aspect = (float)Screen.width / (float)Screen.height;
			return V2HFov(p_fov, p_aspect);
		}

		public static float H2VFov(float p_fov, float p_aspect)
		{
			return 2f * Mathf.Atan(Mathf.Tan(p_fov * ((float)Math.PI / 180f) * 0.5f) / p_aspect) * 57.29578f;
		}

		public static float H2VFov(float p_fov)
		{
			float p_aspect = (float)Screen.width / (float)Screen.height;
			return H2VFov(p_fov, p_aspect);
		}

		public static float H2Lens(float p_hfov)
		{
			float[] array = fov_range;
			float[] array2 = lens_range;
			if (p_hfov >= array[0])
			{
				return array2[0];
			}
			if (p_hfov <= array[array2.Length - 1])
			{
				return array2[array2.Length - 1];
			}
			for (int i = 1; i < array.Length; i++)
			{
				float num = array[i - 1];
				float num2 = array[i];
				if (!(p_hfov < num2))
				{
					float t = (p_hfov - num) / (num2 - num);
					return Mathf.Lerp(array2[i - 1], array2[i], t);
				}
			}
			return 0f;
		}
	}
}
