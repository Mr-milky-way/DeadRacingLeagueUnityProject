using Newtonsoft.Json.Linq;
using thelab.core;

namespace drl.backend
{
	public class DRLTournamentPlacementsData : SerializedData
	{
		public int activeRound
		{
			get
			{
				return Get("active-round", 0);
			}
			set
			{
				Set("active-round", value);
			}
		}

		public DRLPlacementsUserData[] semi1
		{
			get
			{
				JArray jArray = (JArray)Get<object>("semi-one", null);
				if (jArray != null)
				{
					return jArray.ToObject<DRLPlacementsUserData[]>();
				}
				return new DRLPlacementsUserData[0];
			}
		}

		public DRLPlacementsUserData[] semi2
		{
			get
			{
				JArray jArray = (JArray)Get<object>("semi-two", null);
				if (jArray != null)
				{
					return jArray.ToObject<DRLPlacementsUserData[]>();
				}
				return new DRLPlacementsUserData[0];
			}
		}

		public DRLPlacementsUserData[] finals
		{
			get
			{
				JArray jArray = (JArray)Get<object>("finals", null);
				if (jArray != null)
				{
					return jArray.ToObject<DRLPlacementsUserData[]>();
				}
				return new DRLPlacementsUserData[0];
			}
		}
	}
}
