namespace drl.network
{
	public class MasterRegion
	{
		private Region region;

		public CloudRegionCode Code
		{
			get
			{
				if (region != null)
				{
					return region.Code;
				}
				return CloudRegionCode.none;
			}
		}

		public int Ping
		{
			get
			{
				if (region != null)
				{
					return region.Ping;
				}
				return 1000;
			}
		}

		public MasterRegion(Region photonRegion)
		{
			region = photonRegion;
		}
	}
}
