using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLTournamentRaceEndStandingsItem : View<DRLApp>
	{
		public Text rankField;

		public Text usernameField;

		public Text resetsField;

		public Text timeField;

		public Image rankImage;

		public Image userColorImage;

		public UIElementView replayButtonView;

		public UINavigation replayNavigation;

		public GameObject resets;

		public GameObject replay;

		private string replayURL;

		public void Set(string p_rank, string p_username, string p_crashes, string p_score, Color p_color, RaceStatusType p_status, string p_replayURL = null, bool p_useReplay = false)
		{
			rankField.text = p_rank;
			usernameField.text = p_username.ToUpper();
			resetsField.text = p_crashes;
			if (p_status != RaceStatusType.None && p_status != RaceStatusType.Success)
			{
				p_score = "DNF";
			}
			timeField.text = p_score;
			rankImage.color = p_color;
			userColorImage.color = p_color;
			resets.SetActive(!p_useReplay);
			replay.SetActive(p_useReplay);
			replayButtonView.interactable = !string.IsNullOrEmpty(p_replayURL);
			replayURL = p_replayURL;
		}

		public void StartReplay()
		{
			if (!string.IsNullOrEmpty(replayURL))
			{
				Notify("leaderboards.item.replay@click", replayURL);
			}
		}
	}
}
