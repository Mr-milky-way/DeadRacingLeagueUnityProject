using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class DRLCommunityMapData : SerializedData
	{
		public enum ThumbSize
		{
			Small = 0,
			Medium = 1,
			Large = 2
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

		public DRLPlayerProfileData[] collaborators
		{
			get
			{
				object obj = Get("collaborators", (object)new DRLPlayerProfileData[0]);
				if (obj is JArray)
				{
					obj = (obj as JArray).ToObject<DRLPlayerProfileData[]>();
				}
				return (DRLPlayerProfileData[])obj;
			}
			set
			{
				Set("collaborators", value);
			}
		}

		public string[] platformExclusive
		{
			get
			{
				object obj = Get("exclusive-by-platform", (object)new string[0]);
				if (obj is JArray)
				{
					obj = (obj as JArray).ToObject<string[]>();
				}
				return (string[])obj;
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

		public string mapId
		{
			get
			{
				return Get("map-id", "");
			}
			set
			{
				Set("map-id", value);
			}
		}

		public string trackId
		{
			get
			{
				return Get("track-id", "freestyle");
			}
			set
			{
				Set("track-id", value);
			}
		}

		public string podiumId
		{
			get
			{
				return Get("podium-id", "PD-a6d");
			}
			set
			{
				Set("podium-id", value);
			}
		}

		public string mapThumbURL
		{
			get
			{
				return Get("map-thumb", "");
			}
			set
			{
				Set("map-thumb", value);
			}
		}

		public string mapTitle
		{
			get
			{
				return Get("map-title", "");
			}
			set
			{
				Set("map-title", value);
			}
		}

		public string mapCategory
		{
			get
			{
				return Get("map-category", "");
			}
			set
			{
				Set("map-category", value);
			}
		}

		public GameFlag mapCategoryFlag
		{
			get
			{
				string value = mapCategory;
				if (string.IsNullOrEmpty(value))
				{
					return GameFlag.MapCommon;
				}
				return (GameFlag)Enum.Parse(typeof(GameFlag), value);
			}
			set
			{
				string text = ((value == GameFlag.None) ? "" : value.ToString());
				mapCategory = text;
			}
		}

		public string type => Get("map-mode-type", "");

		public GameFlag typeFlag
		{
			get
			{
				string value = type;
				if (string.IsNullOrEmpty(value))
				{
					return GameFlag.Race;
				}
				return (GameFlag)Enum.Parse(typeof(GameFlag), value);
			}
		}

		public int mapDifficulty
		{
			get
			{
				return Get("map-difficulty", 0);
			}
			set
			{
				Set("map-difficulty", value);
			}
		}

		public int mapLaps
		{
			get
			{
				return Get("map-laps", 1);
			}
			set
			{
				Set("map-laps", value);
			}
		}

		public int mapLighting
		{
			get
			{
				return Get("map-lighting", 0);
			}
			set
			{
				Set("map-lighting", value);
			}
		}

		public float mapDistance
		{
			get
			{
				return Get("map-distance", 0f);
			}
			set
			{
				Set("map-distance", value);
			}
		}

		public int mapTriangleCount
		{
			get
			{
				return Get("map-stats-triangle-count", 0);
			}
			set
			{
				Set("map-stats-triangle-count", value);
			}
		}

		public int mapObjectCount
		{
			get
			{
				return Get("map-stats-object-count", 0);
			}
			set
			{
				Set("map-stats-object-count", value);
			}
		}

		public bool isRaceAllowed
		{
			get
			{
				return Get("is-race-allowed", d: true);
			}
			set
			{
				Set("is-race-allowed", value);
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

		public bool allowCopy
		{
			get
			{
				return Get("allow-copy", d: false);
			}
			set
			{
				Set("allow-copy", value);
			}
		}

		public bool isFeatured
		{
			get
			{
				return Get("is-featured", d: false);
			}
			set
			{
				Set("is-featured", value);
			}
		}

		public bool writeEnabled
		{
			get
			{
				return Get("write-enabled", d: true);
			}
			set
			{
				Set("write-enabled", value);
			}
		}

		public bool baseAssetsEnabled
		{
			get
			{
				return Get("base-assets-enabled", d: false);
			}
			set
			{
				Set("base-assets-enabled", value);
			}
		}

		public int version
		{
			get
			{
				return Get("version", -1);
			}
			set
			{
				Set("version", value);
			}
		}

		public string root
		{
			get
			{
				object obj = Get<object>("root", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
			set
			{
				Set("root", value);
			}
		}

		public bool hasRoot
		{
			get
			{
				return Get("has-root", d: false);
			}
			set
			{
				Set("has-root", value);
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
				case SortType.DateAsc:
					text = "created-at";
					text2 = "asc";
					break;
				case SortType.DateDesc:
					text = "created-at";
					text2 = "desc";
					break;
				case SortType.DateUpdateAsc:
					text = "updated-at";
					text2 = "asc";
					break;
				case SortType.DateUpdateDesc:
					text = "updated-at";
					text2 = "desc";
					break;
				case SortType.Featured:
					text = "featured";
					text2 = "";
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

		public bool IsOwner(string p_player_id)
		{
			if (p_player_id == playerId)
			{
				return true;
			}
			DRLPlayerProfileData[] array = collaborators;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].playerId == p_player_id)
				{
					return true;
				}
			}
			return false;
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

		public void Load(string p_json)
		{
			Serialize.FromJson(p_json, this, p_populate: true);
		}

		public string GetThumbURL(ThumbSize p_size)
		{
			Dictionary<string, string> dictionary = Get<JObject>("images", null)?.ToObject<Dictionary<string, string>>();
			if (dictionary == null)
			{
				return mapThumbURL;
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
			return mapThumbURL;
		}
	}
}
