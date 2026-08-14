using UnityEngine;

namespace drl.sim
{
	public class DSCollision : DroneSensor
	{
		public float restingOnSurfaceDistance = 0.05f;

		public float restingOnSurfaceGroundSpeedThreshold = 0.01f;

		public float restingOnSurfaceFallSpeedThreshold = 0.001f;

		public float crashThreshold = 90f;

		private int collisionCount;

		private RaycastHit[] hits;

		public bool active
		{
			get
			{
				return collisionCount > 0;
			}
			set
			{
				collisionCount = 2;
			}
		}

		public bool RestingOnSurface
		{
			get
			{
				if (hits == null || hits.Length < 20)
				{
					hits = new RaycastHit[20];
				}
				int num = Physics.RaycastNonAlloc(base.drone.position, DRLPhysics.Direction.down, hits, restingOnSurfaceDistance, DRLPhysics.Layers.Raycast_Everything, QueryTriggerInteraction.Ignore);
				for (int i = 0; i < hits.Length && i < num; i++)
				{
					RaycastHit raycastHit = hits[i];
					if (!raycastHit.transform.IsChildOf(base.drone.transform))
					{
						return true;
					}
				}
				return false;
			}
		}

		protected override void Refresh(float p_dt)
		{
			if (collisionCount > 0)
			{
				collisionCount--;
			}
		}
	}
}
