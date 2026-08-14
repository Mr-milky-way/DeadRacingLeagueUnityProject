using System.Collections.Generic;
using UnityEngine;
using drl.game;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

public class UICalibrationStepsController : Controller<DRLApp>
{
	public List<UICalibrationStep> steps = new List<UICalibrationStep>();

	private int m_stepIdx;

	public float stepStartDelay = 2f;

	public float stepDuration = 3f;

	[HideInInspector]
	public bool inProgress;

	private CalibrationData calibrationData;

	public void StartCalibration(CalibrationData p_calibrationData)
	{
		foreach (UICalibrationStep step in steps)
		{
			step.timer.duration = stepDuration + 0.05f;
		}
		calibrationData = p_calibrationData;
		steps[0].gameObject.SetActive(value: true);
		steps[0].ShowStep(calibrationData);
		m_stepIdx = 0;
		inProgress = true;
	}

	public UICalibrationStep NextStep()
	{
		if (m_stepIdx >= steps.Count - 1)
		{
			steps[steps.Count - 1].StopStep();
			inProgress = false;
			return null;
		}
		steps[m_stepIdx].StopStep();
		m_stepIdx++;
		steps[m_stepIdx].gameObject.SetActive(value: true);
		steps[m_stepIdx].ShowStep(calibrationData);
		return steps[m_stepIdx];
	}

	public UICalibrationStep PreviousStep()
	{
		if (m_stepIdx <= 0)
		{
			steps[0].StopStep();
			inProgress = false;
			return null;
		}
		steps[m_stepIdx].StopStep();
		m_stepIdx--;
		steps[m_stepIdx].gameObject.SetActive(value: true);
		steps[m_stepIdx].ShowStep(calibrationData);
		return steps[m_stepIdx];
	}

	public void StartStep()
	{
		if (m_stepIdx <= steps.Count - 1 && m_stepIdx >= 0)
		{
			StopAll(m_stepIdx);
			this.TimerRunOnce(delegate
			{
				steps[m_stepIdx].StartStep();
			}, stepStartDelay);
		}
	}

	public UICalibrationStep GetCurrentStep()
	{
		if (m_stepIdx > steps.Count - 1 || m_stepIdx < 0)
		{
			return null;
		}
		return steps[m_stepIdx];
	}

	public void StopAll(int p_excludeIdx = -1)
	{
		for (int i = 0; i < steps.Count; i++)
		{
			if (p_excludeIdx != i)
			{
				steps[i].StopStep();
				steps[i].gameObject.SetActive(value: false);
			}
		}
	}
}
