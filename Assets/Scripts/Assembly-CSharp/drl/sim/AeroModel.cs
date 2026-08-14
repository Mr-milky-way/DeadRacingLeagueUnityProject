using UnityEngine;

namespace drl.sim
{
	public class AeroModel
	{
		public enum SurfacePlane
		{
			x = 0,
			y = 1,
			z = 2
		}

		public const float c_airDensity = 1.225f;

		public const float c_DragCoeficient = 2.1f;

		protected Vector3 m_dragForce;

		protected Vector3 m_liftForce;

		protected Vector3 m_totalForce;

		protected Vector3 m_moment;

		private static GameObject cubePrefab;

		public Vector3 terminalVelocity { get; protected set; }

		public float Cd { get; protected set; }

		public float Cl { get; protected set; }

		public Vector3 dragForce => m_dragForce;

		public Vector3 liftForce => m_liftForce;

		public Vector3 totalForce => m_totalForce;

		public Vector3 moment => m_moment;

		public virtual void RecalculateForces(Drone p_drone, float p_dt, float p_mass, Vector3 p_transformUp, Vector3 p_velocity, Vector3 p_angularVelocity, Quaternion p_orientation)
		{
			Step(p_drone, p_dt, p_mass, p_transformUp, p_velocity, p_angularVelocity, p_orientation);
		}

		public virtual void Step(Drone p_drone, float p_dt, float p_mass, Vector3 p_transformUp, Vector3 p_velocity, Vector3 p_angularVelocity, Quaternion p_orientation)
		{
			m_dragForce = Vector3.zero;
			m_liftForce = Vector3.zero;
			m_totalForce = Vector3.zero;
			m_moment = Vector3.zero;
			terminalVelocity = Vector3.zero;
			Cd = 0f;
			Cl = 0f;
		}

		public virtual void Reset()
		{
		}

		private static bool Trace(Transform p_target, float p_precision, float p_position1, float p_position2, SurfacePlane p_surface = SurfacePlane.y)
		{
			Vector3 origin;
			Vector3 direction;
			switch (p_surface)
			{
			case SurfacePlane.x:
				origin = p_target.TransformPoint(0.5f, p_position1, p_position2);
				direction = -p_target.transform.right;
				break;
			case SurfacePlane.z:
				origin = p_target.TransformPoint(p_position1, p_position2, 0.5f);
				direction = -p_target.forward;
				break;
			default:
				origin = p_target.TransformPoint(p_position1, 0.5f, p_position2);
				direction = -p_target.up;
				break;
			}
			RaycastHit[] array = Physics.SphereCastAll(origin, p_precision / 4f, direction, 1f, -1, QueryTriggerInteraction.Ignore);
			foreach (RaycastHit raycastHit in array)
			{
				if (raycastHit.transform.IsChildOf(p_target))
				{
					return true;
				}
			}
			return false;
		}

		public static float CalculateSurface(Transform p_target, float p_precision, SurfacePlane p_surface = SurfacePlane.y)
		{
			bool flag = true;
			bool flag2 = true;
			int num = 1;
			int num2 = 0;
			if (Trace(p_target, p_precision, 0f, 0f, p_surface))
			{
				num2++;
			}
			while (flag || flag2)
			{
				flag = false;
				for (int i = -num; i <= num; i++)
				{
					if (Trace(p_target, p_precision, p_precision * (float)i, p_precision * (float)num, p_surface))
					{
						flag = true;
						num2++;
					}
					if (Trace(p_target, p_precision, p_precision * (float)i, (0f - p_precision) * (float)num, p_surface))
					{
						flag = true;
						num2++;
					}
					if (i != -num && i != num)
					{
						if (Trace(p_target, p_precision, p_precision * (float)num, p_precision * (float)i, p_surface))
						{
							flag = true;
							num2++;
						}
						if (Trace(p_target, p_precision, (0f - p_precision) * (float)num, p_precision * (float)i, p_surface))
						{
							flag = true;
							num2++;
						}
					}
				}
				if (flag2 && flag)
				{
					flag2 = false;
				}
				num++;
				if (flag2 && (float)num * p_precision > 0.5f)
				{
					flag2 = false;
				}
			}
			return (float)num2 * p_precision * p_precision;
		}

