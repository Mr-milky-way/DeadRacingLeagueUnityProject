using UnityEngine;
using drl.game;

public class NetCollisionDetection : MonoBehaviour
{
	public UAVNetController controller;

	private void OnTriggerEnter(Collider c)
	{
		if (c.CompareTag("UAVCloth"))
		{
			if (base.gameObject.GetComponent<Rigidbody>() == null && controller.mode == UAVNetController.Mode.net)
			{
				controller.HitTrigger(base.gameObject);
				return;
			}
			controller.RegisterContact(base.gameObject, GetContact(c));
		}
		if (c.tag == "UAVNetGun")
		{
			if (base.gameObject.GetComponent<Rigidbody>() == null)
			{
				controller.HitTrigger(base.gameObject);
				return;
			}
			controller.RegisterContact(base.gameObject, GetContact(c));
			c.transform.SetParent(null);
			c.transform.SetParent(base.transform);
		}
	}

	private Vector3 GetContact(Collider cl)
	{
		return cl.transform.InverseTransformPoint(base.gameObject.transform.position);
	}
}
