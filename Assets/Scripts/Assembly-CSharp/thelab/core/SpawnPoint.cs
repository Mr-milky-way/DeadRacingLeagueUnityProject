using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace thelab.core
{
	public struct SpawnPoint
	{
		public Vector3 position;

		public Quaternion rotation;

		public SpawnPoint(Vector3 p_pos, Quaternion p_rot)
		{
			position = p_pos;
			rotation = p_rot;
		}

		public static SpawnPoint FindSafeSpot(Transform p_target, string[] p_ignoreLayers)
		{
			return FindSafeSpot(p_target, (p_ignoreLayers != null) ? (~LayerMask.GetMask(p_ignoreLayers)) : (-1));
		}

		public static SpawnPoint FindSafeSpot(Transform p_target, int p_layerMask = -1)
		{
			return FindSafeSpot(p_target.position, p_target, p_layerMask);
		}

		public static SpawnPoint FindSafeSpot(Transform p_target, float p_maxDistance, int p_layerMask = -1)
		{
			return FindSafeSpot(p_target.position, p_target, p_layerMask, p_maxDistance);
		}

		public static SpawnPoint FindSafeSpot(Vector3 p_sourcePosition, Transform p_target, string[] p_ignoreLayers)
		{
			return FindSafeSpot(p_sourcePosition, p_target, (p_ignoreLayers != null) ? (~LayerMask.GetMask(p_ignoreLayers)) : (-1));
		}

		public static SpawnPoint FindSafeSpot(Vector3 p_sourcePosition, Transform p_target, int p_layerMask = -1, float p_maxDistance = 50f)
		{
			if (p_target == null)
			{
				return new SpawnPoint(Vector3.zero, Quaternion.identity);
			}
			return FindSafeSpot(new Vector3[5]
			{
				p_sourcePosition,
				p_sourcePosition + p_target.forward,
				p_sourcePosition - p_target.forward,
				p_sourcePosition + p_target.right,
				p_sourcePosition - p_target.right
			}, p_target.rotation, p_layerMask, p_maxDistance);
		}

		public static SpawnPoint FindSafeSpot(Vector3[] p_sourcePositions, Quaternion p_rotation, string[] p_ignoreLayers)
		{
			return FindSafeSpot(p_sourcePositions, p_rotation, (p_ignoreLayers != null) ? (~LayerMask.GetMask(p_ignoreLayers)) : (-1));
		}

		public static SpawnPoint FindSafeSpot(Vector3[] p_sourcePositions, Quaternion p_rotation, int p_layerMask = -1, float p_maxDistance = 50f)
		{
			if (p_sourcePositions == null || p_sourcePositions.Length == 0)
			{
				return new SpawnPoint(Vector3.zero, Quaternion.identity);
			}
			Vector3 o_position = p_sourcePositions[0];
			bool flag = false;
			for (int i = 0; i < p_sourcePositions.Length; i++)
			{
				if (flag)
				{
					break;
				}
				if (Physics.Raycast(p_sourcePositions[i] + Vector3.up * 0.25f, Vector3.down, out var hitInfo, p_maxDistance, p_layerMask))
				{
					p_sourcePositions[i] = hitInfo.point;
				}
				flag = GetClosestNavMeshPoint(p_sourcePositions[i], out o_position);
				if (flag)
				{
					flag = !Physics.CheckSphere(o_position + Vector3.up * 0.15f, 0.149f, p_layerMask, QueryTriggerInteraction.Ignore);
				}
			}
			for (int j = 0; j < p_sourcePositions.Length; j++)
			{
				if (flag)
				{
					break;
				}
				o_position = p_sourcePositions[j];
				flag = !Physics.CheckSphere(o_position + Vector3.up * 0.15f, 0.149f, p_layerMask, QueryTriggerInteraction.Ignore);
			}
			if (!flag)
			{
				o_position = p_sourcePositions[0];
			}
			return new SpawnPoint(o_position, p_rotation);
		}

		public static SpawnPoint FindSafeSpot(int p_id, List<float> p_nodeDistances, Vector3Spline p_path, string[] p_ignoreLayers)
		{
			return FindSafeSpot(p_id, p_nodeDistances, p_path, (p_ignoreLayers != null) ? (~LayerMask.GetMask(p_ignoreLayers)) : (-1));
		}

		public static SpawnPoint FindSafeSpot(int p_id, List<float> p_nodeDistances, Vector3Spline p_path, int p_layerMask = -1)
		{
			if (p_id - 1 < p_nodeDistances.Count)
			{
				Vector3[] array = new Vector3[5];
				Quaternion identity = Quaternion.identity;
				float num = p_nodeDistances[p_id - 1];
				if (p_id < p_nodeDistances.Count)
				{
					float num2 = (num + p_nodeDistances[p_id]) / 2f;
					float num3 = Mathf.Min(num + 2f, num2);
					float p_distance = ((num3 < num2) ? (num + 1f) : ((num + num2) / 2f));
					array[0] = p_path.Get(p_distance);
					array[1] = p_path.Get(num3);
				}
				else
				{
					array[0] = p_path.Get(num + 0.5f);
					array[1] = p_path.Get(num + 1f);
				}
				array[2] = p_path.Get(num);
				if (p_id > 1)
				{
					float num4 = (num + p_nodeDistances[p_id - 2]) / 2f;
					float num5 = Mathf.Max(num - 2f, num4);
					float p_distance2 = ((num5 > num4) ? (num - 1f) : ((num + num4) / 2f));
					array[3] = p_path.Get(p_distance2);
					array[4] = p_path.Get(num5);
				}
				else
				{
					array[3] = p_path.Get(num - 0.5f);
					array[4] = p_path.Get(num - 1f);
				}
				identity = Quaternion.LookRotation(array[1] - array[2], Vector3.up);
				return FindSafeSpot(array, identity, p_layerMask);
			}
			Vector3 p_towards = p_path.Get(p_path.length);
			return FindSafeSpot(p_path.Get(p_path.length * 0.999f), p_towards, p_layerMask);
		}

		public static SpawnPoint FindSafeSpot(Vector3 p_point, Vector3 p_towards, string[] p_ignoreLayers)
		{
			return FindSafeSpot(p_point, p_towards, (p_ignoreLayers != null) ? (~LayerMask.GetMask(p_ignoreLayers)) : (-1));
		}

		public static SpawnPoint FindSafeSpot(Vector3 p_point, Vector3 p_towards, int p_layerMask = -1)
		{
			Vector3[] array = new Vector3[5];
			Quaternion quaternion = Quaternion.LookRotation(p_towards - p_point, Vector3.up);
			Vector3 vector = quaternion * Vector3.forward;
			array[0] = p_point + vector * 0.5f;
			array[1] = p_point + vector;
			array[2] = p_point;
			array[3] = p_point - vector * 0.5f;
			array[4] = p_point - vector;
			return FindSafeSpot(array, quaternion, p_layerMask);
		}

		public static bool GetClosestNavMeshPoint(Vector3 p_target, out Vector3 o_position)
		{
			float[] array = new float[6] { 1f, 5f, 10f, 50f, 200f, 1000f };
			for (int i = 0; i < array.Length; i++)
			{
				NavMesh.SamplePosition(p_target, out var hit, array[i], -1);
				if (hit.hit)
				{
					o_position = hit.position;
					return true;
				}
			}
			o_position = Vector3.zero;
			return false;
		}
	}
}
