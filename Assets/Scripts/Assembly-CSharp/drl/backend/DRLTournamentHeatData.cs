using Newtonsoft.Json.Linq;
using thelab.core;

namespace drl.backend
{
	public class DRLTournamentHeatData : SerializedData
	{
		private DRLTournamentHeatResultData[] m_results;

		public string roundTitle
		{
			get
			{
				return Get("round-name", "");
			}
			set
			{
				Set("round-name", value);
			}
		}

		public string heatTitle
		{
			get
			{
				return Get("heat-name", "");
			}
			set
			{
				Set("heat-name", value);
			}
		}

		public int highscore
		{
			get
			{
				return Get("highscore", 0);
			}
			set
			{
				Set("highscore", value);
			}
		}

		public bool resultsArrived
		{
			get
			{
				return Get("results-arrived", d: false);
			}
			set
			{
				Set("results-arrived", value);
			}
		}

		public DRLTournamentHeatResultData[] results
		{
			get
			{
				if (m_results != null)
				{
					return m_results;
				}
				return new DRLTournamentHeatResultData[0];
			}
		}

		internal void WarmUp()
		{
			JArray jArray = (JArray)Get<object>("results", null);
			m_results = ((jArray == null) ? new DRLTournamentHeatResultData[0] : jArray.ToObject<DRLTournamentHeatResultData[]>());
		}
	}
}
