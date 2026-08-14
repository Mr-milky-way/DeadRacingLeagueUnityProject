using Newtonsoft.Json;

namespace drl.backend
{
	public class DRLTournamentSocketMetaData
	{
		[JsonProperty("player-id")]
		public string playerId;
	}
}
