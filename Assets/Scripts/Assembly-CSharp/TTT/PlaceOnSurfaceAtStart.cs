using UnityEngine;

namespace TTT
{
	public class PlaceOnSurfaceAtStart : MonoBehaviour
	{
		public float PivotOffset;

		public float RayOffset = 100f;

		public LayerMask GroundLayer;

		private void Start()
		{
			if (((1 << base.gameObject.layer) & (int)GroundLayer) != 0)
			{
				Debug.LogWarning("GameObject is in the same layer as raycasting layer, raycast might hit gameobject instead of ground");
			}
			if (Physics.Raycast(base.transform.position + Vector3.up * RayOffset, -Vector3.up, out var hitInfo, float.PositiveInfinity, GroundLayer))
			{
				float num = hitInfo.distance - PivotOffset - RayOffset;
				base.transform.Translate(-Vector3.up * num, Space.World);
			}
		}
	}
}
