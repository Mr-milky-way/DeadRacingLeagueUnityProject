namespace drl.backend
{
	public class DRLCommunityTuneResult
	{
		public DRLServicePageData pagging;

		public DRLCommunityTuneData[] data;

		public DRLCommunityTuneResult()
		{
			pagging = new DRLServicePageData();
			data = new DRLCommunityTuneData[0];
		}
	}
}
