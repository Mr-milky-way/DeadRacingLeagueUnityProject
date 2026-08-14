using System;
using UnityEngine;
using thelab.core;

namespace drl.sim.rci
{
	public class DetectCenterPoints : ICalibrationStep
	{
		private Activity m_executionLoop;

		private float[] centerValues;

		private int axisCheckCount;

		public void Setup(object[] p_args = null)
		{
		}

		public void Enter(float p_duration = 0f, bool p_autoExecute = true, Action<object[]> p_onCompleteCallback = null)
		{
			Debug.Log("Calibration>ENTERING CENTER STICKS DETECTION!");
			if (p_autoExecute)
			{
				Execute(p_duration, p_autoExit: true, p_onCompleteCallback);
			}
			centerValues = new float[RCI.GetAxisCount()];
		}

		public void Enter(float p_duration = 0f, bool p_autoExecute = true, Action<object[]> p_onCompleteCallback = null, Action<object[]> p_midUpdateCallback = null)
		{
		}

		public void Execute(float p_duration = 0f, bool p_autoExit = true, Action<object[]> p_onCompleteCallback = null)
		{
			m_executionLoop = Activity.Run(delegate
			{
				for (int i = 0; i < centerValues.Length; i++)
				{
					centerValues[i] += RCI.GetRawFromIndex(i);
				}
				axisCheckCount++;
			}, p_duration, 0f, false);
			if (!p_autoExit)
			{
				return;
			}
			Activity.RunOnce(delegate
			{
				if (m_executionLoop != null)
				{
					Exit(p_onCompleteCallback);
				}
			}, p_duration);
		}

		public void Exit(Action<object[]> p_onCompleteCallback = null)
		{
			for (int i = 0; i < centerValues.Length; i++)
			{
				centerValues[i] /= axisCheckCount;
			}
			Debug.Log("Calibration>EXITING CENTER AXIS DETECTION ");
			p_onCompleteCallback?.Invoke(new object[2]
			{
				CalibrationSteps.CenterPointsDetection,
				centerValues
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
