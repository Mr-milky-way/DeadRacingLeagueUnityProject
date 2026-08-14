using thelab.core;

namespace drl.backend
{
	public class DRLTournamentSubscription : SerializedData
	{
		public string id
		{
			get
			{
				object obj = Get<object>("id", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public string platformId
		{
			get
			{
				object obj = Get<object>(DRLService.PlatformIdKey, null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public string playerId
		{
			get
			{
				object obj = Get<object>("player-id", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public string tournamentId
		{
			get
			{
				object obj = Get<object>("tournament-id", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public string createdAt
		{
			get
			{
				object obj = Get<object>("created-at", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public string updatedAt
		{
			get
			{
				object obj = Get<object>("updated-at", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public int v => Get("v", 0);
	}
}
