using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class DRLTournamentPlayerData : SerializedData
	{
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

		public string platform
		{
			get
			{
				return Get("platform", "");
			}
			set
			{
				Set("platform", value);
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

		public string profileColor2Hex
		{
			get
			{
				return Get("profile-secondary-color", "000000");
			}
			set
			{
				Set("profile-secondary-color", value);
			}
		}

		public Color profileColor2
		{
			get
			{
				if (!ContainsKey("profile-secondary-color"))
				{
					return profileColor;
				}
				return Colorf.ParseRGB(profileColor2Hex, Color.yellow);
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

		public string flagThumbURL => Get("flag-url", "");

		public int parent => Get("parent", 0);

		public int points => Get("points", 0);

		public int score => Get("score", -1);

		public int totalTime => Get("score_total", 0);

		public int position => Get("position", 0);

		public int totalWins => Get("total_wins", 0);

		public bool isWinner => Get("is-winner", d: false);

		public bool isWinnerSecond => Get("is-winner-second", d: false);

		internal void WarmUp()
		{
		}
	}
}
