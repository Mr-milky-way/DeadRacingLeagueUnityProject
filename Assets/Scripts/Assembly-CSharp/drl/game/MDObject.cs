using thelab.core;

namespace drl.game
{
	public class MDObject : SerializedData
	{
		public string id
		{
			get
			{
				return Get("id", "");
			}
			set
			{
				Set("id", value);
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

		public bool replacedGUID
		{
			get
			{
				return Get("replaced-guid", d: false);
			}
			set
			{
				Set("replaced-guid", value);
			}
		}

		public string name
		{
			get
			{
				return Get("name", "Asset");
			}
			set
			{
				Set("name", value);
			}
		}

		public MapAssetType type
		{
			get
			{
				return (MapAssetType)Get("type", 0);
			}
			set
			{
				Set("type", (int)value);
			}
		}

		public MapAssetType category
		{
			get
			{
				if (type >= MapAssetType.__Entities_ && type < MapAssetType.__Renderer_)
				{
					return MapAssetType.Entity;
				}
				if (type >= MapAssetType.__Renderer_ && type < MapAssetType.__Game_)
				{
					return MapAssetType.Renderer;
				}
				if (type >= MapAssetType.__Game_ && type < MapAssetType.__Guides_)
				{
					return MapAssetType.Gate;
				}
				if (type >= MapAssetType.__Guides_ && type < MapAssetType.__Splines_)
				{
					return MapAssetType.Guide;
				}
				if (type >= MapAssetType.__Splines_ && type < MapAssetType.__Podiums_)
				{
					return MapAssetType.Spline;
				}
				if (type >= MapAssetType.__Podiums_ && type < MapAssetType.__Props_)
				{
					return MapAssetType.Podium;
				}
				if (type >= MapAssetType.__Props_ && type < MapAssetType.__RaceProps_)
				{
					return MapAssetType.Prop;
				}
				if (type >= MapAssetType.__RaceProps_)
				{
					_ = type;
					_ = 900;
					return MapAssetType.Tool;
				}
				return MapAssetType.Tool;
			}
		}

		public static string GenerateId()
		{
			return "DMO-" + GUID.Create(24, "", 200, 0, 15, "x1");
		}

		public MDObject()
		{
			if (string.IsNullOrEmpty(id))
			{
				id = GenerateId();
			}
		}

		public string Save(bool p_indent = false)
		{
			return Serialize.ToJson(this, p_indent);
		}

		public void Load(string p_json)
		{
			Serialize.FromJson(p_json, this, p_populate: true);
		}

		public virtual string ToJsonProperties(bool p_indented)
		{
			return ToJson(p_indented);
		}
	}
}
