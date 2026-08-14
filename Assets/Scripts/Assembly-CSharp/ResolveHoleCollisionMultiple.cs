using UnityEngine;

[AddComponentMenu("Relief Terrain/Helpers/Resolve Hole Collision for multiple child colliders")]
public class ResolveHoleCollisionMultiple : MonoBehaviour
{
	public Collider[] childColliders;

	public Collider[] entranceTriggers;

	public TerrainCollider[] terrainColliders;

	public float checkOffset = 1f;

	public bool StartBelowGroundSurface;

	private TerrainCollider terrainColliderForUpdate;

	private Rigidbody _rigidbody;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		for (int i = 0; i < entranceTriggers.Length; i++)
		{
			if (entranceTriggers[i] != null)
			{
				entranceTriggers[i].isTrigger = true;
			}
		}
		if (!(_rigidbody != null) || !StartBelowGroundSurface)
		{
			return;
		}
		for (int j = 0; j < terrainColliders.Length; j++)
		{
			for (int k = 0; k < childColliders.Length; k++)
			{
				if (terrainColliders[j] != null && childColliders[k] != null)
				{
					Physics.IgnoreCollision(childColliders[k], terrainColliders[j], ignore: true);
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		for (int i = 0; i < entranceTriggers.Length; i++)
		{
			if (!(entranceTriggers[i] == other))
			{
				continue;
			}
			for (int j = 0; j < terrainColliders.Length; j++)
			{
				for (int k = 0; k < childColliders.Length; k++)
				{
					if (childColliders[k] != null && terrainColliders[j] != null)
					{
						Physics.IgnoreCollision(childColliders[k], terrainColliders[j], ignore: true);
					}
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (!terrainColliderForUpdate)
		{
			return;
		}
		RaycastHit hitInfo = default(RaycastHit);
		if (terrainColliderForUpdate.Raycast(new Ray(base.transform.position + Vector3.up * checkOffset, Vector3.down), out hitInfo, float.PositiveInfinity))
		{
			for (int i = 0; i < terrainColliders.Length; i++)
			{
				for (int j = 0; j < childColliders.Length; j++)
				{
					if (childColliders[j] != null && terrainColliders[i] != null)
					{
						Physics.IgnoreCollision(childColliders[j], terrainColliders[i], ignore: false);
					}
				}
			}
		}
		terrainColliderForUpdate = null;
	}

	private void OnTriggerExit(Collider other)
	{
		for (int i = 0; i < entranceTriggers.Length; i++)
		{
			if (!(entranceTriggers[i] == other))
			{
				continue;
			}
			for (int j = 0; j < terrainColliders.Length; j++)
			{
				for (int k = 0; k < childColliders.Length; k++)
				{
					if (childColliders[k] != null && terrainColliders[j] != null)
					{
						Physics.IgnoreCollision(childColliders[k], terrainColliders[j], ignore: false);
					}
				}
			}
		}
	}
}
