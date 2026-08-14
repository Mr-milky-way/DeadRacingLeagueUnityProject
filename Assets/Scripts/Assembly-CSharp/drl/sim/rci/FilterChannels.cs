using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using thelab.core;

namespace drl.sim.rci
{
	public class FilterChannels : ICalibrationStep
	{
		private Activity m_executionLoop;

		private float m_activeChannelThreshold = 0.15f;

		private float m_noiseFilterThreshold = 0.05f;

		private float m_channelDetectedThreshold = 3f;

		private float[] axisDeltas;

		private float[] axesMax;

		private float[] axesMin;

		private int[] activeChannelIdxs = new int[4];

		private Dictionary<int, Tuple<float, float>> channelRange = new Dictionary<int, Tuple<float, float>>();

		public void Setup(object[] p_args = null)
		{
		}

		public void Enter(float p_duration = 0f, bool p_autoExecute = true, Action<object[]> p_onCompleteCallback = null)
		{
			axisDeltas = ((RCI.GetAxisCount() >= 4) ? new float[RCI.GetAxisCount()] : new float[4]);
			axesMax = new float[axisDeltas.Length];
			axesMin = new float[axisDeltas.Length];
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
				for (int i = 0; i < RCI.GetAxisCount(); i++)
				{
					if (Mathf.Abs(RCI.GetRawFromIndex(i)) > m_activeChannelThreshold)
					{
						float num = Mathf.Abs(RCI.GetDeltaFromIndex(i));
						float rawFromIndex = RCI.GetRawFromIndex(i);
						if (!float.IsNaN(num) && num > m_noiseFilterThreshold)
						{
							axisDeltas[i] += num;
							axesMax[i] = Mathf.Max(axesMax[i], rawFromIndex);
							axesMin[i] = Mathf.Min(axesMin[i], rawFromIndex);
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
			float num = axisDeltas.Min();
			int num2 = 0;
			for (int i = 0; i < axisDeltas.Length; i++)
			{
				if (axisDeltas[i] > num)
				{
					activeChannelIdxs[num2] = i;
					num2++;
				}
				if (num2 >= activeChannelIdxs.Length)
				{
					break;
				}
			}
			int[] array = activeChannelIdxs;
			for (int j = 0; j < array.Length; j++)
			{
				int num3 = array[j];
				if (!channelRange.ContainsKey(num3))
				{
					Tuple<float, float> tuple = new Tuple<float, float>(axesMax[num3], axesMin[num3]);
					channelRange.Add(num3, tuple);
					Debug.Log("Calibration>FOUND ACTIVE CHANNEL " + num3 + " WITH NOISE " + axisDeltas[num3] + " AND RANGE [" + tuple.Item2 + "," + tuple.Item1 + "]");
				}
			}
			p_onCompleteCallback?.Invoke(new object[3]
			{
				CalibrationSteps.ChannelFiltering,
				channelRange,
				axisDeltas
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
