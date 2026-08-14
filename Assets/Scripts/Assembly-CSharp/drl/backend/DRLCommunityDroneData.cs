using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class DRLCommunityDroneData : SerializedData
	{
		public enum ThumbSize
		{
			Small = 0,
			Medium = 1,
			Large = 2
		}

		public enum SortType
		{
			None = 0,
			NameAsc = 1,
			NameDesc = 2,
			ScoreAsc = 3,
			ScoreDesc = 4,
			RatingCountAsc = 5,
			RatingCountDesc = 6,
			ThrustAsc = 7,
			ThrustDesc = 8,
			SpeedAsc = 9,
			SpeedDesc = 10,
			WeightAsc = 11,
			WeightDesc = 12,
			RPMAsc = 13,
			RPMDesc = 14,
			FlightTimeAsc = 15,
			FlightTimeDesc = 16,
			FlightTotalAsc = 17,
			FlightTotalDesc = 18
		}

		public string guid
		{
			get
			{
				return Get("guid", "");
			}
			set
			{
				Set("guid", value);
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

		public string platformPlayerId => Get("profile-platform-id", "");

		public string platform => Get("profile-platform", "");

		public string profileColorHex => Get("profile-color", "000000");

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

		public float score
		{
			get
			{
				return Get("score", 0f);
			}
			set
			{
				Set("score", value);
			}
		}

		public float rating
		{
			get
			{
				return Get("rating", 0f);
			}
			set
			{
				Set("rating", value);
			}
		}

		public int ratingCount
		{
			get
			{
				return Get("rating-count", 0);
			}
			set
			{
				Set("rating-count", value);
			}
		}

		public string droneThumbURL
		{
			get
			{
				return Get("thumb-url", "");
			}
			set
			{
				Set("thumb-url", value);
			}
		}

		public string droneName
		{
			get
			{
				return Get("name", "");
			}
			set
			{
				Set("name", value);
			}
		}

		public bool isPublic
		{
			get
			{
				return Get("is-public", d: false);
			}
			set
			{
				Set("is-public", value);
			}
		}

		public bool isDroneOfficial
		{
			get
			{
				return Get("is-official", d: false);
			}
			set
			{
				Set("is-official", value);
			}
		}

		public bool isCustomPhysics
		{
			get
			{
				return Get("is-custom-physics", d: false);
			}
			set
			{
				Set("is-custom-physics", value);
			}
		}

		public float droneFlightTime
		{
			get
			{
				return Get("flight-time", 0f);
			}
			set
			{
				Set("flight-time", value);
			}
		}

		public float droneFlightTotal
		{
			get
			{
				return Get("flight-total", 0f);
			}
			set
			{
				Set("flight-total", value);
			}
		}

		public int droneSize
		{
			get
			{
				return Get("size", 6);
			}
			set
			{
				Set("size", value);
			}
		}

		public float droneThrust
		{
			get
			{
				return Get("thrust", 0f);
			}
			set
			{
				Set("thrust", value);
			}
		}

		public float droneSpeed
		{
			get
			{
				return Get("speed", 0f);
			}
			set
			{
				Set("speed", value);
			}
		}

		public float droneWeight
		{
			get
			{
				return Get("weight", 0f);
			}
			set
			{
				Set("weight", value);
			}
		}

		public float droneRPM
		{
			get
			{
				return Get("rpm", 0f);
			}
			set
			{
				Set("rpm", value);
			}
		}

		public string droneFrameId
		{
			get
			{
				return Get("frame-id", "");
			}
			set
			{
				Set("frame-id", value);
			}
		}

		public string droneMotorId
		{
			get
			{
				return Get("motor-id", "");
			}
			set
			{
				Set("motor-id", value);
			}
		}

		public string dronePropId
		{
			get
			{
				return Get("prop-id", "");
			}
			set
			{
				Set("prop-id", value);
			}
		}

		public string droneBatteryId
		{
			get
			{
				return Get("battery-id", "");
			}
			set
			{
				Set("battery-id", value);
			}
		}

		public string droneRigData
		{
			get
			{
				JObject jObject = Get<JObject>("rig-data", null);
				if (jObject == null)
				{
					string text = Get<string>("rig-data", null);
					if (text != null)
					{
						return text;
					}
				}
				if (jObject == null)
				{
					return new JObject().ToString();
				}
				return jObject.ToString();
			}
			set
			{
				Set("rig-data", value);
			}
		}

		public string droneProfileData
		{
			get
			{
				return Get("profile-data", "");
			}
			set
			{
				Set("profile-data", value);
			}
		}

		public string dronePhysicsData
		{
			get
			{
				return Get("physics-data", "");
			}
			set
			{
				Set("physics-data", value);
			}
		}

		public int page
		{
			get
			{
				return Get("page", 0);
			}
			set
			{
				Set("page", value);
			}
		}

		public int limit
		{
			get
			{
				return Get("limit", 0);
			}
			set
			{
				Set("limit", value);
			}
		}

		public SortType sort
		{
			set
			{
				string text = "";
				string text2 = "";
				switch (value)
				{
				case SortType.NameAsc:
					text = "name";
					text2 = "asc";
					break;
				case SortType.NameDesc:
					text = "name";
					text2 = "desc";
					break;
				case SortType.ScoreAsc:
					text = "score";
					text2 = "asc";
					break;
				case SortType.ScoreDesc:
					text = "score";
					text2 = "desc";
					break;
				case SortType.RatingCountAsc:
					text = "rating-count";
					text2 = "asc";
					break;
				case SortType.RatingCountDesc:
					text = "rating-count";
					text2 = "desc";
					break;
				case SortType.ThrustAsc:
					text = "thrust";
					text2 = "asc";
					break;
				case SortType.ThrustDesc:
					text = "thrust";
					text2 = "desc";
					break;
				case SortType.SpeedAsc:
					text = "speed";
					text2 = "asc";
					break;
				case SortType.SpeedDesc:
					text = "speed";
					text2 = "desc";
					break;
				case SortType.WeightAsc:
					text = "weight";
					text2 = "asc";
					break;
				case SortType.WeightDesc:
					text = "weight";
					text2 = "desc";
					break;
				case SortType.RPMAsc:
					text = "rpm";
					text2 = "asc";
					break;
				case SortType.RPMDesc:
					text = "rpm";
					text2 = "desc";
					break;
				case SortType.FlightTimeAsc:
					text = "flight-time";
					text2 = "asc";
					break;
				case SortType.FlightTimeDesc:
					text = "flight-time";
					text2 = "desc";
					break;
				case SortType.FlightTotalAsc:
					text = "flight-total";
					text2 = "asc";
					break;
				case SortType.FlightTotalDesc:
					text = "flight-total";
					text2 = "desc";
					break;
				}
				if (!string.IsNullOrEmpty(text))
				{
					Set("sort", text);
				}
				if (!string.IsNullOrEmpty(text2))
				{
					Set("order", text2);
				}
			}
		}

		public T Convert<T>()
		{
			string text = ToJson();
			if (string.IsNullOrEmpty(text))
			{
				return default(T);
			}
			return Serialize.FromJson<T>(text);
		}

		public string GetThumbURL(ThumbSize p_size)
		{
			Dictionary<string, string> dictionary = Get<JObject>("images", null)?.ToObject<Dictionary<string, string>>();
			if (dictionary == null)
			{
				return droneThumbURL;
			}
			switch (p_size)
			{
			case ThumbSize.Small:
				if (dictionary.ContainsKey("small"))
				{
					return dictionary["small"];
				}
				break;
			case ThumbSize.Medium:
				if (dictionary.ContainsKey("medium"))
				{
					return dictionary["medium"];
				}
				break;
			case ThumbSize.Large:
				if (dictionary.ContainsKey("large"))
				{
					return dictionary["large"];
				}
				break;
			}
			return droneThumbURL;
		}
	}
}