		public static void VisualizeSurface(Transform p_target, float p_precision, SurfacePlane p_surface = SurfacePlane.y)
		{
			bool flag = true;
			bool flag2 = true;
			string p_surface2 = p_surface.ToString();
			int num = 1;
			if (Trace(p_target, p_precision, 0f, 0f, p_surface))
			{
				SpawnCube(p_target, p_precision, 0f, 0f, p_surface2);
			}
			while (flag || flag2)
			{
				flag = false;
				for (int i = -num; i <= num; i++)
				{
					if (Trace(p_target, p_precision, p_precision * (float)i, p_precision * (float)num, p_surface))
					{
						flag = true;
						SpawnCube(p_target, p_precision, p_precision * (float)i, p_precision * (float)num, p_surface2);
					}
					if (Trace(p_target, p_precision, p_precision * (float)i, (0f - p_precision) * (float)num, p_surface))
					{
						flag = true;
						SpawnCube(p_target, p_precision, p_precision * (float)i, (0f - p_precision) * (float)num, p_surface2);
					}
					if (i != -num && i != num)
					{
						if (Trace(p_target, p_precision, p_precision * (float)num, p_precision * (float)i, p_surface))
						{
							flag = true;
							SpawnCube(p_target, p_precision, p_precision * (float)num, p_precision * (float)i, p_surface2);
						}
						if (Trace(p_target, p_precision, (0f - p_precision) * (float)num, p_precision * (float)i, p_surface))
						{
							flag = true;
							SpawnCube(p_target, p_precision, (0f - p_precision) * (float)num, p_precision * (float)i, p_surface2);
						}
					}
				}
				if (flag2 && flag)
				{
					flag2 = false;
				}
				num++;
				if (flag2 && (float)num * p_precision > 0.5f)
				{
					flag2 = false;
				}
			}
		}

		private static void SpawnCube(Transform p_target, float p_precision, float p_position1, float p_position2, string p_surface = "y")
		{
			if (cubePrefab == null)
			{
				cubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Object.DestroyImmediate(cubePrefab.GetComponent<Collider>());
			}
			Transform transform = p_target.Find(p_surface);
			if (!transform)
			{
				transform = new GameObject(p_surface).transform;
				transform.parent = p_target;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;
			}
			Vector3 position = p_surface switch
			{
				"x" => p_target.TransformPoint(0f, p_position1, p_position2), 
				"z" => p_target.TransformPoint(p_position1, p_position2, 0f), 
				_ => p_target.TransformPoint(p_position1, 0f, p_position2), 
			};
			Transform transform2 = Object.Instantiate(cubePrefab).transform;
			transform2.parent = transform;
			transform2.position = position;
			transform2.localScale = Vector3.one * p_precision;
		}

		public static Vector3 CalculateSurfaces(Transform p_target, float p_precision = 0.005f)
		{
			return new Vector3(CalculateSurface(p_target, p_precision, SurfacePlane.x), CalculateSurface(p_target, p_precision), CalculateSurface(p_target, p_precision, SurfacePlane.z));
		}

		public static void VisualizeSurfaces(Transform p_target, float p_precision = 0.005f)
		{
			VisualizeSurface(p_target, p_precision, SurfacePlane.x);
			VisualizeSurface(p_target, p_precision);
			VisualizeSurface(p_target, p_precision, SurfacePlane.z);
		}

		public static Vector3 CalculateDragFactors(Transform p_target, float p_precision = 0.005f)
		{
			Vector3 result = new Vector3(CalculateSurface(p_target, p_precision, SurfacePlane.x), CalculateSurface(p_target, p_precision), CalculateSurface(p_target, p_precision, SurfacePlane.z));
			result.x = 1.225f * result.x * 2.1f / 2f;
			result.y = 1.225f * result.y * 2.1f / 2f;
			result.z = 1.225f * result.z * 2.1f / 2f;
			return result;
		}
	}
}
