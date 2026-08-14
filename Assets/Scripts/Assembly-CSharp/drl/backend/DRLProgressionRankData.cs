using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class DRLProgressionRankData : SerializedData
	{
		public bool isPlayer
		{
			get
			{
				return Get("is-player", d: false);
			}
			set
			{
				Set("is-player", value);
			}
		}

		public bool isTop
		{
			get
			{
				return Get("is-top", d: false);
			}
			set
			{
				Set("is-top", value);
			}
		}

		public bool isBottom
		{
			get
			{
				return Get("is-bottom", d: false);
			}
			set
			{
				Set("is-bottom", value);
			}
		}

		public string profileColorHex
		{
			get
			{
				return Get("profile-color", "000000");
			}
			set
			{
				Set("profile-color", value);
			}
		}

		public Color profileColor
		{
			get
			{
				if (!ContainsKey("profile-color"))
				{
					return Color.magenta;
				}
				return Colorf.ParseRGB(profileColorHex, Color.yellow);
			}
		}

		public string profileThumbURL
		{
			get
			{
				return Get("profile-thumb", "");
			}
			set
			{
				Set("profile-thumb", value);
			}
		}

		public string profileName
		{
			get
			{
				return Get("profile-name", "");
			}
			set
			{
				Set("profile-name", value);
			}
		}

		public string flagThumbURL
		{
			get
			{
				return Get("flag-url", "");
			}
			set
			{
				Set("profile-thumb", value);
			}
		}

		public int position
		{
			get
			{
				return Get("position", -1);
			}
			set
			{
				Set("position", value);
			}
		}

		public string type
		{
			get
			{
				return Get("type", "player");
			}
			set
			{
				Set("type", value);
			}
		}

		public int weekXP
		{
			get
			{
				return Get("xp", 0);
			}
			set
			{
				Set("xp", value);
			}
		}
	}
}
