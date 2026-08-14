using System;

namespace drl.sim.rci
{
	public interface ICalibrationStep
	{
		void Setup(object[] p_args = null);

		void Enter(float p_duration = 0f, bool p_autoExecute = true, Action<object[]> p_onCompleteCallback = null);

		void Enter(float p_duration = 0f, bool p_autoExecute = true, Action<object[]> p_onCompleteCallback = null, Action<object[]> p_midUpdateCallback = null);

		void Execute(float p_duration = 0f, bool p_autoExit = true, Action<object[]> p_onCompleteCallback = null);

		void Exit(Action<object[]> p_onCompleteCallback = null);

		void Cancel();
	}
}
