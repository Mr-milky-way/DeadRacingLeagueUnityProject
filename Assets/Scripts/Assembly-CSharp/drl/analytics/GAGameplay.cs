namespace drl.analytics
{
	public class GAGameplay : IGAModule
	{
		public string Id => "gameplay";

		public void LoadGame(string gamemode, string gametype, string mapId, string trackId)
		{
			string text = gamemode + ":" + gametype + ":" + mapId;
			if (!string.IsNullOrEmpty(trackId))
			{
				text = text + ":" + trackId;
			}
			if (!string.IsNullOrEmpty(text))
			{
				GADesign.DesignEvent(Id + ":" + text.ToLower());
			}
		}
	}
}
