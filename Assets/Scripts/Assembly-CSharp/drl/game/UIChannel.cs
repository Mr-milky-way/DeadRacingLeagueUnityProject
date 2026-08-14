using System;
using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	[Serializable]
	public class UIChannel
	{
		public Text channelName;

		public Dropdown channelSelection;

		public Toggle invertToggle;

		public Image leftRawBar;

		public Image rightRawBar;

		public GameObject calibratedBar;

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

		public int ID = -1;
	}
}
