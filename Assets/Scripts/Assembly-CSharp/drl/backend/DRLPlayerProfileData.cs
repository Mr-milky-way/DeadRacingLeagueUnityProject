using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class DRLPlayerProfileData : SerializedData
	{
		public string platformId
		{
			get
			{
				return Get(DRLService.PlatformIdKey, "");
			}
			set
			{
				Set(DRLService.PlatformIdKey, value);
			}
		}

		public string playerId
		{
			get
			{
				return Get("player-id", "");
			}
			set
			{
				Set("player-id", value);
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

		public string secondaryProfileColorHex
		{
			get
			{
				Debug.Log("Color: " + secondaryProfileColor.ToString());
				return Get("profile-secondary-color", "00ff00");
			}
			set
			{
				Set("profile-secondary-color", value);
			}
		}

		public Color secondaryProfileColor
		{
			get
			{
				if (!ContainsKey("secondary-profile-color"))
				{
					return profileColor;
				}
				return Colorf.ParseRGB(secondaryProfileColorHex, Color.yellow);
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

		public int profileRank
		{
			get
			{
				return Get("profile-rank", 0);
			}
			set
			{
				Set("profile-rank", value);
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

		public string name => Get("username", "");

		public bool hasGame
		{
			get
			{
				return Get("has-game", d: false);
			}
			set
			{
				Set("has-game", value);
			}
		}

		public bool isDRLPilot
		{
			get
			{
				return Get("is-drl-pilot", d: false);
			}
			set
			{
				Set("is-drl-pilot", value);
			}
		}

		public string flagThumbURL => Get("flag-url", "");
	}
}
