using UnityEngine;

namespace thelab.core
{
	public class TerrainQuality : MonoBehaviour
	{
		public string label = "";

		[SerializeField]
		private Terrain m_terrain;

		public bool drawDetails = true;

		[Range(0f, 250f)]
		public float detailDistance = 80f;

		[Range(0f, 1f)]
		public float detailDensity = 1f;

		[Range(0f, 2000f)]
		public float treeDistance = 100f;

		[Range(5f, 2000f)]
		public float billboardStart = 50f;

		public Terrain target
		{
			get
			{
				if (!m_terrain)
				{
					return m_terrain = GetComponent<Terrain>();
				}
				return m_terrain;
			}
			set
			{
				m_terrain = value;
			}
		}

		private void Start()
		{
		}

		public void OnEnable()
		{
			Apply();
		}

		protected void Apply()
		{
			if ((bool)target)
			{
				Debug.Log("TerrainQuality> Apply Quality.");
				target.drawTreesAndFoliage = drawDetails;
				target.detailObjectDistance = detailDistance;
				target.detailObjectDensity = detailDensity;
				target.treeBillboardDistance = billboardStart;
			}
		}
	}
}
