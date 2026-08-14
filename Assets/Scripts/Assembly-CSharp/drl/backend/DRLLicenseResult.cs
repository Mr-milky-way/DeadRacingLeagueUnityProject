using thelab.core;

namespace drl.backend
{
	public class DRLLicenseResult : SerializedData
	{
		public bool exists => Get("exists", d: false);

		public int id => Get("id", 11);

		public float cost => Get("cost", 19.99f);
	}
}
