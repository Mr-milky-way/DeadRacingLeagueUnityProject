using UnityEngine;

namespace thelab.core
{
	public class OrbitCameraPreset : MonoBehaviour
	{
		public bool useOverridenInitRotation;

		public Vector2 initialRotationOverride;

		public bool useOverridenInitDistance;

		public float initialDistanceOverride;

		[HideInInspector]
		public Vector2 angle;

		[HideInInspector]
		public float distance;

		public OrbitConstraint constraint => GetComponent<OrbitConstraint>();

		public Vector2 initialRotation
		{
			get
			{
				if (useOverridenInitRotation)
				{
					return initialRotationOverride;
				}
				return (constraint.angleMin + constraint.angleMax) * 0.5f;
			}
		}

		public float initialDistance
		{
			get
			{
				if (useOverridenInitDistance)
				{
					return initialDistanceOverride;
				}
				return (constraint.distanceMin + constraint.distanceMax) * 0.5f;
			}
		}

		public void initPreset()
		{
			angle = initialRotation;
			distance = initialDistance;
		}
	}
}
