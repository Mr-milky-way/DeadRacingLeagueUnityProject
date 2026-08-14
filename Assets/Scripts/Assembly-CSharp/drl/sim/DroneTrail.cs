using UnityEngine;

namespace drl.sim
{
	[RequireComponent(typeof(TrailRenderer))]
	public class DroneTrail : DronePart
	{
		[SerializeField]
		private TrailRenderer m_renderer;

		public TrailRenderer renderer
		{
			get
			{
				if (!this)
				{
					return null;
				}
				if (!base.gameObject)
				{
					return null;
				}
				if (!m_renderer)
				{
					return m_renderer = GetComponent<TrailRenderer>();
				}
				return m_renderer;
			}
		}

		public override string GetPrefix()
		{
			return "TR";
		}

		protected override void OnInitialize()
		{
			DroneCrashNode component = GetComponent<DroneCrashNode>();
			if ((bool)component)
			{
				Object.Destroy(component);
			}
			if ((bool)renderer)
			{
				renderer.gameObject.layer = 17;
			}
		}
	}
}
