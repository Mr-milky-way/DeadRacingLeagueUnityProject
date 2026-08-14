using UnityEngine;

namespace thelab.core
{
	public class RandomTransform : MonoBehaviour
	{
		public Vector3 minRotation;

		public Vector3 maxRotation;

		public Vector3 minPosition;

		public Vector3 maxPosition;

		public Vector3 minScale;

		public Vector3 maxScale;

		public bool applyOnAwake;

		protected void Awake()
		{
			if (applyOnAwake)
			{
				Apply();
			}
		}

		public void Apply()
		{
			Vector3 localEulerAngles = base.transform.localEulerAngles;
			float x = minRotation.x;
			float x2 = maxRotation.x;
			localEulerAngles.x += Random.Range(x, x2);
			x = minRotation.y;
			x2 = maxRotation.y;
			localEulerAngles.y += Random.Range(x, x2);
			x = minRotation.z;
			x2 = maxRotation.z;
			localEulerAngles.z += Random.Range(x, x2);
			base.transform.localEulerAngles = localEulerAngles;
			localEulerAngles = base.transform.localPosition;
			x = minPosition.x;
			x2 = maxPosition.x;
			localEulerAngles.x += Random.Range(x, x2);
			x = minPosition.y;
			x2 = maxPosition.y;
			localEulerAngles.y += Random.Range(x, x2);
			x = minPosition.z;
			x2 = maxPosition.z;
			localEulerAngles.z += Random.Range(x, x2);
			base.transform.localPosition = localEulerAngles;
			localEulerAngles = base.transform.localScale;
			x = minScale.x;
			x2 = maxScale.x;
			localEulerAngles.x += Random.Range(x, x2);
			x = minScale.y;
			x2 = maxScale.y;
			localEulerAngles.y += Random.Range(x, x2);
			x = minScale.z;
			x2 = maxScale.z;
			localEulerAngles.z += Random.Range(x, x2);
			base.transform.localScale = localEulerAngles;
		}
	}
}
