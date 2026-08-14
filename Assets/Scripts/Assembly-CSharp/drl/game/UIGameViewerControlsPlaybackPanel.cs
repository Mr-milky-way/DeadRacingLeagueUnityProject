using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIGameViewerControlsPlaybackPanel : MonoBehaviour
	{
		[Header("Playback")]
		public Image[] playStates;

		public DRLSliderView speedSlider;

		public Text speedSliderField;

		public DRLSliderView timeSlider;

		public Text timeSliderField;

		public List<GameObject> replayModeLayout;

		[Header("Cameras")]
		public UIElementView[] cameraModeButtons;

		public ListComponent cameraToolButtonList;

		[Header("Targets")]
		public ListComponent targetButtonList;

		public DRLStepperView targetStepper;

		[Header("Stats")]
		public DRLToggleView nameToggle;

		public DRLToggleView raceStatsToggle;

		public DRLToggleView controllerToggle;

		[Header("DEPRECATED")]
		public DRLIntStepperView cameraStepper;

		public DRLToggleView motorsToggle;

		public DRLToggleView autoHideToggle;

		public ViewerCameraModeType oldCameraMode
		{
			get
			{
				return (ViewerCameraModeType)cameraStepper.value;
			}
			set
			{
				cameraStepper.SetValue((int)value);
			}
		}

		public bool motorsVisible
		{
			get
			{
				if (!motorsToggle.toggle)
				{
					return false;
				}
				return motorsToggle.toggle.isOn;
			}
			set
			{
				if ((bool)motorsToggle.toggle)
				{
					motorsToggle.toggle.isOn = value;
				}
			}
		}

		public bool autoHideEnabled
		{
			get
			{
				return autoHideToggle.toggle.isOn;
			}
			set
			{
				autoHideToggle.toggle.isOn = value;
			}
		}

		public float speed
		{
			get
			{
				return speedSlider.slider.value;
			}
			set
			{
				speedSlider.slider.enabled = false;
				speedSlider.slider.value = value;
				speedSlider.slider.enabled = true;
			}
		}

		public float time
		{
			get
			{
				return timeSlider.slider.value;
			}
			set
			{
				timeSlider.slider.enabled = false;
				timeSlider.slider.value = value;
				timeSlider.slider.enabled = true;
			}
		}

		public float duration
		{
			get
			{
				return timeSlider.slider.maxValue;
			}
			set
			{
				timeSlider.slider.maxValue = value;
				timeSlider.slider.minValue = 0f;
			}
		}

		public int targetIndex
		{
			get
			{
				return targetStepper.index;
			}
			set
			{
				targetStepper.index = value;
				targetStepper.Refresh();
			}
		}

		public bool playerNameVisible
		{
			get
			{
				return nameToggle.toggle.isOn;
			}
			set
			{
				nameToggle.toggle.isOn = value;
			}
		}

		public bool raceStatsVisible
		{
			get
			{
				return raceStatsToggle.toggle.isOn;
			}
			set
			{
				raceStatsToggle.toggle.isOn = value;
			}
		}

		public bool controllerVisible
		{
			get
			{
				return controllerToggle.toggle.isOn;
			}
			set
			{
				controllerToggle.toggle.isOn = value;
			}
		}

		protected void Start()
		{
			speedSlider.slider.onValueChanged.AddListener(RefreshSpeedField);
			timeSlider.slider.onValueChanged.AddListener(RefreshTimeField);
		}

		public void SetPause(bool p_flag)
		{
			Image image = Reflection<object>.Get(playStates, 0);
			if ((bool)image)
			{
				image.enabled = !p_flag;
			}
			image = Reflection<object>.Get(playStates, 1);
			if ((bool)image)
			{
				image.enabled = p_flag;
			}
		}

		private void RefreshSpeedField(float v)
		{
			int num = Mathf.FloorToInt(v * 100f);
			string text = ((num <= 0) ? "" : "+");
			speedSliderField.text = text + num + "%";
		}

		private void RefreshTimeField(float v)
		{
			timeSliderField.text = Format.SecondsToTime(v, 2, p_use_ms: true);
		}

		public void SetSpectatorMode()
		{
			for (int i = 0; i < replayModeLayout.Count; i++)
			{
				replayModeLayout[i].SetActive(value: false);
			}
			base.gameObject.SetActive(value: false);
			Activity.RunOnce(delegate
			{
				base.gameObject.SetActive(value: true);
			}, 1f / 30f);
		}

		public void SetReplayMode()
		{
			for (int i = 0; i < replayModeLayout.Count; i++)
			{
				replayModeLayout[i].SetActive(value: true);
			}
			base.gameObject.SetActive(value: false);
			Activity.RunOnce(delegate
			{
				base.gameObject.SetActive(value: true);
			}, 1f / 30f);
		}

		public void SetTargets(List<string> p_targets)
		{
			List<string> list = new List<string>(p_targets);
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = list[i].ToUpper();
				if (list[i].Length > 11)
				{
					list[i] = list[i].Substring(0, 11) + "...";
				}
			}
			targetStepper.labels = ((list.Count > 0) ? list.ToArray() : new string[1] { "NO PLAYERS" });
			targetStepper.index = 0;
			targetStepper.min = 0;
			targetStepper.max = list.Count - 1;
			targetStepper.Refresh();
		}
	}
}
