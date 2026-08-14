namespace Epic.OnlineServices.Achievements
{
	public class DefinitionV2 : ISettable
	{
		public string AchievementId { get; set; }

		public string UnlockedDisplayName { get; set; }

		public string UnlockedDescription { get; set; }

		public string LockedDisplayName { get; set; }

		public string LockedDescription { get; set; }

		public string FlavorText { get; set; }

		public string UnlockedIconURL { get; set; }

		public string LockedIconURL { get; set; }

		public bool IsHidden { get; set; }

		public StatThresholds[] StatThresholds { get; set; }

		internal void Set(DefinitionV2Internal? other)
		{
			if (other.HasValue)
			{
				AchievementId = other.Value.AchievementId;
				UnlockedDisplayName = other.Value.UnlockedDisplayName;
				UnlockedDescription = other.Value.UnlockedDescription;
				LockedDisplayName = other.Value.LockedDisplayName;
				LockedDescription = other.Value.LockedDescription;
				FlavorText = other.Value.FlavorText;
				UnlockedIconURL = other.Value.UnlockedIconURL;
				LockedIconURL = other.Value.LockedIconURL;
				IsHidden = other.Value.IsHidden;
				StatThresholds = other.Value.StatThresholds;
			}
		}

		public void Set(object other)
		{
			Set(other as DefinitionV2Internal?);
		}
	}
}
