namespace drl.analytics
{
	public class GAControllers : IGAModule
	{
		private string lastConnected = "";

		public string Id => "controllers";

		public void ConnectedNew(string hardwareName)
		{
			if (!(lastConnected == hardwareName))
			{
				lastConnected = hardwareName;
				GADesign.DesignEvent(Id + ":connected:" + hardwareName);
			}
		}
	}
}
