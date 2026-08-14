using System.Collections;
using UnityEngine;

public class TreeExplosion : MonoBehaviour
{
	public float BlastRange = 30f;

	public float BlastForce = 30000f;

	public GameObject DeadReplace;

	public GameObject Explosion;

	private void Explode()
	{
		Object.Instantiate(Explosion, base.transform.position, Quaternion.identity);
		TerrainData terrainData = Terrain.activeTerrain.terrainData;
		ArrayList arrayList = new ArrayList();
		TreeInstance[] treeInstances = terrainData.treeInstances;
		for (int i = 0; i < treeInstances.Length; i++)
		{
			TreeInstance treeInstance = treeInstances[i];
			if (Vector3.Distance(Vector3.Scale(treeInstance.position, terrainData.size) + Terrain.activeTerrain.transform.position, base.transform.position) < BlastRange)
			{
				GameObject obj = Object.Instantiate(DeadReplace, Vector3.Scale(treeInstance.position, terrainData.size) + Terrain.activeTerrain.transform.position, Quaternion.identity);
				obj.GetComponent<Rigidbody>().maxAngularVelocity = 1f;
				obj.GetComponent<Rigidbody>().AddExplosionForce(BlastForce, base.transform.position, 20f + BlastRange * 5f, -20f);
			}
			else
			{
				arrayList.Add(treeInstance);
			}
		}
		terrainData.treeInstances = (TreeInstance[])arrayList.ToArray(typeof(TreeInstance));
	}

	private void Update()
	{
		if (Input.GetButtonDown("Fire1"))
		{
			Explode();
		}
	}
}
