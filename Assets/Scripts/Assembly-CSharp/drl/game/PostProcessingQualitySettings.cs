using System;

namespace drl.game
{
	[Serializable]
	public class PostProcessingQualitySettings
	{
		public bool eyeAdaptation;

		public bool bloom;

		public bool chromaticAberration;

		public bool grain;

		public bool colorGrading;

		public bool radioFx;

		public bool sunShafts;

		public bool screenSpaceReflection;
	}
}
