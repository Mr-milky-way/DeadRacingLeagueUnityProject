using UnityEngine;

[AddComponentMenu("Relief Terrain/Helpers/Resolve Hole Collision")]
[RequireComponent(typeof(Collider))]
public class ResolveHoleCollision : MonoBehaviour
{
	public Collider[] entranceTriggers;

	public TerrainCollider[] terrainColliders;

	public float checkOffset = 1f;

	public bool StartBelowGroundSurface;

	private TerrainCollider terrainColliderForUpdate;

	private Collider _collider;

	private Rigidbody _rigidbody;

	private void Awake()
	{
		_collider = GetComponent<Collider>();
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
			if (terrainColliders[j] != null && _collider != null)
			{
				Physics.IgnoreCollision(_collider, terrainColliders[j], ignore: true);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_collider == null)
		{
			return;
		}
		for (int i = 0; i < entranceTriggers.Length; i++)
		{
			if (entranceTriggers[i] == other)
			{
				for (int j = 0; j < terrainColliders.Length; j++)
				{
					Physics.IgnoreCollision(_collider, terrainColliders[j], ignore: true);
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
				Physics.IgnoreCollision(_collider, terrainColliders[i], ignore: false);
			}
		}
		terrainColliderForUpdate = null;
	}

	private void OnTriggerExit(Collider other)
	{
		if (_collider == null)
		{
			return;
		}
		for (int i = 0; i < entranceTriggers.Length; i++)
		{
			if (entranceTriggers[i] == other)
			{
				for (int j = 0; j < terrainColliders.Length; j++)
				{
					Physics.IgnoreCollision(_collider, terrainColliders[j], ignore: false);
				}
			}
		}
	}
}
