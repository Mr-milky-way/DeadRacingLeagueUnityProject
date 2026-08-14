using System;
using thelab.core;

public class AsyncCalibrationStep
{
	private Action completeAction;

	private Action cancelAction;

	private Activity runningActivity;

	public bool actionFinished { get; private set; }

	public AsyncCalibrationStep(Action completeAction, Action cancelAction, Action<Activity> runningActivity)
	{
		this.completeAction = completeAction;
		this.cancelAction = cancelAction;
		this.runningActivity = Activity.Run(runningActivity);
	}

	public void SetPause(bool running)
	{
		throw new NotImplementedException();
	}

	public bool CompleteStep()
	{
		if (actionFinished)
		{
			return false;
		}
		completeAction();
		runningActivity.Stop();
		actionFinished = true;
		return true;
	}

	public bool CancelStep()
	{
		if (actionFinished)
		{
			return false;
		}
		cancelAction();
		runningActivity.Stop();
		actionFinished = true;
		return true;
	}
}
