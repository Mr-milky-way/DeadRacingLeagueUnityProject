using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLTournamentStandingsRanksHeader : View<DRLApp>
	{
		public Text ranksField;

		public Text winsField;

		public void SetLayout(bool p_simCup)
		{
			Localization locale = base.app.model.storage.locale;
			string p_default = (p_simCup ? "TOTAL WINS" : "TOTAL");
			winsField.text = locale.Get(p_simCup ? "vdrl.label.total-wins" : "vdrl.label.total", p_default);
		}
	}
}
