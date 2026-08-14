using System;
using UnityEngine;

namespace drl.game
{
	[Serializable]
	public class DetailsQualitySettings
	{
		public float lodBias;

		public string categoryName = "details";

		public bool categoryEnabled;

		[Header("Fov LOD Bias")]
		public float minLODBiasFOV = 40f;

		public float maxLODBiasFOV = 115f;

		public float minLODBiasOffset = -0.36f;

		public float maxLODBiasOffset = 1f;

		public float minLODBias = 0.65f;

		[Header("GPUInstancer")]
		public float gpuIDetailDensity = 1f;

		public float gpuIMaxDistance = 120f;

		public float gpuIBillboardDistance = 0.85f;

		public float GetLODBiasOffset(float p_fov)
		{
			float t = Mathf.Clamp01((p_fov - minLODBiasFOV) / (maxLODBiasFOV - minLODBiasFOV));
			return Mathf.Max(Mathf.Lerp(minLODBiasOffset, maxLODBiasOffset, t), minLODBias);
		}
	}
}
