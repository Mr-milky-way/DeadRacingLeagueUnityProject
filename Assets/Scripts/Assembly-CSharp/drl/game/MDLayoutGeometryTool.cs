using UnityEngine;

namespace drl.game
{
	public class MDLayoutGeometryTool : MDGuide
	{
		public static int MaxAssets = 250;

		private float[] m_params_slices = new float[6] { 0f, 0f, 0f, 1f, 1f, 1f };

		internal static float[] m_params_slices_default = new float[6] { 0f, 0f, 0f, 1f, 1f, 1f };

		public LayoutGeometryType layoutType
		{
			get
			{
				return (LayoutGeometryType)Get("lgt-layout-type", 0);
			}
			set
			{
				Set("lgt-layout-type", value);
			}
		}

		public float assetRadius
		{
			get
			{
				return Get("lgt-asset-radius", 1f);
			}
			set
			{
				Set("lgt-asset-radius", value);
			}
		}

		public float assetMargin
		{
			get
			{
				return Get("lgt-asset-margin", 0f);
			}
			set
			{
				Set("lgt-asset-margin", value);
			}
		}

		public float assetDensity
		{
			get
			{
				return Get("lgt-asset-density", 1f);
			}
			set
			{
				Set("lgt-asset-density", value);
			}
		}

		public float layoutRadius
		{
			get
			{
				return Get("lgt-layout-radius", 2f);
			}
			set
			{
				Set("lgt-layout-radius", value);
			}
		}

		public float layoutHeight
		{
			get
			{
				return Get("lgt-layout-height", 2f);
			}
			set
			{
				Set("lgt-layout-height", value);
			}
		}

		public float layoutAperture
		{
			get
			{
				return Get("lgt-layout-aperture", 0f);
			}
			set
			{
				Set("lgt-layout-aperture", value);
			}
		}

		public Vector3 layoutGridSize
		{
			get
			{
				return GetVector3("lgt-layout-grid-size", Vector3.one * 2f);
			}
			set
			{
				SetVector3("lgt-layout-grid-size", value);
			}
		}

		public bool previewVisible
		{
			get
			{
				return Get("lgt-preview-visible", d: true);
			}
			set
			{
				Set("lgt-preview-visible", value);
			}
		}

		public LayoutParams layoutParams
		{
			get
			{
				LayoutParams result = default(LayoutParams);
				result.seed = Random.Range(1000, 2000);
				result.span = assetRadius / assetDensity + assetMargin;
				result.slices.Set(GetCast("lgt-params-slice", m_params_slices_default));
				result.random = GetVector3("lgt-params-random", Vector3.zero);
				result.fill = Get("lgt-params-fill", d: false);
				result.max = MaxAssets;
				return result;
			}
			set
			{
				int num = 0;
				LayoutParams layoutParams = value;
				m_params_slices[num++] = layoutParams.slices.x;
				m_params_slices[num++] = layoutParams.slices.y;
				m_params_slices[num++] = layoutParams.slices.z;
				m_params_slices[num++] = layoutParams.slices.rangeX;
				m_params_slices[num++] = layoutParams.slices.rangeY;
				m_params_slices[num++] = layoutParams.slices.rangeZ;
				Set("lgt-params-slice", m_params_slices);
				SetVector3("lgt-params-random", layoutParams.random);
				Set("lgt-params-fill", layoutParams.fill);
			}
		}

		public MDLayoutGeometryTool()
		{
			base.type = MapAssetType.Guide;
		}
	}
}
