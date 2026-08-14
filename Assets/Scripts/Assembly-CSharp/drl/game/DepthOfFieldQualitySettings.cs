using System;
using UnityEngine.PostProcessing;

namespace drl.game
{
	[Serializable]
	public class DepthOfFieldQualitySettings
	{
		public bool enabled;

		public DepthOfFieldModel.KernelSize kernelSize;
	}
}
