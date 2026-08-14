using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	[ExecuteInEditMode]
	public class MELayoutGenerator : MonoBehaviour
	{
		[SerializeField]
		private LineRenderer m_renderer;

		public LayoutGeometryType type;

		[Header("Total Assets")]
		public int asset_count;

		[Header("General")]
		public LayoutParams layout;

		[Range(0.4f, 2.4f)]
		public float asset_radius = 1f;

		[Range(0f, 0.5f)]
		public float asset_margin;

		[Range(0.05f, 1f)]
		public float asset_density = 1f;

		[Header("Sphere")]
		[Range(0.1f, 36f)]
		public float sphere_radius = 2f;

		[Header("Cone")]
		[Range(0.1f, 36f)]
		public float cone_radius = 1f;

		[Range(0f, 36f)]
		public float cone_height = 1f;

		[Range(0f, 1f)]
		public float cone_aperture;

		[Header("Cylinder")]
		[Range(0.1f, 36f)]
		public float cyl_radius = 1f;

		[Range(0f, 36f)]
		public float cyl_height = 1f;

		[Header("Grid")]
		[Range(0.1f, 36f)]
		public float grid_x = 1f;

		[Range(0.1f, 36f)]
		public float grid_y = 1f;

		[Range(0.1f, 36f)]
		public float grid_z = 1f;

		public GameObject template;

		public List<GameObject> assets;

		internal Vector3[] buffer = new Vector3[0];

		public LineRenderer renderer
		{
			get
			{
				if (!m_renderer)
				{
					return m_renderer = GetComponent<LineRenderer>();
				}
				return m_renderer;
			}
		}

		protected void Start()
		{
		}

		[ContextMenu("Clear")]
		public void Clear()
		{
			for (int i = 0; i < assets.Count; i++)
			{
				if ((bool)assets[i])
				{
					Object.Destroy(assets[i].gameObject);
				}
			}
			assets.Clear();
		}

		[ContextMenu("Refresh")]
		public void Refresh()
		{
			if (assets == null)
			{
				assets = new List<GameObject>();
			}
			layout.span = asset_radius / asset_density + asset_margin;
			int num = buffer.Length;
			switch (type)
			{
			case LayoutGeometryType.Sphere:
				num = LayoutGeometry.SphereDistribute(sphere_radius, layout, null);
				break;
			case LayoutGeometryType.Cone:
				num = LayoutGeometry.ConeDistribute(cone_radius, cone_height, cone_aperture, layout, null);
				break;
			case LayoutGeometryType.Cylinder:
				num = LayoutGeometry.CylinderDistribute(cyl_radius, cyl_height, layout, null);
				break;
			case LayoutGeometryType.Grid:
				num = LayoutGeometry.GridDistribute(grid_x, grid_y, grid_z, layout, null);
				break;
			}
			if (buffer.Length != num)
			{
				buffer = new Vector3[num];
			}
			switch (type)
			{
			case LayoutGeometryType.Sphere:
				num = LayoutGeometry.SphereDistribute(sphere_radius, layout, buffer);
				break;
			case LayoutGeometryType.Cone:
				num = LayoutGeometry.ConeDistribute(cone_radius, cone_height, cone_aperture, layout, buffer);
				break;
			case LayoutGeometryType.Cylinder:
				num = LayoutGeometry.CylinderDistribute(cyl_radius, cyl_height, layout, buffer);
				break;
			case LayoutGeometryType.Grid:
				num = LayoutGeometry.GridDistribute(grid_x, grid_y, grid_z, layout, buffer);
				break;
			}
			asset_count = num;
			while (assets.Count < buffer.Length)
			{
				GameObject gameObject = (template ? Object.Instantiate(template) : GameObject.CreatePrimitive(PrimitiveType.Sphere));
				gameObject.transform.parent = base.transform;
				gameObject.gameObject.SetActive(value: false);
				assets.Add(gameObject);
			}
			for (int i = 0; i < assets.Count; i++)
			{
				if (!assets[i])
				{
					assets.RemoveAt(i--);
				}
			}
			for (int j = 0; j < assets.Count; j++)
			{
				if ((bool)assets[j])
				{
					if (!assets[j].gameObject.activeInHierarchy)
					{
						break;
					}
					assets[j].gameObject.SetActive(value: false);
				}
			}
			for (int k = 0; k < buffer.Length; k++)
			{
				GameObject obj = assets[k];
				obj.name = k.ToString() ?? "";
				obj.transform.localPosition = buffer[k];
				obj.transform.localScale = Vector3.one * asset_radius;
				obj.gameObject.SetActive(value: true);
			}
		}
	}
}
