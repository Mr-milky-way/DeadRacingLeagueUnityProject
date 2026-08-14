using UnityEngine;

namespace drl.sim
{
	public static class DRLPhysics
	{
		public static class Layers
		{
			public static readonly int Raycast_IgnoreDrone = ~LayerMask.GetMask("DronePart", "Player", "Ignore Raycast");

			public static readonly int Raycast_Ground = ~LayerMask.GetMask("DronePart", "Player", "Ignore Raycast");

			public static readonly int Raycast_GroundEffect = LayerMask.GetMask("Default", "Water", "Terrain", "Blocked", "Collision", "DroneAsset");

			public static readonly int Raycast_Everything = -1;

			public static readonly int Raycast_DroneOnly = LayerMask.GetMask("DronePart", "Player");

			public static readonly int Raycast_PathGateIntersections = -1;

			public static readonly int Raycast_GateTriggers = LayerMask.GetMask("Gate");

			public static readonly int Raycast_BacktraceCollisions = ~LayerMask.GetMask("Ignore Raycast", "UI", "Player", "DronePart");

			public static readonly int Raycast_BacktraceTriggers = ~LayerMask.GetMask("Ignore Raycast", "UI", "Player", "Collision", "Water");
		}

		public static class Direction
		{
			public static readonly Vector3 down = Vector3.down;

			public static readonly Vector3 up = Vector3.up;
		}

		public static Vector3 Div(Vector3 v, Vector3 v2)
		{
			float num = ((Mathf.Abs(v2.x) <= Mathf.Epsilon) ? 0f : (1f / v2.x));
			float num2 = ((Mathf.Abs(v2.y) <= Mathf.Epsilon) ? 0f : (1f / v2.y));
			float num3 = ((Mathf.Abs(v2.z) <= Mathf.Epsilon) ? 0f : (1f / v2.z));
			return new Vector3(v.x * num, v.y * num2, v.z * num3);
		}
	}
}
