using System;
using UnityEngine.PostProcessing;

namespace drl.game
{
	[Serializable]
	public class AmbientOcclusionQualitySettings
	{
		public float intensity = 1f;

		public float radius = 1f;

		public bool enabled;

		public AmbientOcclusionModel.SampleCount sampleCount = AmbientOcclusionModel.SampleCount.Lowest;

		public bool downSampling;

		public bool forceForwardCompatibility;

		public bool highPrecision;
	}
}
