using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MELayoutSurface : MonoBehaviour
	{
		private static TransformVector[] samplesPool = new TransformVector[5000];

		public List<Vector3> random;

		public int count;

		public Vector3 offsetPosition;

		public Vector3 ditherPosition;

		public bool orientEnabled;

		public Vector3 orientOffset;

		public Vector3 orientStep;

		private Thread m_data_refresh_thd;

		public TransformVector[] samples => samplesPool;

		protected void Awake()
		{
			if (random == null)
			{
				random = new List<Vector3>();
			}
		}

		public virtual void Generate(int p_count, Action p_callback = null)
		{
			if (p_count != random.Count)
			{
				Randomize();
			}
			GenerateApply(p_count, p_callback);
		}

		private void GenerateApply(int p_count, Action p_callback = null)
		{
			count = p_count;
			for (int i = 0; i < count; i++)
			{
				samples[i].Set(Vector3.zero);
				float p_ratio = ((count - 1 <= 0) ? 0f : ((float)i / (float)(count - 1)));
				samples[i] = OnAsyncDataStep(i, p_ratio, samples[i]);
			}
			OnAsyncDataRefresh();
			if (p_callback != null)
			{
				Activity.RunOnce(p_callback);
			}
		}

		protected virtual TransformVector OnAsyncDataStep(int p_index, float p_ratio, TransformVector p_sample)
		{
			p_sample.Set(Vector3.zero);
			return p_sample;
		}

		protected virtual void OnAsyncDataRefresh()
		{
		}

		public void Randomize()
		{
			random.Clear();
			for (int i = 0; i < 300; i++)
			{
				random.Add(UnityEngine.Random.insideUnitSphere);
			}
		}

		public Vector3 GetRandom(int p_index)
		{
			if (random.Count > 0)
			{
				return random[p_index % random.Count];
			}
			return UnityEngine.Random.insideUnitSphere;
		}

		public TransformVector Get(int p_index)
		{
			int num = Mathf.Clamp(p_index, 0, count - 1);
			return samples[num];
		}

		public int GetNextIndexByDistance(int p_index, float p_distance)
		{
			float num = p_distance;
			int num2 = p_index + 1;
			if (num2 >= count)
			{
				return -1;
			}
			for (num2 = p_index + 1; num2 < count; num2++)
			{
				TransformVector transformVector = Get(p_index - 1);
				TransformVector transformVector2 = Get(p_index);
				float num3 = Vector3.Distance(transformVector.position, transformVector2.position);
				if (!(num3 > num))
				{
					break;
				}
				num -= num3;
			}
			if (num <= 0f)
			{
				return num2;
			}
			return -1;
		}

		public TransformVector GetOffset(int p_index, float p_distance)
		{
			int nextIndexByDistance = GetNextIndexByDistance(p_index, p_distance);
			if (nextIndexByDistance < 0)
			{
				return default(TransformVector);
			}
			return Get(nextIndexByDistance);
		}

		public TransformVector GetNormalized(float p_ratio)
		{
			float t = Mathf.Clamp01(p_ratio);
			float b = count - 1;
			int p_index = (int)Mathf.Lerp(0f, b, t);
			return Get(p_index);
		}

		public TransformVector GetOffset(float p_ratio, float p_distance)
		{
			float t = Mathf.Clamp01(p_ratio);
			float b = count - 1;
			int p_index = (int)Mathf.Lerp(0f, b, t);
			return GetOffset(p_index, p_distance);
		}
	}
}
