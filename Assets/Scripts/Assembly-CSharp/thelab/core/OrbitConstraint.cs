using UnityEngine;

namespace thelab.core
{
	public class OrbitConstraint : MonoBehaviour
	{
		public float distanceMin = float.NegativeInfinity;

		public float distanceMax = float.PositiveInfinity;

		public bool useAngleXConstraint = true;

		public bool useAngleYConstraint = true;

		public Vector2 angleMin = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

		public Vector2 angleMax = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

		public void Set(float p_distance_min, float p_distance_max, Vector2 p_angle_min, Vector2 p_angle_max, bool p_use_angle_x, bool p_use_angle_y)
		{
			distanceMin = p_distance_min;
			distanceMax = p_distance_max;
			angleMin = p_angle_min;
			angleMax = p_angle_max;
			useAngleXConstraint = p_use_angle_x;
			useAngleYConstraint = p_use_angle_y;
		}

		public void Set(float p_distance_min, float p_distance_max)
		{
			Set(p_distance_min, p_distance_max, new Vector2(float.NegativeInfinity, float.NegativeInfinity), new Vector2(float.NegativeInfinity, float.NegativeInfinity), p_use_angle_x: true, p_use_angle_y: true);
		}

		public void Set(Vector2 p_angle_min, Vector2 p_angle_max)
		{
			Set(float.NegativeInfinity, float.PositiveInfinity, p_angle_min, p_angle_max, p_use_angle_x: true, p_use_angle_y: true);
		}

		public void Set(OrbitConstraint v)
		{
			Set(v.distanceMin, v.distanceMax, v.angleMin, v.angleMax, v.useAngleXConstraint, v.useAngleYConstraint);
		}

		public void Clear()
		{
			distanceMin = (angleMin.x = (angleMin.y = float.NegativeInfinity));
			distanceMax = (angleMax.x = (angleMax.y = float.PositiveInfinity));
		}

		private void Start()
		{
		}
	}
}
