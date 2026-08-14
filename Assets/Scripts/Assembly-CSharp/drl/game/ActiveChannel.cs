using System;
using UnityEngine.UI;
using drl.sim.rci;

namespace drl.game
{
	[Serializable]
	public class ActiveChannel
	{
		public RawAxis axis;

		public Toggle invertToggle;

		public Toggle midStickToggle;

		public Button resetButton;

		public Image leftRawBar;

		public Image rightRawBar;

		public Image leftCalibratedBar;

		public Image rightCalibratedBar;

		public Image deadzoneLeftBar;

		public Image deadzoneRightBar;

		public Slider sliderMin;

		public Slider sliderMax;

		public Slider sliderCenter;

		public Slider sliderZero;

		public Slider sliderDeadzone;

		public Text sliderDeadzoneLabel;

		public int ID;

		public void Reset()
		{
			invertToggle.isOn = false;
			deadzoneLeftBar.fillAmount = 0f;
			deadzoneRightBar.fillAmount = 0f;
			sliderMin.value = 0f;
			sliderMax.value = 0f;
			sliderCenter.value = 0f;
			sliderZero.value = -1f;
			sliderDeadzone.value = 0f;
			sliderDeadzoneLabel.text = "DEADZONE 0%";
		}
	}
}
