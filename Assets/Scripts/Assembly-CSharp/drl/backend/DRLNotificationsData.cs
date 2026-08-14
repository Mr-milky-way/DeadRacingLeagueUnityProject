using thelab.core;

namespace drl.backend
{
	public class DRLNotificationsData : SerializedData
	{
		public string id => Get("id", "");

		public string guid => Get("guid", "");

		public string title => Get("title", "");

		public string description => Get<string>("description");
	}
}
