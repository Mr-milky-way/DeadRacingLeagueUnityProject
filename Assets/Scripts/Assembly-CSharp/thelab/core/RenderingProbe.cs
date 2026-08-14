using System;
using UnityEngine;

namespace thelab.core
{
	public class RenderingProbe : MonoBehaviour
	{
		[Serializable]
		public struct Sample
		{
			public Color ambientColor;

			public Color fogColor;

			public float fogDensity;

			public float fogTexBlend;

			public static Sample Lerp(Sample a, Sample b, float r)
			{
				return new Sample
				{
					ambientColor = Color.Lerp(a.ambientColor, b.ambientColor, r),
					fogColor = Color.Lerp(a.fogColor, b.fogColor, r),
					fogDensity = Mathf.Lerp(a.fogDensity, b.fogDensity, r),
					fogTexBlend = Mathf.Lerp(a.fogTexBlend, b.fogTexBlend, r)
				};
			}
		}

		public int importance;

		public Color ambientColor = Color.gray;

		public Color fogColor = Color.gray;

		[Range(0f, 0.2f)]
		public float fogDensity = 0.005f;

		[Range(0f, 1f)]
		public float fogTexBlend = 1f;

		public Sample sample => new Sample
		{
			ambientColor = ambientColor,
			fogColor = fogColor,
			fogDensity = fogDensity,
			fogTexBlend = fogTexBlend
		};

		public virtual float GetDistance(Vector3 p_position)
		{
			return Vector3.Distance(p_position, base.transform.position);
		}

		public virtual float GetIntensity(Vector3 p_position)
		{
			return 1f;
		}

		protected virtual void OnDrawGizmos()
		{
			Vector3 position = base.transform.position;
			float constantSize = Hierarchy.GetConstantSize(position, 5f);
			Gizmos.color = ambientColor;
			Gizmos.DrawWireSphere(position, constantSize + 1f);
			Gizmos.color = fogColor;
			Gizmos.DrawSphere(position, constantSize);
		}
	}
}
