using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.game;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

public class UICalibrationStep : UIScreenView
{
	public enum Direction
	{
		Forward = 0,
		Back = 1
	}

	public CalibrationSteps step;

	public TimerView timer;

	public Text timerField;

	public Image leftFillBar;

	public Image rightFillBar;

	public Image timerBar;

	public Toggle invertToggle;

	public RawAxis axis;

	public Direction direction;

	private CalibrationFSM calibrationFSM;

	private int axisChannelIdx = -1;

	private float minAxisRange = -1f;

	private float maxAxisRange = 1f;

	private CalibrationData calibrationData;

	public FadeComponent fade;

	public GameObject preCalibrationText;

	public Text calibrationInfoText;

	public string calibrationInfo = "";

	public void ShowStep(CalibrationData p_data)
	{
		timerField.text = ((int)timer.duration).ToString();
		if ((bool)leftFillBar)
		{
			leftFillBar.fillAmount = 0f;
		}
		if ((bool)rightFillBar)
		{
			rightFillBar.fillAmount = 0f;
		}
		if ((bool)timerBar)
		{
			timerBar.fillAmount = 0f;
		}
		axisChannelIdx = -1;
		if (preCalibrationText != null)
		{
			preCalibrationText.SetActive(value: true);
		}
		calibrationInfoText.gameObject.SetActive(preCalibrationText == null);
		fade.FadeIn(0f);
		if (calibrationFSM == null)
		{
			calibrationFSM = new CalibrationFSM();
		}
		calibrationData = p_data;
		if (invertToggle != null)
		{
			invertToggle.onValueChanged.AddListener(delegate
			{
				OnInvertToggle();
			});
		}
	}

	public void StartStep()
	{
		if (timer.active)
		{
			return;
		}
		calibrationInfoText.text = calibrationInfo;
		calibrationInfoText.gameObject.SetActive(value: true);
		if (preCalibrationText != null)
		{
			preCalibrationText.SetActive(value: false);
		}
		DRLUINavigationSystem.controllerEnabled = false;
		DRLUINavigationSystem.controllerNavEnabled = false;
		this.TimerRunOnce(delegate
		{
			timer.gameObject.SetActive(value: true);
			if (invertToggle != null)
			{
				invertToggle.gameObject.SetActive(value: false);
			}
			timer.Play();
		}, 0.3f);
		StartCalibrationFSMStep();
		this.TimerRunOnce(delegate
		{
			if (calibrationFSM != null && calibrationFSM.running)
			{
				calibrationFSM.CancelStep();
			}
		}, timer.duration + 0.5f);
	}

	public void StopStep()
	{
		timer.Stop();
		timerField.text = "";
		fade.FadeOut(0f);
		DRLUINavigationSystem.controllerEnabled = true;
		DRLUINavigationSystem.controllerNavEnabled = true;
		this.TimerRunOnce(delegate
		{
			calibrationInfoText.text = calibrationInfo;
			base.gameObject.SetActive(value: false);
		}, 0.3f);
		if (calibrationFSM != null)
		{
			calibrationFSM.CancelStep();
		}
		calibrationFSM = null;
	}

	private void Update()
	{
		UpdateUIIndicators();
	}

	private void UpdateUIIndicators()
	{
		if (timerBar.gameObject != null && timer.active)
		{
			timerBar.fillAmount = timer.elapsed / timer.duration;
		}
		if (step == CalibrationSteps.AxisChannelDetection)
		{
			float num = 0f;
			if (axisChannelIdx >= 0)
			{
				num = ((axisChannelIdx >= RCI.GetAxisCount()) ? (RCI.GetButtonRawIndex(axisChannelIdx) ? 1f : (-1f)) : RCI.GetRawFromIndex(axisChannelIdx));
			}
			if (calibrationData != null && calibrationData.Invert.ContainsKey(axis))
			{
				num = (calibrationData.Invert[axis] ? (0f - num) : num);
			}
			if (num > 0f)
			{
				leftFillBar.fillAmount = num / Mathf.Abs(maxAxisRange);
				rightFillBar.fillAmount = 0f;
			}
			else
			{
				leftFillBar.fillAmount = 0f;
				rightFillBar.fillAmount = Mathf.Abs(num) / Mathf.Abs(minAxisRange);
			}
		}
		if (!timer.active)
		{
			timerField.text = "";
		}
		else
		{
			timerField.text = Mathf.Clamp((int)(timer.duration - timer.elapsed) + 1, 1, (int)timer.duration).ToString();
		}
	}

