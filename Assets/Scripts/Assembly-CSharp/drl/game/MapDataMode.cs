using System;

namespace drl.game
{
	public class MapDataMode
	{
		public MapData parent;

		private MapDataRaceMode m_race;

		private MapDataCollectableMode m_collectable;

		public string type
		{
			get
			{
				return parent.Get("map-mode-type", "");
			}
			set
			{
				parent.mapDirty = true;
				parent.Set("map-mode-type", value);
			}
		}

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
			set
			{
				string text = value.ToString();
				type = text;
			}
		}

		public MapDataRaceMode race
		{
			get
			{
				if (m_race != null)
				{
					return m_race;
				}
				return m_race = new MapDataRaceMode(parent);
			}
		}

		public MapDataCollectableMode collectable
		{
			get
			{
				if (m_collectable != null)
				{
					return m_collectable;
				}
				return m_collectable = new MapDataCollectableMode(parent);
			}
		}

		public MapDataMode(MapData p_parent)
		{
			parent = p_parent;
		}
	}
}
