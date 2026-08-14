using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using drl.game;

public class StickyNet : MonoBehaviour
{
	private Cloth cloth;

	private Dictionary<int, float> distances = new Dictionary<int, float>();

	public UAVNetController controller;

	private void OnCollisionEnter(Collision c)
	{
		distances.Clear();
		if (!c.gameObject.CompareTag("UAVNetGun"))
		{
			return;
		}
		cloth = c.gameObject.GetComponent<Cloth>();
		c.gameObject.GetComponent<Rigidbody>().isKinematic = true;
		ClothSkinningCoefficient[] coefficients = cloth.coefficients;
		cloth.enabled = false;
		ContactPoint[] contacts = c.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			for (int j = 0; j < cloth.vertices.Length; j++)
			{
				distances.Add(j, Vector3.Distance(contactPoint.otherCollider.ClosestPoint(contactPoint.point), cloth.transform.TransformPoint(cloth.vertices[j])));
			}
		}
		IOrderedEnumerable<KeyValuePair<int, float>> orderedEnumerable = distances.OrderBy(delegate(KeyValuePair<int, float> kv)
		{
			KeyValuePair<int, float> keyValuePair = kv;
			return keyValuePair.Value;
		});
		int num = 0;
		foreach (KeyValuePair<int, float> item in orderedEnumerable)
		{
			if (num >= 6)
			{
				break;
			}
			coefficients[item.Key].maxDistance = 0f;
			num++;
		}
		cloth.coefficients = coefficients;
		cloth.enabled = true;
		cloth.useGravity = true;
	}
}
