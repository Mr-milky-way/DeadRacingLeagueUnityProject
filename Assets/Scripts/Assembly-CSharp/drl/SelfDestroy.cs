using UnityEngine;

namespace drl
{
	public class SelfDestroy : MonoBehaviour
	{
		public float delay;

		protected void Awake()
		{
			if (delay > 0f)
			{
				Invoke("Destroy", delay);
			}
			else
			{
				Destroy();
			}
		}

		public void Destroy()
		{
			Object.Destroy(base.gameObject);
		}
	}
}
