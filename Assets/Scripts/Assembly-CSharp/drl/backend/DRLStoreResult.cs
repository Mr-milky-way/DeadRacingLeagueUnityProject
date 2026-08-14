namespace drl.backend
{
	public class DRLStoreResult
	{
		public bool success;

		public DRLServicePageData pagging;

		public DRLStoreProductData[] data;

		public DRLStoreResult()
		{
			success = true;
			pagging = new DRLServicePageData();
			data = new DRLStoreProductData[0];
		}
	}
}
