using System;
using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class DRLMissionScore : MonoBehaviour
	{
		[Serializable]
		public class Task
		{
			public FNMissionScoreType type;

			[Range(0f, 1f)]
			public float weight;

			public float min;

			public float max;

			public AnimationCurve curve;

			public Task()
			{
				weight = 1f;
				curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			}

			public float Evaluate(float p_score)
			{
				float num = Mathf.Max(max, min) - Mathf.Min(max, min);
				if (num <= 0f)
				{
					if (!(p_score < min))
					{
						return 1f;
					}
					return 0f;
				}
				float time = (p_score - min) / num;
				time = Mathf.Clamp01(curve.Evaluate(time));
				if (type.ToString().Contains("TimeMin"))
				{
					time = 1f - time;
				}
				return time;
			}
		}

		public List<Task> tasks;

		public float weight
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < tasks.Count; i++)
				{
					num += tasks[i].weight;
				}
				return num;
			}
		}

		public int GetTaskIndex(FNMissionScoreType p_type)
		{
			for (int i = 0; i < tasks.Count; i++)
			{
				if (tasks[i].type == p_type)
				{
					return i;
				}
			}
			return -1;
		}

		public Task GetTask(FNMissionScoreType p_type)
		{
			int taskIndex = GetTaskIndex(p_type);
			if (taskIndex >= 0)
			{
				return tasks[taskIndex];
			}
			return null;
		}

		public float Evaluate(object[] p_scores)
		{
			if (tasks.Count <= 0)
			{
				return 1f;
			}
			if (p_scores.Length != tasks.Count)
			{
				Debug.LogWarning("DRLMissionScore> [" + base.name + "] scores and tasks count do not match!");
				return 0f;
			}
			float num = Mathf.Max(0f, weight);
			float num2 = 0f;
			int num3 = p_scores.Length;
			string text = "";
			text = "DRLMissionScore> Evaluating [" + num3 + "] scores\n";
			for (int i = 0; i < num3; i++)
			{
				object obj = p_scores[i];
				float p_score = 0f;
				if (obj != null && obj is float && !float.IsNaN((float)obj))
				{
					p_score = (float)obj;
				}
				float num4 = tasks[i].Evaluate(p_score);
				num2 += num4 * tasks[i].weight;
				text = text + tasks[i].type.ToString() + " - valid[" + (obj != null) + "] v[" + p_score + "] r[" + num4.ToString("0.00") + "] rw[" + num4 * tasks[i].weight + "]\n";
			}
			float result = ((num <= 0f) ? 0f : (num2 / num));
			text = text + "=== sum[" + num2.ToString("0.00") + "] wsum[" + num.ToString("0.00") + "] result[" + result.ToString("0.00") + "]";
			Debug.Log(text);
			return result;
		}

		public float Evaluate(DataFlow p_data)
		{
			object[] array = new object[tasks.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = 0f;
			}
			if (!p_data)
			{
				Debug.LogWarning("DRLMissionScore> Invalid Data Flow");
				return Evaluate(array);
			}
			foreach (KeyValuePair<string, object> datum in p_data.data)
			{
				string key = datum.Key;
				object value = datum.Value;
				if (key.Contains("Count") || key.Contains("TimeMin") || key.Contains("TimeOut"))
				{
					FNMissionScoreType p_type = Reflection<object>.GetEnum<FNMissionScoreType>(key);
					float num = ((value is float) ? Reflection<object>.Cast<float>(value) : float.NaN);
					int taskIndex = GetTaskIndex(p_type);
					if (taskIndex >= 0)
					{
						array[taskIndex] = num;
					}
				}
			}
			Debug.Log("DRLMission> score-list[" + Format.Join(",", array) + "]");
			return Evaluate(array);
		}
	}
}
