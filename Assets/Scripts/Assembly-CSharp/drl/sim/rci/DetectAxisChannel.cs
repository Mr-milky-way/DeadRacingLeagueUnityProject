using System;
using System.Linq;
using UnityEngine;
using thelab.core;

namespace drl.sim.rci
{
	public class DetectAxisChannel : ICalibrationStep
	{
		private Activity m_executionLoop;

		private float m_activeChannelThreshold = 0.15f;

		private float m_channelDetectedThreshold = 0.3f;

		private RawAxis rawAxis;

		private CalibrationData data;

		private float m_noiseFilterThreshold = 0.05f;

		private float[] axisDeltas;

		private int activeChannelIdx;

		private float deltaMax = float.MinValue;

		private float invertDelta;

		private bool inverted;

		private int prevChannelIdx = -1;

		private float invertTimer = 0.3f;

		private float[] buttonDeltas;

		public void Setup(object[] p_args = null)
		{
			if (p_args == null || p_args.Length < 2)
			{
				throw new Exception("DetectAxisChannel>Calibration step requires RawAxis and Calibration data to be provided.");
			}
			rawAxis = (RawAxis)(int)p_args[0];
			data = (CalibrationData)p_args[1];
		}

		public void Enter(float p_duration = 0f, bool p_autoExecute = true, Action<object[]> p_onCompleteCallback = null)
		{
		}

		public void Enter(float p_duration = 0f, bool p_autoExecute = true, Action<object[]> p_onCompleteCallback = null, Action<object[]> p_updateCallback = null)
		{
			int axisCount = RCI.GetAxisCount();
			int buttonCount = RCI.GetButtonCount();
			axisDeltas = ((axisCount + buttonCount >= 4) ? new float[axisCount + buttonCount] : new float[4]);
			buttonDeltas = new float[buttonCount];
			if (p_autoExecute)
			{
				Execute(p_duration, p_autoExit: true, p_onCompleteCallback, p_updateCallback);
			}
		}

		public void Execute(float p_duration = 0f, bool p_autoExit = true, Action<object[]> p_onCompleteCallback = null)
		{
		}

		public void Execute(float p_duration = 0f, bool p_autoExit = true, Action<object[]> p_onCompleteCallback = null, Action<object[]> p_updateCallback = null)
		{
			Debug.Log("Calibration>EXECUTING CHANNEL DETECTION " + rawAxis);
			int axisCount = RCI.GetAxisCount();
			int buttonCount = RCI.GetButtonCount();
			m_executionLoop = Activity.Run(delegate
			{
				for (int i = 0; i < axisCount; i++)
				{
					if ((data == null || !data.ElementIDs.ContainsValue(i)) && Mathf.Abs(RCI.GetRawFromIndex(i)) > m_activeChannelThreshold)
					{
						float num = Mathf.Abs(RCI.GetDeltaFromIndex(i));
						if (!float.IsNaN(num) && num > m_noiseFilterThreshold && i >= 0 && i < axisDeltas.Length)
						{
							axisDeltas[i] += num;
						}
					}
				}
				if (rawAxis == RawAxis.ToggleA || rawAxis == RawAxis.ToggleB)
				{
					for (int j = 0; j < buttonCount; j++)
					{
						if (RCI.GetButtonChanged(j))
						{
							axisDeltas[j + axisCount] += 1f;
						}
					}
				}
				deltaMax = axisDeltas.Max();
				UpdateFoundChannel(p_updateCallback);
				if (deltaMax >= m_channelDetectedThreshold)
				{
					Debug.Log("FOUND CHANNEL WITH THRESHOLD " + deltaMax);
					if (m_executionLoop != null)
					{
						ClearRunningActivity();
						if (p_autoExit)
						{
							Exit(p_onCompleteCallback);
						}
					}
				}
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
			activeChannelIdx = Array.IndexOf(axisDeltas, deltaMax);
			if (data != null && data.ElementIDs.ContainsValue(activeChannelIdx))
			{
				activeChannelIdx = -1;
				deltaMax = 0f;
			}
			Debug.Log("Calibration>FOUND AXIS : " + rawAxis.ToString() + " ON CHANNEL " + activeChannelIdx + " WITH NOISE " + deltaMax);
			p_onCompleteCallback?.Invoke(new object[5]
			{
				CalibrationSteps.AxisChannelDetection,
				rawAxis,
				activeChannelIdx,
				deltaMax,
				inverted
			});
			ClearRunningActivity();
		}

		public void UpdateFoundChannel(Action<object[]> p_onFoundChannelCallback = null)
		{
			activeChannelIdx = Array.IndexOf(axisDeltas, deltaMax);
			if (data != null && data.ElementIDs.ContainsValue(activeChannelIdx))
			{
				activeChannelIdx = -1;
			}
			if (activeChannelIdx != prevChannelIdx)
			{
				invertTimer = 0.3f;
				invertDelta = 0f;
				prevChannelIdx = activeChannelIdx;
			}
			if (invertTimer > 0f && activeChannelIdx >= 0)
			{
				if (activeChannelIdx < RCI.GetAxisCount())
				{
					float num = Mathf.Abs(RCI.GetDeltaFromIndex(activeChannelIdx));
					if (!float.IsNaN(num) && num > m_noiseFilterThreshold)
					{
						invertDelta += RCI.GetRawFromIndex(activeChannelIdx);
						invertTimer -= m_executionLoop.deltaTime;
					}
				}
				else
				{
					invertDelta += RCI.GetRawFromIndex(activeChannelIdx);
					invertTimer -= m_executionLoop.deltaTime;
				}
			}
			inverted = invertDelta < 0f;
			p_onFoundChannelCallback?.Invoke(new object[4]
			{
				CalibrationSteps.AxisChannelDetection,
				rawAxis,
				activeChannelIdx,
				inverted
			});
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
