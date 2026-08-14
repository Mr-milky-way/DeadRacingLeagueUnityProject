using thelab.core;

namespace drl.backend
{
	public class DRLAchievementRequirementsData : SerializedData
	{
		public string _id => Get("Id", "");

		public string title => Get("title", "");

		public string name => Get("name", "");

		public string achievementID => Get("achievement", "");

		public string type => Get("type", "");

		public string track => Get("track", "");

		public string guid => Get("guid", "");

		public string map => Get("map", "");

		public bool isCustomMap => Get("is-custom-map", d: false);

		public string customMap => Get("custom-map", "");

		public bool hasSubTasks => Get("Has-sub-tasks", d: false);

		public float completed => Get("completed", 0f);

		public float progression => Get("progression", 0f);

		public string lbEntries => Get("Lb-entries", "");
	}
}
