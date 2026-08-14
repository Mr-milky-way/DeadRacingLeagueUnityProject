using System;

namespace drl.backend
{
	[Serializable]
	public class DRLBundleEntry
	{
		public string type;

		public string id;

		public int version;

		public string file;

		public int size;

		public string os;

		public DRLBundleEntryType GetBundleType()
		{
			return (DRLBundleEntryType)Enum.Parse(typeof(DRLBundleEntryType), type);
		}
	}
}
