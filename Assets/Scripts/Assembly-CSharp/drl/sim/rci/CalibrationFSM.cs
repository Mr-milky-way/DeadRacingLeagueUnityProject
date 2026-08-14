using System;
using System.Collections.Generic;

namespace drl.sim.rci
{
	public class CalibrationFSM
	{
		public ICalibrationStep currentStep;

		public Action<object[]> stepResultAction;

		public Action<object[]> midStepUpdateAction;

		public float assignmentDuration = 5f;

		public bool running;

		public void StartStep<T>(bool p_autoComplete = true) where T : ICalibrationStep, new()
		{
			if (currentStep != null)
			{
				currentStep.Cancel();
				currentStep = null;
			}
			currentStep = new T();
			running = true;
			if (currentStep.GetType() == typeof(DetectAxisChannel))
			{
				currentStep.Enter(assignmentDuration, p_autoComplete, StepResultCallback, MidStepUpdateCallback);
			}
			else
			{
				currentStep.Enter(assignmentDuration, p_autoComplete, StepResultCallback);
			}
		}

		public void StartStep<T>(bool p_autoComplete = true, params object[] p_data) where T : ICalibrationStep, new()
		{
			if (currentStep != null)
			{
				currentStep.Cancel();
				currentStep = null;
			}
			currentStep = new T();
			if (p_data != null)
			{
				currentStep.Setup(p_data);
			}
			running = true;
			if (currentStep.GetType() == typeof(DetectAxisChannel))
			{
				currentStep.Enter(assignmentDuration, p_autoComplete, StepResultCallback, MidStepUpdateCallback);
			}
			else
			{
				currentStep.Enter(assignmentDuration, p_autoComplete, StepResultCallback);
			}
		}

		public void StartStep<T>(bool p_autoComplete = true, float p_duration = 5f, bool p_async = false, params object[] p_data) where T : ICalibrationStep, new()
		{
			assignmentDuration = p_duration;
			if (!p_async)
			{
				if (currentStep != null)
				{
					currentStep.Cancel();
					currentStep = null;
				}
				currentStep = new T();
				if (p_data != null)
				{
					currentStep.Setup(p_data);
				}
				running = true;
				if (currentStep.GetType() == typeof(DetectAxisChannel))
				{
					currentStep.Enter(assignmentDuration, p_autoComplete, StepResultCallback, MidStepUpdateCallback);
				}
				else
				{
					currentStep.Enter(assignmentDuration, p_autoComplete, StepResultCallback);
				}
			}
			else
			{
				ICalibrationStep calibrationStep = new T();
				if (p_data != null)
				{
					calibrationStep.Setup(p_data);
				}
				running = true;
				calibrationStep.Enter(assignmentDuration, p_autoComplete, StepResultCallback);
			}
		}

		public void ChangeStep<T>(bool p_autoComplete = true) where T : ICalibrationStep, new()
		{
			if (currentStep != null)
			{
				currentStep.Exit(StepResultCallback);
			}
			currentStep = new T();
			running = true;
			currentStep.Enter(assignmentDuration, p_autoComplete, StepResultCallback);
		}

		public void ChangeStep<T>(bool p_autoComplete = true, params object[] p_data) where T : ICalibrationStep, new()
		{
			if (currentStep != null)
			{
				currentStep.Exit(StepResultCallback);
			}
			currentStep = new T();
			if (p_data != null)
			{
				currentStep.Setup(p_data);
			}
			running = true;
			currentStep.Enter(assignmentDuration, p_autoComplete, StepResultCallback);
		}

		public void ChangeStep<T>(bool p_autoComplete = true, float p_duration = 5f, params object[] p_data) where T : ICalibrationStep, new()
		{
			if (currentStep != null)
			{
				currentStep.Exit(StepResultCallback);
			}
			currentStep = new T();
			if (p_data != null)
			{
				currentStep.Setup(p_data);
			}
			assignmentDuration = p_duration;
			running = true;
			currentStep.Enter(assignmentDuration, p_autoComplete, StepResultCallback);
		}

		public void StartSteps<T>(IList<T> p_steps, bool p_autoComplete = true, float p_duration = 5f, params object[] p_data) where T : ICalibrationStep, new()
		{
			foreach (T p_step in p_steps)
			{
				ICalibrationStep calibrationStep = p_step;
				if (p_data != null)
				{
					calibrationStep.Setup(p_data);
				}
				assignmentDuration = p_duration;
				running = true;
				calibrationStep.Enter(assignmentDuration, p_autoComplete, StepResultCallback);
			}
		}

		public void ExecuteStep(bool p_autoExit = true)
		{
			if (currentStep != null)
			{
				running = true;
				currentStep.Execute(assignmentDuration, p_autoExit, StepResultCallback);
			}
		}

		public void ExitStep()
		{
			if (currentStep != null)
			{
				currentStep.Exit(StepResultCallback);
			}
		}

		public void StepResultCallback(object[] o)
		{
			if (stepResultAction != null)
			{
				stepResultAction(o);
				currentStep = null;
			}
			running = false;
		}

		public void CancelStep()
		{
			if (currentStep != null)
			{
				currentStep.Cancel();
				running = false;
				currentStep = null;
			}
		}

		public void MidStepUpdateCallback(object[] o)
		{
			if (midStepUpdateAction != null)
			{
				midStepUpdateAction(o);
			}
		}
	}
}
