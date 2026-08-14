namespace drl.game
{
	public class MapDataCollectableMode
	{
		public MapData parent;

		public int collectableCount
		{
			get
			{
				return parent.Get("cm-collectable-count", 1);
			}
			set
			{
				parent.Set("cm-collectable-count", value);
			}
		}

		public MapDataCollectableMode(MapData p_parent)
		{
			parent = p_parent;
		}
	}
}
