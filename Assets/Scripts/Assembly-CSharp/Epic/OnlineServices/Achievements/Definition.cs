namespace Epic.OnlineServices.Achievements
{
	public class Definition : ISettable
	{
		public string AchievementId { get; set; }

		public string DisplayName { get; set; }

		public string Description { get; set; }

		public string LockedDisplayName { get; set; }

		public string LockedDescription { get; set; }

		public string HiddenDescription { get; set; }

		public string CompletionDescription { get; set; }

		public string UnlockedIconId { get; set; }

		public string LockedIconId { get; set; }

		public bool IsHidden { get; set; }

		public StatThresholds[] StatThresholds { get; set; }

		internal void Set(DefinitionInternal? other)
		{
			if (other.HasValue)
			{
				AchievementId = other.Value.AchievementId;
				DisplayName = other.Value.DisplayName;
				Description = other.Value.Description;
				LockedDisplayName = other.Value.LockedDisplayName;
				LockedDescription = other.Value.LockedDescription;
				HiddenDescription = other.Value.HiddenDescription;
				CompletionDescription = other.Value.CompletionDescription;
				UnlockedIconId = other.Value.UnlockedIconId;
				LockedIconId = other.Value.LockedIconId;
				IsHidden = other.Value.IsHidden;
				StatThresholds = other.Value.StatThresholds;
			}
		}

		public void Set(object other)
		{
			Set(other as DefinitionInternal?);
		}
	}
}
