namespace drl.game
{
	public class MapDataRaceMode
	{
		public MapData parent;

		public int lapCount
		{
			get
			{
				return parent.Get("map-laps", 1);
			}
			set
			{
				parent.Set("map-laps", value);
			}
		}

		public float distance
		{
			get
			{
				return parent.Get("map-distance", 0f);
			}
			set
			{
				parent.Set("map-distance", value);
			}
		}

		public bool allowed
		{
			get
			{
				return parent.Get("is-race-allowed", d: false);
			}
			set
			{
				parent.Set("is-race-allowed", value);
			}
		}

		public MapDataRaceMode(MapData p_parent)
		{
			parent = p_parent;
		}
	}
}
