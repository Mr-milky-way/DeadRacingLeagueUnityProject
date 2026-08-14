using UnityEngine;

namespace drl.level
{
	public class TerrainHole : MonoBehaviour
	{
		public Terrain terrain;

		public Material material;

		protected void Awake()
		{
			Refresh();
		}

		public void Refresh()
		{
			Material material = this.material;
			if (!this.material)
			{
				material = (terrain ? terrain.materialTemplate : null);
			}
			if (!material)
			{
				return;
			}
			int num = Mathf.Min(base.transform.childCount, 12);
			for (int i = 0; i < 12; i++)
			{
				string text = "_Hole" + i;
				if (material.HasProperty(text))
				{
					material.SetVector(text, new Vector4(0f, 0f, 0f, 1E-05f));
				}
			}
			for (int j = 0; j < num; j++)
			{
				Transform child = base.transform.GetChild(j);
				TerrainHoleElement component = child.GetComponent<TerrainHoleElement>();
				if ((bool)component)
				{
					string text2 = "_Hole" + j;
					if (material.HasProperty(text2))
					{
						Vector4 value = child.position;
						value.w = component.radius;
						material.SetVector(text2, value);
					}
				}
			}
		}
	}
}
