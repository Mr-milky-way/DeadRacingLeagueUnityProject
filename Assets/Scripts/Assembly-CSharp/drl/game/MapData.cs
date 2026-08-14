using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class MapData : SerializedData
	{
		public enum ThumbSize
		{
			Small = 0,
			Medium = 1,
			Large = 2
		}

		private MapDataMode m_mode;

		private int[] m_map_asset_layers;

		private int[] m_map_styles;

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

		public DRLPlayerProfileData[] collaborators
		{
			get
			{
				object obj = Get<object>("collaborators", null);
				if (obj is JArray)
				{
					obj = (obj as JArray).ToObject<DRLPlayerProfileData[]>();
				}
				if (obj == null)
				{
					obj = new DRLPlayerProfileData[0];
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

		public MapDataMode mode
		{
			get
			{
				if (m_mode != null)
				{
					return m_mode;
				}
				return m_mode = new MapDataMode(this);
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

		public string mapGroups
		{
			get
			{
				return Get("map-groups", "");
			}
			set
			{
				Set("map-groups", value);
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
				return Get("podium-id", "");
			}
			set
			{
				Set("podium-id", value);
			}
		}

		public int mapDifficulty
		{
			get
			{
				return Get("map-difficulty", 1);
			}
			set
			{
				Set("map-difficulty", value);
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

		public bool containsMapLighting => ContainsKey("map-lighting");

		public int[] mapAssetLayers
		{
			get
			{
				if (m_map_asset_layers != null)
				{
					return m_map_asset_layers;
				}
				object obj = null;
				try
				{
					obj = GetCast<int[]>("map-asset-layers", null);
				}
				catch (Exception)
				{
					obj = new int[0];
				}
				int[] array = null;
				array = ((!(obj is JArray)) ? ((int[])obj) : (obj as JArray).ToObject<int[]>());
				if (((array != null) ? array.Length : 0) < 8)
				{
					array = new int[8];
				}
				m_map_asset_layers = new int[array.Length];
				for (int i = 0; i < m_map_asset_layers.Length; i++)
				{
					m_map_asset_layers[i] = array[i];
				}
				return m_map_asset_layers;
			}
			set
			{
				m_map_asset_layers = ((value == null) ? new int[0] : value);
				int[] array = new int[m_map_asset_layers.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = m_map_asset_layers[i];
				}
				Set("map-asset-layers", array);
			}
		}

		public int mapAssetLayer0
		{
			get
			{
				int[] array = mapAssetLayers;
				if (array.Length < 1)
				{
					return 0;
				}
				return array[0];
			}
			set
			{
				int[] array = mapAssetLayers;
				array[0] = value;
				mapAssetLayers = array;
			}
		}

		public int mapAssetLayer1
		{
			get
			{
				int[] array = mapAssetLayers;
				if (array.Length < 2)
				{
					return 0;
				}
				return array[1];
			}
			set
			{
				int[] array = mapAssetLayers;
				array[1] = value;
				mapAssetLayers = array;
			}
		}

		public int mapAssetLayer2
		{
			get
			{
				int[] array = mapAssetLayers;
				if (array.Length < 3)
				{
					return 0;
				}
				return array[2];
			}
			set
			{
				int[] array = mapAssetLayers;
				array[2] = value;
				mapAssetLayers = array;
			}
		}

		public int mapStyle0
		{
			get
			{
				int[] array = mapStyles;
				if (array.Length < 1)
				{
					return 0;
				}
				return array[0];
			}
			set
			{
				int[] array = mapStyles;
				array[0] = value;
				mapStyles = array;
			}
		}

		public int mapStyle1
		{
			get
			{
				int[] array = mapStyles;
				if (array.Length < 2)
				{
					return 0;
				}
				return array[1];
			}
			set
			{
				int[] array = mapStyles;
				array[1] = value;
				mapStyles = array;
			}
		}

		public int mapStyle2
		{
			get
			{
				int[] array = mapStyles;
				if (array.Length < 3)
				{
					return 0;
				}
				return array[2];
			}
			set
			{
				int[] array = mapStyles;
				array[2] = value;
				mapStyles = array;
			}
		}

		public int[] mapStyles
		{
			get
			{
				if (m_map_styles != null)
				{
					return m_map_styles;
				}
				object obj = null;
				try
				{
					obj = GetCast<int[]>("map-styles", null);
				}
				catch (Exception)
				{
					obj = new int[0];
				}
				int[] array = null;
				array = ((!(obj is JArray)) ? ((int[])obj) : (obj as JArray).ToObject<int[]>());
				if (((array != null) ? array.Length : 0) < 8)
				{
					array = new int[8];
				}
				m_map_styles = new int[array.Length];
				for (int i = 0; i < m_map_styles.Length; i++)
				{
					m_map_styles[i] = array[i];
				}
				return m_map_styles;
			}
			set
			{
				m_map_styles = ((value == null) ? new int[0] : value);
				int[] array = new int[m_map_styles.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = m_map_styles[i];
				}
				Set("map-styles", array);
			}
		}

		public bool mapDirty
		{
			get
			{
				return Get("map-dirty", d: false);
			}
			set
			{
				Set("map-dirty", value);
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
				if (mapObjectCount <= 0 && value > 0)
				{
					mapDirty = true;
				}
				Set("map-stats-object-count", value);
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
				mapDirty = true;
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
				mapDirty = true;
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
				mapDirty = true;
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
				string text = value.ToString();
				mapCategory = text;
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

		public bool isFeatured => Get("is-featured", d: false);

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

		public int order => Get("norder", -1);

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

		public string fullTrackURL => Get("full-track-url", "");

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

		public MDEntity root
		{
			get
			{
				return GetCast<MDEntity>("root", null, p_add: true);
			}
			set
			{
				Set("root", value);
			}
		}

		public MapDataPrefs prefs
		{
			get
			{
				return GetCast<MapDataPrefs>("prefs", null, p_add: true);
			}
			set
			{
				Set("prefs", value);
			}
		}

		public static string GenerateGUID()
		{
			return "CMP-" + GUID.Create(24, "", 200, 0, 15, "x1");
		}

		public bool IsAllowedOnPlatform()
		{
			string prefix = OS.prefix;
			string text = "";
			string[] array = platformExclusive;
			if (array == null || array.Length == 0)
			{
				return true;
			}
			switch (prefix)
			{
			case "win":
			case "osx":
			case "unix":
				text = "steam";
				break;
			case "xbox":
				text = "xbox";
				break;
			case "ps4":
				text = "playstation";
				break;
			}
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].ToUpper() == text.ToUpper())
				{
					return true;
				}
			}
			return false;
		}

		public int GetCollaboratorCount()
		{
			return collaborators.Length;
		}

		public DRLPlayerProfileData GetCollaborator(int p_index)
		{
			if (p_index >= 0)
			{
				if (p_index < collaborators.Length)
				{
					return collaborators[p_index];
				}
				return null;
			}
			return null;
		}

		public int GetCollaboratorIndex(string p_id)
		{
			int collaboratorCount = GetCollaboratorCount();
			for (int i = 0; i < collaboratorCount; i++)
			{
				DRLPlayerProfileData collaborator = GetCollaborator(i);
				if (collaborator != null && collaborator.playerId == p_id)
				{
					return i;
				}
			}
			return -1;
		}

		public void RemoveCollaboratorById(string p_id)
		{
			int collaboratorIndex = GetCollaboratorIndex(p_id);
			if (collaboratorIndex >= 0)
			{
				List<DRLPlayerProfileData> list = new List<DRLPlayerProfileData>();
				list.AddRange(collaborators);
				list.RemoveAt(collaboratorIndex);
				collaborators = list.ToArray();
			}
		}

		public void AddCollaborator(DRLPlayerProfileData p_data)
		{
			if (p_data != null && GetCollaboratorIndex(p_data.playerId) < 0)
			{
				List<DRLPlayerProfileData> list = new List<DRLPlayerProfileData>();
				list.AddRange(collaborators);
				list.Add(p_data);
				collaborators = list.ToArray();
			}
		}

		public List<string> GetGroups()
		{
			List<string> list = new List<string>(mapGroups.Split(','));
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = list[i].Trim();
				if (string.IsNullOrEmpty(list[i]))
				{
					list.RemoveAt(i--);
				}
			}
			return list;
		}

		public MapData()
		{
			if (string.IsNullOrEmpty(guid))
			{
				guid = GenerateGUID();
			}
			root = new MDEntity();
			root.name = "$root";
			if (prefs == null)
			{
				prefs = new MapDataPrefs();
			}
		}

		public string Save(bool p_indent = false)
		{
			return Serialize.ToJson(this, p_indent);
		}

		public MapData Load(string p_json)
		{
			Serialize.FromJson(p_json, this, p_populate: true);
			root?.RefreshParenting();
			return this;
		}

		public MapData Clone()
		{
			return new MapData().Load(ToJson());
		}

		public MDEntity LoadRoot(string p_json)
		{
			root = new MDEntity();
			root.Load(p_json);
			root.RefreshParenting();
			return root;
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
