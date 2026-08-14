using System;
using UnityEngine;
using thelab.core;

namespace drl.sim.rci
{
	public class MinAxisRange : ICalibrationStep
	{
		private Activity m_executionLoop;

		private RawAxis rawAxis;

		private float minValue;

		private int channelIdx;

		private bool isToggle;

		private bool inverted;

		public void Setup(object[] p_args = null)
		{
			if (p_args == null || p_args.Length < 2)
			{
				throw new Exception("MinAxisRange>Calibration step requires RawAxis, channelIdx and invert to be provided.");
			}
			rawAxis = (RawAxis)(int)p_args[0];
			channelIdx = (int)p_args[1];
			inverted = (bool)p_args[2];
		}

		public void Enter(float p_duration = 0f, bool p_autoExecute = true, Action<object[]> p_onCompleteCallback = null)
		{
			Debug.Log("Calibration>ENTERING MIN AXIS RANGE DETECTION!");
			minValue = float.MaxValue;
			isToggle = (rawAxis == RawAxis.ToggleA || rawAxis == RawAxis.ToggleB) && channelIdx >= RCI.GetAxisCount();
			if (p_autoExecute)
			{
				Execute(p_duration, p_autoExit: true, p_onCompleteCallback);
			}
		}

		public void Enter(float p_duration = 0f, bool p_autoExecute = true, Action<object[]> p_onCompleteCallback = null, Action<object[]> p_midUpdateCallback = null)
		{
		}

		public void Execute(float p_duration = 0f, bool p_autoExit = true, Action<object[]> p_onCompleteCallback = null)
		{
			if (isToggle)
			{
				minValue = -1f;
				Exit(p_onCompleteCallback);
				return;
			}
			m_executionLoop = Activity.Run(delegate
			{
				float rawFromIndex = RCI.GetRawFromIndex(channelIdx);
				rawFromIndex = (inverted ? (0f - rawFromIndex) : rawFromIndex);
				minValue = Mathf.Min(rawFromIndex, minValue);
				if (rawFromIndex - minValue > 0.1f)
				{
					p_autoExit = false;
					Exit(p_onCompleteCallback);
				}
			}, p_duration, 0f, false);
			if (!p_autoExit)
			{
				return;
			}
			Activity.RunOnce(delegate
			{
				if (m_executionLoop != null && p_autoExit)
				{
					Exit(p_onCompleteCallback);
				}
			}, p_duration);
		}

		public void Exit(Action<object[]> p_onCompleteCallback = null)
		{
			Debug.Log("Calibration>EXITING MIN AXIS RANGE DETECTION WITH MIN RANGE: " + minValue);
			p_onCompleteCallback?.Invoke(new object[3]
			{
				CalibrationSteps.MinAxisRange,
				rawAxis,
				minValue
			});
			ClearRunningActivity();
		}

		public void Cancel()
		{
			ClearRunningActivity();
		}

		private void ClearRunningActivity()
		{
			if (m_executionLoop != null)
			{
				m_executionLoop.Stop();
				m_executionLoop.manager.Remove(m_executionLoop);
				m_executionLoop = null;
			}
		}
	}
}
