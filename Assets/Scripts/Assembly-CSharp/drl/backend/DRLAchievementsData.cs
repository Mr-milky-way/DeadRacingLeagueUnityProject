using Newtonsoft.Json.Linq;
using thelab.core;

namespace drl.backend
{
	public class DRLAchievementsData : SerializedData
	{
		public string achievementId => Get("id", "");

		public float progression => Get("progression", 0f);

		public DRLAchievementData achievement => Get<JObject>("achievement").ToObject<DRLAchievementData>();

		public string platformId => Get("platform-id", "");

		public float progress => Get("progress", 0f);

		public string lockedMessage => Get("locked-message", "");

		public string unlockedMessage => Get("unlocked-message", "");

		public string title => Get("title", "");
	}
}
