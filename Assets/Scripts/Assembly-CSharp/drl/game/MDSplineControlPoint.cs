namespace drl.game
{
	public class MDSplineControlPoint : MDGuide
	{
		public int index
		{
			get
			{
				return Get("scp-index", -1);
			}
			set
			{
				Set("scp-index", value);
			}
		}

		public MDSplineControlPoint()
		{
			base.type = MapAssetType.SplineControlPoint;
		}
	}
}
