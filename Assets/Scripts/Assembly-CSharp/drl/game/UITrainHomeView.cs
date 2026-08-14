using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UITrainHomeView : UIScreenView
	{
		public DRLQuest quest;

		public DRLMission mission;

		public ListComponent listField;

		public DRLMarkerProgressView progressField;

		public Text timeField;

		public Text attemptsField;

		public UILeaderboardCardView leaderCard;

		public UILeaderboardCardView userCard;

		public FadeComponent navRightFade;

		public Text resetConfirmationField;

		public UINavigation resetButtonNav;

		public UINavigation exitNav;

		public UINavigation backNav;

		public UINavigation leadersNav;

		public FadeSlideComponent qualifySuccessFade;

		public FadeSlideComponent qualifyFailFade;

		public Text qualifyTimeField;

		public UINavigation beginnerButtonNavigation;

		public DRLCampaign data;

		public UIScreenManager manager => AssertLocal<UIScreenManager>("manager");

		public void SetBackEnabled(bool p_flag)
		{
			if ((bool)backNav)
			{
				backNav.gameObject.SetActive(p_flag);
			}
		}
	}
}
