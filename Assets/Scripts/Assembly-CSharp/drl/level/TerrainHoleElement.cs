using UnityEngine;

namespace drl.level
{
	public class TerrainHoleElement : MonoBehaviour
	{
		public float radius;

		private TerrainHole m_manager;

		public TerrainHole manager
		{
			get
			{
				if ((bool)m_manager)
				{
					return m_manager;
				}
				Transform parent = base.transform.parent;
				while ((bool)parent)
				{
					m_manager = parent.GetComponent<TerrainHole>();
					if ((bool)m_manager)
					{
						return m_manager;
					}
					parent = parent.parent;
				}
				return m_manager;
			}
		}

		protected void OnDrawGizmos()
		{
			Gizmos.color = Color.black;
			Gizmos.DrawWireSphere(base.transform.position, radius);
			if ((bool)manager)
			{
				manager.Refresh();
			}
		}
	}
}
