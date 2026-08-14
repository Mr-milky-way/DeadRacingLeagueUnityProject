using UnityEngine;

namespace thelab.core
{
	public class FNColliderElementHolder : MonoBehaviour
	{
		public ColliderEventComponent targetCollider;

		[ContextMenu("Refresh")]
		public void Refresh()
		{
			if ((bool)targetCollider)
			{
				targetCollider.gameObject.name = base.gameObject.name;
			}
		}
	}
}
