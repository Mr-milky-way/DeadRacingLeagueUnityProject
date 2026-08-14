using UnityEngine;

namespace drl.sim
{
	public class DroneCollisionSleep : MonoBehaviour
	{
		public int maxCollision = 2;

		public int collisionCount;

		public bool willSleep;

		private Rigidbody m_rb;

		public void Clear()
		{
			collisionCount = 0;
			willSleep = false;
		}

		public void OnCollisionEnter(Collision p_collision)
		{
			if (p_collision.transform.IsChildOf(base.transform))
			{
				return;
			}
			collisionCount++;
			if (collisionCount < maxCollision)
			{
				return;
			}
			collisionCount = maxCollision;
			if (!willSleep)
			{
				willSleep = true;
				Rigidbody rigidbody = (m_rb ? m_rb : (m_rb = GetComponent<Rigidbody>()));
				if ((bool)rigidbody)
				{
					rigidbody.velocity = Vector3.zero;
					rigidbody.angularVelocity = Vector3.zero;
					rigidbody.Sleep();
				}
				Object.Destroy(this);
			}
		}
	}
}
