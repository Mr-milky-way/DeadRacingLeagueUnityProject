using UnityEngine;

namespace TTT
{
	[ExecuteInEditMode]
	public class ForceGrassDistanceInEditor : MonoBehaviour
	{
		public float distance = 250f;

		private Terrain terrain;

		private void Start()
		{
			terrain = GetComponent<Terrain>();
			if (terrain == null)
			{
				Debug.LogError("This gameobject is not terrain, disabling forced details distance", base.gameObject);
				base.enabled = false;
			}
		}

		private void Update()
		{
			terrain.detailObjectDistance = distance;
		}
	}
}
