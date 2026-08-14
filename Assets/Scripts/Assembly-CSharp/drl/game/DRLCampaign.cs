using System.Collections.Generic;
using UnityEngine;
using drl.sim;

namespace drl.game
{
	public class DRLCampaign : DRLGameAsset
	{
		[TextArea(1, 4)]
		public string title;

		[TextArea(1, 4)]
		public string description;

		public bool tournament;

		public DroneRigData drone;

		public string podium;

		public List<DRLCampaignRace> races;

		public float qualifyTime;

		public string label => title.ToUpper().Replace("\n", " ");

		public int total
		{
			get
			{
				int num = 0;
				for (int i = 0; i < races.Count; i++)
				{
					num += races[i].total;
				}
				return num;
			}
		}

		public bool hasCustomMaps
		{
			get
			{
				for (int i = 0; i < races.Count; i++)
				{
					if (!string.IsNullOrEmpty(races[i].customMapId))
					{
						return true;
					}
				}
				return false;
			}
		}

		public int GetCompleteCount(int p_id)
		{
			int num = 0;
			int num2 = 0;
			if (p_id >= total)
			{
				return total;
			}
			for (int i = 0; i < races.Count; i++)
			{
				DRLCampaignRace dRLCampaignRace = races[i];
				int num3 = num2 + dRLCampaignRace.total;
				num2 += dRLCampaignRace.total;
				if (dRLCampaignRace != null && dRLCampaignRace.hasTrack && dRLCampaignRace.total > 0)
				{
					if (p_id < num3)
					{
						break;
					}
					num++;
				}
			}
			return num;
		}

		public bool IsRaceComplete(int p_id, int p_race_id)
		{
			if (p_race_id < 0)
			{
				return false;
			}
			return GetRaceIndex(p_id) > p_race_id;
		}

		public bool IsComplete(int p_id)
		{
			return GetCompleteCount(p_id) >= total;
		}

		public DRLCampaignRace GetRace(int p_id, out int p_phase, out int p_heat)
		{
			int raceIndex = GetRaceIndex(p_id, out p_phase, out p_heat);
			if (raceIndex >= 0)
			{
				return races[raceIndex];
			}
			return null;
		}

		public DRLCampaignRace GetRace(int p_id)
		{
			int p_phase = 0;
			int p_heat = 0;
			return GetRace(p_id, out p_phase, out p_heat);
		}

		public int GetRaceIndex(int p_id, out int p_phase, out int p_heat)
		{
			p_phase = -1;
			p_heat = -1;
			if (p_id < 0)
			{
				return -1;
			}
			if (p_id >= total)
			{
				return -1;
			}
			if (races.Count <= 0)
			{
				return -1;
			}
			int num = 0;
			for (int i = 0; i < races.Count; i++)
			{
				DRLCampaignRace dRLCampaignRace = races[i];
				int num2 = num;
				int num3 = num + dRLCampaignRace.total;
				num += dRLCampaignRace.total;
				if (dRLCampaignRace != null && dRLCampaignRace.hasTrack && dRLCampaignRace.total > 0 && p_id >= num2 && p_id < num3)
				{
					int num4 = p_id - num2;
					int num5 = num4 / dRLCampaignRace.heats;
					int num6 = num4 % dRLCampaignRace.heats;
					p_phase = num5;
					p_heat = num6;
					return i;
				}
			}
			return -1;
		}

		public int GetRaceIndex(int p_id)
		{
			int p_phase = 0;
			int p_heat = 0;
			return GetRaceIndex(p_id, out p_phase, out p_heat);
		}

		public override string GetPrefix()
		{
			return "CP";
		}
	}
}
