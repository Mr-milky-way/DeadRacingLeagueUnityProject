using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UITournamentRaceEndsView : UIScreenView
	{
		public Text title;

		public void SetTitle(string p_roundName, bool p_is_terminated = false)
		{
			Localization locale = base.app.model.storage.locale;
			string text = (p_is_terminated ? locale.Get("vdrl.label.match-reset", "MATCH RESET") : locale.Get("vdrl.label.time-expired", "TIME EXPIRED"));
			string text2 = (p_is_terminated ? locale.Get("vdrl.label.round-terminated", "TERMINATED") : locale.Get("vdrl.label.round-complete", "COMPLETE"));
			title.text = text + " <color=#f00>/</color> " + p_roundName + " " + text2;
		}
	}
}
