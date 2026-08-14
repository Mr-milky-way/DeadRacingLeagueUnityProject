using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class DRLTournamentHeatResultData : SerializedData
	{
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

		public int crashes
		{
			get
			{
				return Get("crashes", 0);
			}
			set
			{
				Set("crashes", value);
			}
		}

		public int score
		{
			get
			{
				return Get("score", 0);
			}
			set
			{
				Set("score", value);
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

		public Color color
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

		public bool success => Get("success", d: true);
	}
}
