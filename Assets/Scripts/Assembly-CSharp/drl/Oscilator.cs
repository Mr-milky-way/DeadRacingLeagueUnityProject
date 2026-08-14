using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl
{
	public class Oscilator : MonoBehaviour
	{
		public List<Transform> targets;

		public Vector3 offset;

		public Vector3 position;

		public Vector3 rotation;

		public Vector3[] speed;

		public Vector3[] scale;

		public List<Vector3> startPosition;

		public List<Vector3> startRotation;

		public float randomScale;

		public Vector3[] randomSeeds;

		private List<Vector3> m_wave_values;

		private List<Transform> m_targets;

		protected void Awake()
		{
			m_wave_values = new List<Vector3>();
			m_targets = new List<Transform>();
			startPosition = new List<Vector3>();
			startRotation = new List<Vector3>();
			int num = Mathf.Min(speed.Length, scale.Length);
			randomSeeds = new Vector3[num];
			for (int i = 0; i < num; i++)
			{
				randomSeeds[i] = UnityEngine.Random.insideUnitSphere;
			}
		}

		protected void Update()
		{
			Vector3 zero = Vector3.zero;
			float num = Time.time * ((float)Math.PI / 180f);
			int num2 = Mathf.Min(speed.Length, scale.Length);
			List<Transform> list = m_targets;
			list.Clear();
			if (targets.Count <= 0)
			{
				list.Add(base.transform);
			}
			else
			{
				list.AddRange(targets);
			}
			if (startPosition.Count != list.Count)
			{
				for (int i = 0; i < list.Count; i++)
				{
					startPosition.Add(list[i].localPosition);
				}
			}
			if (startRotation.Count != list.Count)
			{
				for (int j = 0; j < list.Count; j++)
				{
					startRotation.Add(list[j].localEulerAngles);
				}
			}
			m_wave_values.Clear();
			for (int k = 0; k < list.Count; k++)
			{
				Vector3 vector = offset * k * ((float)Math.PI / 180f);
				zero = Vector3.zero;
				for (int l = 0; l < num2; l++)
				{
					float num3 = Mathf.Sin(speed[l].x * num + vector.x);
					float num4 = Mathf.Sin(speed[l].y * num + vector.y);
					float num5 = Mathf.Sin(speed[l].z * num + vector.z);
					num3 *= scale[l].x * Mathf.Lerp(1f, randomSeeds[l].x * randomScale, Mathf.Clamp01(randomScale));
					num4 *= scale[l].y * Mathf.Lerp(1f, randomSeeds[l].y * randomScale, Mathf.Clamp01(randomScale));
					num5 *= scale[l].z * Mathf.Lerp(1f, randomSeeds[l].z * randomScale, Mathf.Clamp01(randomScale));
					zero.x += num3;
					zero.y += num4;
					zero.z += num5;
					m_wave_values.Add(zero);
				}
			}
			Vector3 vector2 = default(Vector3);
			Vector3 vector3 = default(Vector3);
			num2 = Mathf.Min(m_wave_values.Count, list.Count);
			for (int m = 0; m < num2; m++)
			{
				zero = m_wave_values[m];
				vector2.x = zero.x * position.x;
				vector2.y = zero.y * position.y;
				vector2.z = zero.z * position.z;
				vector3.x = zero.x * rotation.x;
				vector3.y = zero.y * rotation.y;
				vector3.z = zero.z * rotation.z;
				Transform obj = list[m];
				obj.localPosition = startPosition[m] + vector2;
				obj.localEulerAngles = startRotation[m] + vector3;
			}
		}
	}
}
