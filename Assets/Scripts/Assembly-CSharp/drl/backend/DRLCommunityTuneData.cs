using UnityEngine;
using drl.sim;
using thelab.core;

namespace drl.backend
{
	public class DRLCommunityTuneData : SerializedData
	{
		private SerializedData m_extendedData;

		public string playerId
		{
			get
			{
				if (extendedData.ContainsKey("player-id"))
				{
					return extendedData.Get("player-id", "");
				}
				return Get("player-id", "");
			}
			set
			{
				Set("player-id", value);
				extendedData.Set("player-id", value);
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

		public string name
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

		public string rig
		{
			get
			{
				return Get("rig", "");
			}
			set
			{
				Set("rig", value.StartsWith("*") ? value : ("*" + value));
			}
		}

		public DroneRigData rigData
		{
			get
			{
				if (string.IsNullOrEmpty(rig))
				{
					return null;
				}
				if (rig.StartsWith("*"))
				{
					return DroneRigData.FromJson(rig);
				}
				return DroneRigData.NewFromLegacy(SerializedData.FromJson<DroneRigLegacyData>(rig));
			}
		}

		public string data
		{
			get
			{
				return Get("data", "");
			}
			set
			{
				Set("data", value);
			}
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

		protected SerializedData extendedData
		{
			get
			{
				if (m_extendedData == null)
				{
					m_extendedData = SerializedData.FromJson<SerializedData>(data);
				}
				return m_extendedData;
			}
		}

		public float flightTimeMine
		{
			get
			{
				return extendedData.Get("flight-time", 0f);
			}
			set
			{
				extendedData.Set("flight-time", value);
			}
		}

		public float flightTimeTotal
		{
			get
			{
				return extendedData.Get("flight-total", 0f);
			}
			set
			{
				extendedData.Set("flight-total", value);
			}
		}

		public int thrust
		{
			get
			{
				return extendedData.Get("thrust", 0);
			}
			set
			{
				extendedData.Set("thrust", value);
			}
		}

		public int weight
		{
			get
			{
				return extendedData.Get("weight", 0);
			}
			set
			{
				extendedData.Set("weight", value);
			}
		}

		public int size
		{
			get
			{
				return extendedData.Get("size", 0);
			}
			set
			{
				extendedData.Set("size", value);
			}
		}

		public static string GenerateGUID()
		{
			return "DPT-" + GUID.Create(16, "", 200, 0, 15, "x1");
		}

		public DRLCommunityTuneData()
		{
			if (string.IsNullOrEmpty(guid))
			{
				guid = GenerateGUID();
			}
		}

		public T GetData<T>()
		{
			string text = data;
			if (string.IsNullOrEmpty(text))
			{
				return default(T);
			}
			return Serialize.FromJson<T>(text);
		}

		public void SetData(SerializedData p_data = null)
		{
			data = extendedData.ToJson();
		}
	}
}
