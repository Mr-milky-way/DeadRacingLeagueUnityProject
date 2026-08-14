namespace thelab.mvc
{
	public class UIAchievementButtonView : UIElementView
	{
		private string achievementID;

		public string AchievementID
		{
			get
			{
				return achievementID;
			}
			set
			{
				achievementID = value;
			}
		}
	}
}
