using System;

namespace drl.game
{
	[Serializable]
	public class GraphicsQualityPreset
	{
		public enum LowMedHigh
		{
			Low = 0,
			Medium = 1,
			High = 2
		}

		public enum LowestLowMedHigh
		{
			Lowest = 0,
			Low = 1,
			Medium = 2,
			High = 3
		}

		public enum OffLowMedHigh
		{
			Off = 0,
			Low = 1,
			Medium = 2,
			High = 3
		}

		public enum OffOn
		{
			Off = 0,
			On = 1
		}

		public enum AASetting
		{
			Off = 0,
			FXAA = 1,
			TAA = 2
		}

		public string id = "preset";

		public string label = "Preset Name";

		public OffLowMedHigh shadow;

		public AASetting antialias;

		public LowestLowMedHigh texture;

		public OffLowMedHigh ambientOcclusion;

		public LowMedHigh postProcessing;

		public bool motionBlur;

		public OffLowMedHigh depthOfField;

		public bool advancedRendering = true;

		public bool waterReflection = true;

		public LowMedHigh tier;

		public LowMedHigh effectsQuality;

		public LowMedHigh details;

		public float resolutionScale = 1f;
	}
}