	private void StartCalibrationFSMStep()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (calibrationFSM == null)
		{
			calibrationFSM = new CalibrationFSM();
		}
		switch (step)
		{
		case CalibrationSteps.CenterPointsDetection:
		{
			CalibrationFSM obj4 = calibrationFSM;
			obj4.stepResultAction = (Action<object[]>)Delegate.Combine(obj4.stepResultAction, new Action<object[]>(OnCenterPointsDetected));
			calibrationFSM.StartStep<DetectCenterPoints>(p_autoComplete: true, timer.duration, p_async: false, Array.Empty<object>());
			break;
		}
		case CalibrationSteps.AxisChannelDetection:
		{
			CalibrationFSM obj2 = calibrationFSM;
			obj2.stepResultAction = (Action<object[]>)Delegate.Combine(obj2.stepResultAction, new Action<object[]>(OnAxisDetected));
			CalibrationFSM obj3 = calibrationFSM;
			obj3.midStepUpdateAction = (Action<object[]>)Delegate.Combine(obj3.midStepUpdateAction, new Action<object[]>(OnUpdateAxis));
			calibrationData.ElementIDs[axis] = -1;
			float p_duration = ((axis == RawAxis.ToggleA || axis == RawAxis.ToggleB) ? (timer.duration * 3f / 5f) : timer.duration);
			calibrationFSM.StartStep<DetectAxisChannel>(p_autoComplete: true, p_duration, p_async: false, new object[2] { axis, calibrationData });
			break;
		}
		case CalibrationSteps.ChannelFiltering:
		{
			CalibrationFSM obj = calibrationFSM;
			obj.stepResultAction = (Action<object[]>)Delegate.Combine(obj.stepResultAction, new Action<object[]>(OnChannelsFiltered));
			calibrationFSM.StartStep<FilterChannels>(p_autoComplete: true, timer.duration, p_async: false, Array.Empty<object>());
			break;
		}
		case CalibrationSteps.CenterPause:
			this.TimerRunOnce(delegate
			{
				Notify("calibration.step.complete@timer.complete");
			}, timer.duration);
			break;
		case CalibrationSteps.MaxAxisRange:
		case CalibrationSteps.MinAxisRange:
			break;
		}
	}

	private void OnCenterPointsDetected(object[] p_result)
	{
		if (p_result != null && p_result.Length >= 2 && (CalibrationSteps)p_result[0] == CalibrationSteps.CenterPointsDetection)
		{
			float[] array = (float[])p_result[1];
			calibrationData.Centers = new float[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				Debug.Log($"Calibration>DETECTED CENTER POINT FOR AXIS {i} VALUE {array[i]}");
				calibrationData.Centers[i] = array[i];
			}
			if (calibrationFSM != null)
			{
				CalibrationFSM obj = calibrationFSM;
				obj.stepResultAction = (Action<object[]>)Delegate.Remove(obj.stepResultAction, new Action<object[]>(OnCenterPointsDetected));
			}
			calibrationInfoText.text = "<color=green>SUCCESS!</color> CHANNEL CENTERS SET.";
			Notify("calibration.step.complete@timer.complete");
		}
	}

	private void OnAxisDetected(object[] p_result)
	{
		if (p_result == null || p_result.Length < 5 || (CalibrationSteps)p_result[0] != CalibrationSteps.AxisChannelDetection)
		{
			return;
		}
		RawAxis rawAxis = (RawAxis)p_result[1];
		int num = (int)p_result[2];
		float num2 = (float)p_result[3];
		bool flag = (bool)p_result[4];
		if (num2 == 0f)
		{
			axisChannelIdx = -1;
			calibrationData.ElementIDs[rawAxis] = axisChannelIdx;
			Notify("calibration.axis.invert", calibrationData);
			if (calibrationFSM != null)
			{
				if (calibrationFSM.running)
				{
					calibrationFSM.CancelStep();
				}
				CalibrationFSM obj = calibrationFSM;
				obj.stepResultAction = (Action<object[]>)Delegate.Remove(obj.stepResultAction, new Action<object[]>(OnAxisDetected));
				CalibrationFSM obj2 = calibrationFSM;
				obj2.midStepUpdateAction = (Action<object[]>)Delegate.Remove(obj2.midStepUpdateAction, new Action<object[]>(OnUpdateAxis));
			}
			this.TimerRunOnce(delegate
			{
				if (base.gameObject.activeInHierarchy)
				{
					OnAxisUndetected();
				}
			}, timer.duration - timer.elapsed - 0.05f);
			return;
		}
		axisChannelIdx = num;
		Debug.Log("Calibration>AXIS " + rawAxis.ToString() + " DETECTED ON CHANNEL " + num + " " + calibrationData.Centers.Length);
		calibrationData.ElementIDs[rawAxis] = axisChannelIdx;
		if (direction == Direction.Forward)
		{
			calibrationData.Invert[rawAxis] = flag;
		}
		if (calibrationFSM != null)
		{
			if (calibrationFSM.running)
			{
				calibrationFSM.CancelStep();
			}
			CalibrationFSM obj3 = calibrationFSM;
			obj3.stepResultAction = (Action<object[]>)Delegate.Remove(obj3.stepResultAction, new Action<object[]>(OnAxisDetected));
			CalibrationFSM obj4 = calibrationFSM;
			obj4.midStepUpdateAction = (Action<object[]>)Delegate.Remove(obj4.midStepUpdateAction, new Action<object[]>(OnUpdateAxis));
			if (axisChannelIdx != -1)
			{
				if (calibrationData.ChannelRange.ContainsKey(calibrationData.ElementIDs[rawAxis]))
				{
					float item = calibrationData.ChannelRange[calibrationData.ElementIDs[rawAxis]].Item2;
					if (calibrationData.RangeMin.ContainsKey(rawAxis))
					{
						calibrationData.RangeMin[rawAxis] = item;
					}
					else
					{
						calibrationData.RangeMin.Add(rawAxis, item);
					}
					float item2 = calibrationData.ChannelRange[calibrationData.ElementIDs[rawAxis]].Item1;
					if (calibrationData.RangeMax.ContainsKey(rawAxis))
					{
						calibrationData.RangeMax[rawAxis] = item2;
					}
					else
					{
						calibrationData.RangeMax.Add(rawAxis, item2);
					}
				}
				if (rawAxis == RawAxis.ToggleA || rawAxis == RawAxis.ToggleB)
				{
					CalibrationFSM obj5 = calibrationFSM;
					obj5.stepResultAction = (Action<object[]>)Delegate.Combine(obj5.stepResultAction, new Action<object[]>(OnMaxAxisRangeDetected));
					calibrationFSM.StartStep<MaxAxisRange>(p_autoComplete: true, timer.duration - timer.elapsed, p_async: true, new object[3] { axis, axisChannelIdx, flag });
					CalibrationFSM obj6 = calibrationFSM;
					obj6.stepResultAction = (Action<object[]>)Delegate.Combine(obj6.stepResultAction, new Action<object[]>(OnMinAxisRangeDetected));
					calibrationFSM.StartStep<MinAxisRange>(p_autoComplete: true, timer.duration - timer.elapsed, p_async: true, new object[3] { axis, axisChannelIdx, flag });
				}
				else
				{
					calibrationInfoText.text = "<color=green>SUCCESS!</color> CHANNEL DETECTED.";
					Notify("calibration.step.complete@timer.complete");
				}
			}
		}
		Notify("calibration.axis.invert", calibrationData);
	}

	private void OnUpdateAxis(object[] p_result)
	{
		if (p_result == null || p_result.Length < 4 || (CalibrationSteps)p_result[0] != CalibrationSteps.AxisChannelDetection)
		{
			return;
		}
		RawAxis key = (RawAxis)p_result[1];
		int num = (int)p_result[2];
		bool value = (bool)p_result[3];
		axisChannelIdx = num;
		if (axisChannelIdx != -1)
		{
			CalibrationData calibrationData = new CalibrationData();
			calibrationData.ElementIDs[key] = axisChannelIdx;
			if (direction == Direction.Forward)
			{
				calibrationData.Invert[key] = value;
				this.calibrationData.Invert[key] = value;
			}
			Notify("calibration.axis.invert", calibrationData);
		}
	}

	private void OnAxisUndetected()
	{
		Debug.Log("Calibration>AXIS " + axis.ToString() + " NOT DETECTED!");
		calibrationInfoText.text = "<color=red> FAILED! </color> COULDN'T DETECT CHANNEL.";
		Notify("calibration.axis.undetected");
	}

	private void RefineCenters(float p_calculatedCenter)
	{
		if (calibrationData != null && axisChannelIdx >= 0 && axisChannelIdx < RCI.GetAxisCount() && axisChannelIdx <= calibrationData.Centers.Length - 1)
		{
			float num = Mathf.Min(Mathf.Abs(p_calculatedCenter), Mathf.Abs(calibrationData.Centers[axisChannelIdx]));
			calibrationData.Centers[axisChannelIdx] = num;
		}
	}

	public void OnMinAxisRangeDetected(object[] p_result)
	{
		if (p_result != null && p_result.Length >= 3 && (CalibrationSteps)p_result[0] == CalibrationSteps.MinAxisRange)
		{
			RawAxis rawAxis = (RawAxis)p_result[1];
			float num = (float)p_result[2];
			if (rawAxis != RawAxis.ToggleA && rawAxis != RawAxis.ToggleB)
			{
				minAxisRange = ((num <= -0.05f) ? num : (-1f));
			}
			else
			{
				maxAxisRange = num;
			}
			Debug.Log("Calibration>AXIS " + rawAxis.ToString() + " MIN " + minAxisRange);
			if (calibrationData.RangeMin.ContainsKey(rawAxis))
			{
				calibrationData.RangeMin[rawAxis] = minAxisRange;
			}
			else
			{
				calibrationData.RangeMin.Add(rawAxis, minAxisRange);
			}
			if (calibrationFSM != null)
			{
				CalibrationFSM obj = calibrationFSM;
				obj.stepResultAction = (Action<object[]>)Delegate.Remove(obj.stepResultAction, new Action<object[]>(OnMinAxisRangeDetected));
			}
			Notify("calibration.axis.invert", calibrationData);
		}
	}

	public void OnMaxAxisRangeDetected(object[] p_result)
	{
		if (p_result != null && p_result.Length >= 3 && (CalibrationSteps)p_result[0] == CalibrationSteps.MaxAxisRange)
		{
			RawAxis rawAxis = (RawAxis)p_result[1];
			float num = (float)p_result[2];
			if (rawAxis != RawAxis.ToggleA && rawAxis != RawAxis.ToggleB)
			{
				maxAxisRange = ((num >= 0.05f) ? num : 1f);
			}
			else
			{
				maxAxisRange = num;
			}
			if (calibrationData.ChannelRange.ContainsKey(calibrationData.ElementIDs[rawAxis]))
			{
				float item = calibrationData.ChannelRange[calibrationData.ElementIDs[rawAxis]].Item1;
				maxAxisRange = Mathf.Max(maxAxisRange, item);
			}
			if (calibrationData.RangeMax.ContainsKey(rawAxis))
			{
				calibrationData.RangeMax[rawAxis] = maxAxisRange;
			}
			else
			{
				calibrationData.RangeMax.Add(rawAxis, maxAxisRange);
			}
			Debug.Log("Calibration>AXIS " + rawAxis.ToString() + " MAX " + maxAxisRange);
			Notify("calibration.axis.invert", calibrationData);
			if (calibrationFSM != null)
			{
				CalibrationFSM obj = calibrationFSM;
				obj.stepResultAction = (Action<object[]>)Delegate.Remove(obj.stepResultAction, new Action<object[]>(OnMaxAxisRangeDetected));
			}
			calibrationInfoText.text = "<color=green>SUCCESS!</color> CHANNEL DETECTED.";
			Notify("calibration.step.complete@timer.complete");
		}
	}

	public void OnChannelsFiltered(object[] p_result)
	{
		if (p_result == null || p_result.Length < 3 || (CalibrationSteps)p_result[0] != CalibrationSteps.ChannelFiltering)
		{
			return;
		}
		Dictionary<int, Tuple<float, float>> dictionary = (Dictionary<int, Tuple<float, float>>)p_result[1];
		float[] array = (float[])p_result[2];
		calibrationData.ActiveChannels = new int[dictionary.Count];
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<int, Tuple<float, float>> item in dictionary)
		{
			Debug.Log($"Calibration>DETECTED CHANNEL FOR AXIS {num2} VALUE {item.Key}");
			calibrationData.ActiveChannels[num2] = item.Key;
			if (!calibrationData.ChannelRange.ContainsKey(item.Key))
			{
				calibrationData.ChannelRange.Add(item.Key, item.Value);
			}
			if (array[item.Key] <= 0f)
			{
				num++;
				calibrationData.ActiveChannels[num2] = -1;
			}
			num2++;
		}
		if (calibrationFSM != null)
		{
			CalibrationFSM obj = calibrationFSM;
			obj.stepResultAction = (Action<object[]>)Delegate.Remove(obj.stepResultAction, new Action<object[]>(OnChannelsFiltered));
		}
		if (num == 0)
		{
			calibrationInfoText.text = "<color=green>SUCCESS!</color> CHANNELS FILTERED.";
			Notify("calibration.axis.invert", calibrationData);
			Notify("calibration.step.complete@timer.complete");
		}
		else
		{
			calibrationInfoText.text = "<color=red>WARNING!</color> NOT ALL CHANNELS COULD BE FILTERED.";
			Notify("calibration.step.complete@timer.complete");
		}
	}

	private void OnInvertToggle()
	{
		float num = maxAxisRange;
		maxAxisRange = minAxisRange;
		minAxisRange = num;
		if (calibrationData != null)
		{
			if (calibrationData.Invert.ContainsKey(axis))
			{
				calibrationData.Invert[axis] = invertToggle.isOn;
			}
			else
			{
				calibrationData.Invert.Add(axis, invertToggle.isOn);
			}
		}
		Notify("calibration.axis.invert", axis, calibrationData);
	}
}
