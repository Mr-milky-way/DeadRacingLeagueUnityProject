using System;
using UnityEngine;

namespace drl.game
{
	[Serializable]
	public class EffectsQualitySettings
	{
		public bool softParticles;

		public SkinWeights blendWeights = SkinWeights.TwoBones;

		public string categoryName = "effects";

		public bool categoryEnabled;
	}
}
