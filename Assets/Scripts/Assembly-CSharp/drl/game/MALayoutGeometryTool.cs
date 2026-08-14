using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	public class MALayoutGeometryTool : MAGuide
	{
		[Header("Layout")]
		[SerializeField]
		private LayoutGeometryType m_layout_type;

		[SerializeField]
		private float m_asset_radius = 1f;

		[SerializeField]
		private float m_asset_margin;

		[SerializeField]
		private float m_asset_density = 1f;

		[SerializeField]
		private float m_layout_radius = 2f;

		[SerializeField]
		private float m_layout_height = 2f;

		[SerializeField]
		private float m_layout_aperture;

		[SerializeField]
		private Vector3 m_layout_grid_size = Vector3.one * 2f;

		[SerializeField]
		private bool m_preview_visible = true;

		private bool m_layout_params_init;

		[SerializeField]
		private LayoutParams m_layout_params;

		public Vector3[] layoutPositions;

		[Header("Templates")]
		public Transform defaultTemplate;

		public List<Component> templates;

		[Header("Assets")]
		public Transform previewContainer;

		public List<Component> previews;

		public int layoutCount;

		public LayoutGeometryType layoutType
		{
			get
			{
				return m_layout_type;
			}
			set
			{
				m_layout_type = value;
				Write();
				DelayRefresh();
			}
		}

		public float assetRadius
		{
			get
			{
				return m_asset_radius;
			}
			set
			{
				m_asset_radius = value;
				Write();
				DelayRefresh();
			}
		}

		public float assetMargin
		{
			get
			{
				return m_asset_margin;
			}
			set
			{
				m_asset_margin = value;
				Write();
				DelayRefresh();
			}
		}

		public float assetDensity
		{
			get
			{
				return m_asset_density;
			}
			set
			{
				m_asset_density = value;
				Write();
				DelayRefresh();
			}
		}

		public float layoutRadius
		{
			get
			{
				return m_layout_radius;
			}
			set
			{
				m_layout_radius = value;
				Write();
				DelayRefresh();
			}
		}

		public float layoutHeight
		{
			get
			{
				return m_layout_height;
			}
			set
			{
				m_layout_height = value;
				Write();
				DelayRefresh();
			}
		}

		public float layoutAperture
		{
			get
			{
				return m_layout_aperture;
			}
			set
			{
				m_layout_aperture = value;
				Write();
				DelayRefresh();
			}
		}

		public Vector3 layoutGridSize
		{
			get
			{
				return m_layout_grid_size;
			}
			set
			{
				m_layout_grid_size = value;
				Write();
				DelayRefresh();
			}
		}

		public bool previewVisible
		{
			get
			{
				return m_preview_visible;
			}
			set
			{
				m_preview_visible = value;
				Write();
				DelayRefresh();
			}
		}

		public LayoutParams layoutParams
		{
			get
			{
				if (!m_layout_params_init)
				{
					m_layout_params_init = true;
					m_layout_params = new LayoutParams
					{
						seed = Random.Range(1000, 2000),
						random = Vector3.zero,
						fill = false,
						max = MDLayoutGeometryTool.MaxAssets
					};
					m_layout_params.slices.Set(MDLayoutGeometryTool.m_params_slices_default);
				}
				m_layout_params.span = assetRadius / assetDensity + assetMargin;
				return m_layout_params;
			}
			set
			{
				m_layout_params = value;
				Write();
				DelayRefresh();
			}
		}

		public bool isDefaultTemplate => templates.Count <= 0;

		public new MDLayoutGeometryTool data
		{
			get
			{
				return base.data as MDLayoutGeometryTool;
			}
			set
			{
				base.data = value;
			}
		}

		protected void Start()
		{
		}

		public override void Write()
		{
			base.Write();
			MDLayoutGeometryTool mDLayoutGeometryTool = data;
			if (mDLayoutGeometryTool != null)
			{
				mDLayoutGeometryTool.layoutType = m_layout_type;
				mDLayoutGeometryTool.assetRadius = m_asset_radius;
				mDLayoutGeometryTool.assetMargin = m_asset_margin;
				mDLayoutGeometryTool.assetDensity = m_asset_density;
				mDLayoutGeometryTool.layoutRadius = m_layout_radius;
				mDLayoutGeometryTool.layoutHeight = m_layout_height;
				mDLayoutGeometryTool.layoutAperture = m_layout_aperture;
				mDLayoutGeometryTool.layoutGridSize = m_layout_grid_size;
				mDLayoutGeometryTool.layoutParams = m_layout_params;
				mDLayoutGeometryTool.previewVisible = m_preview_visible;
			}
		}

		public override void Read()
		{
			MDLayoutGeometryTool mDLayoutGeometryTool = data;
			if (mDLayoutGeometryTool != null)
			{
				m_layout_type = mDLayoutGeometryTool.layoutType;
				m_layout_type = mDLayoutGeometryTool.layoutType;
				m_asset_radius = mDLayoutGeometryTool.assetRadius;
				m_asset_margin = mDLayoutGeometryTool.assetMargin;
				m_asset_density = mDLayoutGeometryTool.assetDensity;
				m_layout_radius = mDLayoutGeometryTool.layoutRadius;
				m_layout_height = mDLayoutGeometryTool.layoutHeight;
				m_layout_aperture = mDLayoutGeometryTool.layoutAperture;
				m_layout_grid_size = mDLayoutGeometryTool.layoutGridSize;
				m_layout_params = mDLayoutGeometryTool.layoutParams;
				m_preview_visible = mDLayoutGeometryTool.previewVisible;
			}
			base.Read();
		}

		protected override MDObject NewData()
		{
			return new MDLayoutGeometryTool();
		}

		[ContextMenu("Refresh")]
		protected override void OnRefresh()
		{
			int num = 0;
			switch (layoutType)
			{
			case LayoutGeometryType.Sphere:
				num = LayoutGeometry.SphereDistribute(layoutRadius, layoutParams, null);
				break;
			case LayoutGeometryType.Cone:
				num = LayoutGeometry.ConeDistribute(layoutRadius, layoutHeight, layoutAperture, layoutParams, null);
				break;
			case LayoutGeometryType.Cylinder:
				num = LayoutGeometry.CylinderDistribute(layoutRadius, layoutHeight, layoutParams, null);
				break;
			case LayoutGeometryType.Grid:
				num = LayoutGeometry.GridDistribute(layoutGridSize.x, layoutGridSize.y, layoutGridSize.z, layoutParams, null);
				break;
			}
			if (layoutPositions == null || layoutPositions.Length != num)
			{
				layoutPositions = new Vector3[num];
			}
			switch (layoutType)
			{
			case LayoutGeometryType.Sphere:
				num = LayoutGeometry.SphereDistribute(layoutRadius, layoutParams, layoutPositions);
				break;
			case LayoutGeometryType.Cone:
				num = LayoutGeometry.ConeDistribute(layoutRadius, layoutHeight, layoutAperture, layoutParams, layoutPositions);
				break;
			case LayoutGeometryType.Cylinder:
				num = LayoutGeometry.CylinderDistribute(layoutRadius, layoutHeight, layoutParams, layoutPositions);
				break;
			case LayoutGeometryType.Grid:
				num = LayoutGeometry.GridDistribute(layoutGridSize.x, layoutGridSize.y, layoutGridSize.z, layoutParams, layoutPositions);
				break;
			}
			Clear();
			List<Component> list = new List<Component>();
			if (templates.Count <= 0)
			{
				list.Add(defaultTemplate);
			}
			else
			{
				list.AddRange(templates);
			}
			Vector3[] array = layoutPositions;
			int num2 = Mathf.Min(MDLayoutGeometryTool.MaxAssets * 2, array.Length);
			if (list.Count > 0)
			{
				while (previews.Count < num2)
				{
					Component component = Object.Instantiate(list[previews.Count % list.Count]);
					component.transform.SetParent(previewContainer, worldPositionStays: true);
					if (component is MAEntity)
					{
						MAEntity mAEntity = component as MAEntity;
						if (mAEntity.tags.Contains(MapAssetType.NoSave))
						{
							mAEntity.tags.Add(MapAssetType.NoSave);
						}
					}
					component.gameObject.SetActive(value: false);
					previews.Add(component);
				}
				layoutCount = 0;
				for (int i = 0; i < num2; i++)
				{
					bool flag = false;
					if (i > 0)
					{
						for (int num3 = i - 1; num3 >= 0; num3--)
						{
							if (Vector3.Distance(array[i], array[num3]) <= 0.05f)
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						Component component2 = previews[i];
						Component component3 = list[i % list.Count];
						component2.name = component3.name + "-" + i.ToString("000");
						Transform transform = component2.transform;
						transform.localPosition = array[i];
						if (component2 is MACollectable)
						{
							(component2 as MACollectable).sizeScale = Vector3.one * assetRadius;
						}
						else
						{
							transform.localScale = Vector3.one * assetRadius;
						}
						layoutCount++;
					}
				}
			}
			for (int j = 0; j < layoutCount; j++)
			{
				previews[j].gameObject.SetActive(m_preview_visible);
			}
		}

		[ContextMenu("Clear")]
		protected void Clear(bool p_destroy = false)
		{
			for (int i = 0; i < previews.Count; i++)
			{
				Component component = previews[i];
				if (!component)
				{
					previews.RemoveAt(i--);
				}
				else if (p_destroy)
				{
					if (Application.isPlaying)
					{
						Object.Destroy(component.gameObject);
					}
					else
					{
						Object.DestroyImmediate(component.gameObject);
					}
				}
				else
				{
					component.gameObject.SetActive(value: false);
				}
			}
			if (p_destroy)
			{
				previews.Clear();
			}
		}

		public void SetTemplates(IList p_list)
		{
			templates.Clear();
			if (p_list != null)
			{
				for (int i = 0; i < p_list.Count; i++)
				{
					if (p_list[i] is MAEntity)
					{
						templates.Add(p_list[i] as MAEntity);
					}
				}
			}
			Clear(p_destroy: true);
			DelayRefresh();
		}

		protected override void Awake()
		{
			base.Awake();
		}

		public void SetIngame(bool p_flag)
		{
			base.gameObject.SetActive(value: false);
		}

		public override void OnEditorSelect()
		{
		}

		public override void OnEditorUnselect()
		{
			SetTemplates(null);
		}
	}
}
