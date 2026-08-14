using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DroneSkin : DronePart
	{
		public new DroneAssetTagType category = DroneAssetTagType.Frame;

		public Texture2D albedo;

		public Texture2D mask;

		public Material material;

		[SerializeField]
		private List<Color> m_palette;

		private static List<Color> m_default_palette = new List<Color>(new Color[11]
		{
			Colorf.RGBToColor(16711680u),
			Colorf.RGBToColor(16750080u),
			Colorf.RGBToColor(16187141u),
			Colorf.RGBToColor(2817792u),
			Colorf.RGBToColor(1097984u),
			Colorf.RGBToColor(8978399u),
			Colorf.RGBToColor(16777215u),
			Colorf.RGBToColor(28142u),
			Colorf.RGBToColor(393456u),
			Colorf.RGBToColor(9306351u),
			Colorf.RGBToColor(16515327u)
		});

		public List<Color> palette
		{
			get
			{
				if (m_palette == null)
				{
					m_palette = new List<Color>();
				}
				if (m_palette.Count <= 0)
				{
					return m_default_palette;
				}
				return m_palette;
			}
		}

		public override string GetPrefix()
		{
			return "SK";
		}
	}
}
