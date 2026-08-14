using UnityEngine;

namespace thelab.core
{
	public class Rotator : MonoBehaviour
	{
		public Vector3 speed;

		public Vector3 angle;

		protected void Awake()
		{
			angle = base.transform.localEulerAngles;
		}

		protected void Update()
		{
			angle += Time.deltaTime * speed;
			base.transform.localEulerAngles = angle;
		}

		public void Clear()
		{
			angle = Vector3.zero;
		}
	}
}
