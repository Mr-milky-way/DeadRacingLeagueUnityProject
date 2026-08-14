namespace drl.analytics
{
	public class GAUserInterface : IGAModule
	{
		public string Id => "UI";

		public void ClickMenuCard(string menuCard)
		{
			if (!string.IsNullOrEmpty(menuCard))
			{
				GADesign.DesignEvent(Id + ":" + menuCard.ToLower());
			}
		}
	}
}
