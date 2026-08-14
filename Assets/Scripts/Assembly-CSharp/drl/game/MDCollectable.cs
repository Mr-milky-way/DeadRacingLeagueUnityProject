namespace drl.game
{
	public class MDCollectable : MDRenderer
	{
		public int index
		{
			get
			{
				return Get("collectable-index", -1);
			}
			set
			{
				Set("collectable-index", value);
			}
		}

		public int size
		{
			get
			{
				return Get("collectable-size", -1);
			}
			set
			{
				Set("collectable-size", value);
			}
		}

		public MapCollectableMode mode
		{
			get
			{
				return (MapCollectableMode)Get("collectable-mode", 1);
			}
			set
			{
				Set("collectable-mode", (int)value);
			}
		}

		public int score
		{
			get
			{
				return Get("collectable-score", 1);
			}
			set
			{
				Set("collectable-score", value);
			}
		}

		public string group
		{
			get
			{
				return Get("collectable-group", "");
			}
			set
			{
				Set("collectable-group", value);
			}
		}

		public int groupBonus
		{
			get
			{
				return Get("collectable-group-bonus", 1);
			}
			set
			{
				Set("collectable-group-bonus", value);
			}
		}

		public MapCollectableGroupMode groupMode
		{
			get
			{
				return (MapCollectableGroupMode)Get("collectable-group-mode", 1);
			}
			set
			{
				Set("collectable-group-mode", (int)value);
			}
		}

		public int style
		{
			get
			{
				return Get("collectable-style", 0);
			}
			set
			{
				Set("collectable-style", value);
			}
		}

		public MDCollectable()
		{
			base.type = MapAssetType.Collectable;
		}
	}
}
