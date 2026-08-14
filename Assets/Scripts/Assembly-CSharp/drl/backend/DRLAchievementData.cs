using thelab.core;

namespace drl.backend
{
	public class DRLAchievementData : SerializedData
	{
		public string _id => Get("_id", "");

		public string title => Get("title", "");

		public float progression => Get("progression", 0f);

		public string lockedMessage => Get("locked-message", "");

		public string unlockedMessage => Get("unlocked-message", "");

		public string lockedImageURL => Get("locked-image-url", "");

		public string unlockedImageURL => Get("unlocked-image-url", "");

		public string steamId => Get("steam-id", "");

		public string xboxId => Get("xbox-id", "");

		public string slug => Get("slug", "");

		public string epicId => Get("epic-id", "");

		public string id => Get("id", "");

		public bool hasRequirements => Get("has-requirements", d: false);

		public int xpBonus => Get("completion-xp", 0);
	}
}
