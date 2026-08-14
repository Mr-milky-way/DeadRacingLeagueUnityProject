using drl.game;

namespace drl.backend
{
	public class DRLMapDataResult
	{
		public DRLServicePageData pagging;

		public MapData[] data;

		public bool success;

		public DRLMapDataResult()
		{
			success = true;
			pagging = new DRLServicePageData();
			data = new MapData[0];
		}
	}
}
