using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class DRLPlacementsUserData : SerializedData
	{
		public int position
		{
			get
			{
				return Get("position", 0);
			}
			set
			{
				Set("position", value);
			}
		}

		public string username
		{
			get
			{
				return Get("username", "");
			}
			set
			{
				Set("username", value);
			}
		}

		public string profileColorHex
		{
			get
			{
				return Get("color", "000000");
			}
			set
			{
				Set("color", value);
			}
		}

		public Color profileColor
		{
			get
			{
				if (!ContainsKey("color"))
				{
					return Color.magenta;
				}
				return Colorf.ParseRGB(profileColorHex, Color.yellow);
			}
		}
	}
}
