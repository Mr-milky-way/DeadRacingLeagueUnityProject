namespace drl.game
{
	public class MDPodium : MDRenderer
	{
		public int index
		{
			get
			{
				return Get("podium-index", -1);
			}
			set
			{
				Set("podium-index", value);
			}
		}

		public MDPodium()
		{
			base.type = MapAssetType.Podium;
		}
	}
}
