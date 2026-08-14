using System;
using UnityEngine;

namespace drl.game
{
	[Serializable]
	public class ShadowQualitySettings
	{
		public enum Cascades
		{
			None = 0,
			Two = 2,
			Four = 4
		}

		public ShadowQuality quality;

		public ShadowResolution resolution;

		public float distance;

		public Cascades cascades;

		public float shadowCascade2Split;

		public Vector3 shadowCascade4Split;
	}
}
