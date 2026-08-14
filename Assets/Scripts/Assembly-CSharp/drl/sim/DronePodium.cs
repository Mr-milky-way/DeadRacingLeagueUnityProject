using UnityEngine;

namespace drl.sim
{
	public class DronePodium : DRLAsset
	{
		private Transform m_spawn;

		public Transform spawn
		{
			get
			{
				if (!m_spawn)
				{
					return m_spawn = base.transform.Find("node-spawn");
				}
				return m_spawn;
			}
		}

		public override string GetPrefix()
		{
			return "PD";
		}
	}
}
