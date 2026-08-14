namespace drl.backend
{
	public class DRLCommunityMapResult
	{
		public DRLServicePageData pagging;

		public DRLCommunityMapData[] data;

		public bool success;

		public DRLCommunityMapResult()
		{
			success = true;
			pagging = new DRLServicePageData();
			data = new DRLCommunityMapData[0];
		}
	}
}
