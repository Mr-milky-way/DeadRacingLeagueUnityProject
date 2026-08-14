using System;
using UnityEngine.PostProcessing;

namespace drl.game
{
	[Serializable]
	public class AntiAliasingQualitySettings
	{
		public bool enabled;

		public AntialiasingModel.Method method;

		public AntialiasingModel.FxaaPreset preset;
	}
}
