using thelab.core;

namespace drl.backend
{
	public class DRLSocketTestData : SerializedData
	{
		public string someData
		{
			get
			{
				return Get("someData", "default");
			}
			set
			{
				Set("someData", value);
			}
		}
	}
}
