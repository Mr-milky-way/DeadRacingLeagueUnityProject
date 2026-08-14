using UnityEngine;

namespace drl.game
{
	public class MDRenderer : MDEntity
	{
		public Color emissionColor
		{
			get
			{
				return GetColor("color-emission", Color.black);
			}
			set
			{
				SetColor("color-emission", value);
			}
		}

		public float colorIntensity
		{
			get
			{
				return Get("color-intensity", 1.5f);
			}
			set
			{
				Set("color-intensity", value);
			}
		}

		public Color color0
		{
			get
			{
				return GetColor("color-0", Color.white);
			}
			set
			{
				SetColor("color-0", value);
			}
		}

		public Color color1
		{
			get
			{
				return GetColor("color-1", Color.white);
			}
			set
			{
				SetColor("color-1", value);
			}
		}

		public Color color2
		{
			get
			{
				return GetColor("color-2", Color.white);
			}
			set
			{
				SetColor("color-2", value);
			}
		}

		public int mapStyle0
		{
			get
			{
				return Get("map-style-0", -1);
			}
			set
			{
				Set("map-style-0", value);
			}
		}

		public int mapStyle1
		{
			get
			{
				return Get("map-style-1", -1);
			}
			set
			{
				Set("map-style-1", value);
			}
		}

		public int mapStyle2
		{
			get
			{
				return Get("map-style-2", -1);
			}
			set
			{
				Set("map-style-2", value);
			}
		}

		public int style0
		{
			get
			{
				return Get("style-0", 0);
			}
			set
			{
				Set("style-0", value);
			}
		}

		public int style1
		{
			get
			{
				return Get("style-1", 0);
			}
			set
			{
				Set("style-1", value);
			}
		}

		public int style2
		{
			get
			{
				return Get("style-2", 0);
			}
			set
			{
				Set("style-2", value);
			}
		}

		public bool visible
		{
			get
			{
				return Get("visible", d: true);
			}
			set
			{
				Set("visible", value);
			}
		}

		public bool isLayout
		{
			get
			{
				return Get("is-layout", d: false);
			}
			set
			{
				Set("is-layout", value);
			}
		}

		public MDRenderer()
		{
			base.type = MapAssetType.Renderer;
		}
	}
}
