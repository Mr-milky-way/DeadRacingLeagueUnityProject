using System;

namespace drl.game
{
	[Serializable]
	public class TextureQualitySettings
	{
		public enum Quality
		{
			Full = 0,
			Half = 1,
			Quarter = 2,
			Eighth = 3
		}

		public enum Filtering
		{
			PerTexture = 0,
			ForcedOn = 1
		}

		public Quality quality;

		public Filtering filtering;
	}
}
