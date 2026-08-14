using UnityEngine;

namespace thelab.core
{
	public class SplineLayoutElement : MonoBehaviour
	{
		public bool ignoreLayout;

		public bool ignoreRotation;

		public bool useGlobalUp = true;

		public bool groundAlign;

		public bool snap;

		public Vector3 position;

		public Vector3 rotation;

		public float length;
	}
}
