using UnityEngine;

namespace drl.sim
{
	public class DSBarometer : DroneSensor
	{
		[SerializeField]
		private float m_height;

		[SerializeField]
		private float m_heightAboveSurface;

		private RaycastHit[] hit;

		private float m_raycast_elapsed = 100f;

		public float pressure => 0f;

		public float height
		{
			get
			{
				if (!base.enabled)
				{
					return 0f;
				}
				return m_height;
			}
		}

		public float heightAboveSurface
		{
			get
			{
				if (!base.enabled)
				{
					return 0f;
				}
				return m_heightAboveSurface;
			}
		}

		protected override void Refresh(float p_dt)
		{
			if (hit == null)
			{
				hit = new RaycastHit[10];
			}
			Vector3 position = base.drone.position;
			m_height = position.y;
			int num = Physics.RaycastNonAlloc(position, DRLPhysics.Direction.down, hit, 1000f, DRLPhysics.Layers.Raycast_Ground, QueryTriggerInteraction.Ignore);
			m_heightAboveSurface = -1f;
			if (num <= 0)
			{
				return;
			}
			int num2 = Mathf.Min(num, hit.Length);
			m_heightAboveSurface = hit[0].distance;
			for (int i = 1; i < num2; i++)
			{
				if (m_heightAboveSurface > hit[i].distance)
				{
					m_heightAboveSurface = hit[i].distance;
					break;
				}
			}
		}
	}
}
