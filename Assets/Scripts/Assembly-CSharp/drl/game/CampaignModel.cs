using drl.backend;

namespace drl.game
{
	public class CampaignModel : RaceModel
	{
		public DRLRaceResultData data;

		public int count;

		public int race;

		public int phase;

		public string phaseName;

		public int heat;

		public bool campaignComplete;

		public bool replayUploadComplete;

		public CampaignResultsModel results => base.app.model.storage.state.player.results.campaign;

		public DRLCampaign campaign => base.app.arguments.game.campaign;

		public override bool IsComplete()
		{
			if (base.IsComplete())
			{
				return replayUploadComplete;
			}
			return false;
		}
	}
}
