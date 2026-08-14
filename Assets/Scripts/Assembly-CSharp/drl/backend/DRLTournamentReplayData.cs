using thelab.core;

namespace drl.backend
{
	public class DRLTournamentReplayData : SerializedData
	{
		private bool m_backendReplaysReady;

		public int heat
		{
			get
			{
				return Get("heat", -1);
			}
			set
			{
				Set("heat", value);
			}
		}

		public string URLs
		{
			get
			{
				return Get("urls", "");
			}
			set
			{
				Set("urls", value);
			}
		}

		public bool replaysReady { get; set; }

		public bool backendReplaysReady => m_backendReplaysReady;

		public DRLTournamentReplayData()
		{
			m_backendReplaysReady = true;
		}

		public DRLTournamentReplayData(int p_heat, string p_urls)
		{
			heat = p_heat;
			URLs = p_urls;
		}

		public string[] GetHeatReplays()
		{
			if (string.IsNullOrEmpty(URLs))
			{
				return null;
			}
			string[] array = URLs.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Trim(';');
			}
			return array;
		}

		public void Copy(DRLTournamentReplayData p_data)
		{
			if (p_data == null || p_data.heat == -1)
			{
				return;
			}
			heat = p_data.heat;
			if (!string.IsNullOrEmpty(URLs) && !string.IsNullOrEmpty(p_data.URLs))
			{
				string[] array = URLs.Trim(';').Split(';');
				string[] array2 = p_data.URLs.Trim(';').Split(';');
				if (array.Length <= array2.Length)
				{
					URLs = p_data.URLs;
					replaysReady = p_data.replaysReady;
				}
			}
		}
	}
}
