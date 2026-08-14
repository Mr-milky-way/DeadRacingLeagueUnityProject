namespace drl.backend
{
	public class DRLCommunityDroneResult
	{
		public DRLServicePageData pagging;

		public DRLCommunityDroneData[] data;

		public DRLCommunityDroneResult()
		{
			pagging = new DRLServicePageData();
			data = new DRLCommunityDroneData[0];
		}
	}
}
