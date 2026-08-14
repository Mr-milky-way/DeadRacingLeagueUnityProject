using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLTournamentStandingsHeatsHeader : View<DRLApp>
	{
		public ListComponent list;

		public void Set(int p_heatCount, int p_activeHeat, bool p_suddenDeath = false, bool p_goldenHeat = false)
		{
			list.Clear();
			for (int i = 0; i < p_heatCount; i++)
			{
				DRLTournamentStandingsHeatHeaderItem dRLTournamentStandingsHeatHeaderItem = list.Push<DRLTournamentStandingsHeatHeaderItem>();
				int num = Mathf.Clamp(p_activeHeat, 1, p_heatCount);
				dRLTournamentStandingsHeatHeaderItem.SetHeatActive(num == i + 1);
				if (i < p_heatCount - 1)
				{
					dRLTournamentStandingsHeatHeaderItem.SetHeatTitle(GetHeatTitle(i + 1));
					continue;
				}
				if (p_suddenDeath)
				{
					dRLTournamentStandingsHeatHeaderItem.SetHeatSD();
				}
				if (p_goldenHeat)
				{
					dRLTournamentStandingsHeatHeaderItem.SetHeatGH();
				}
				if (!p_suddenDeath && !p_goldenHeat)
				{
					dRLTournamentStandingsHeatHeaderItem.SetHeatTitle(GetHeatTitle(p_heatCount));
				}
			}
		}

		private string GetHeatTitle(int p_heatIdx)
		{
			return base.app.model.storage.locale.Get("vdrl.label.heat", "HEAT") + " " + p_heatIdx;
		}
	}
}
