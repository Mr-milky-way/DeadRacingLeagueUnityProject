using System;
using UnityEngine;
using thelab.core;

namespace drl.sim.rci
{
	public class TestStep : ICalibrationStep
	{
		private Activity m_executionLoop;

		public CalibrationSteps type { get; set; }

		public void Setup(object[] p_args = null)
		{
		}

		public void Enter(float p_duration = 0f, bool p_autoExecute = true, Action<object[]> p_onCompleteCallback = null)
		{
			Debug.Log("Calibration>ENTERED STEP ");
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
			m_executionLoop = Activity.Run(delegate
			{
				Debug.Log("Calibration>EXECUTING STEP ");
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

		public void Exit(Action<object[]> p_callback = null)
		{
			Debug.Log("Calibration>EXITING STEP ");
			p_callback?.Invoke(new object[1] { 42f });
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
