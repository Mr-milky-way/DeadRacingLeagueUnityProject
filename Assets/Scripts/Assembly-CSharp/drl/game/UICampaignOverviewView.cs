using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UICampaignOverviewView : UIScreenView
	{
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

		public DRLCampaign data;

		private float m_time;

		public int attemptsCount
		{
			set
			{
				attemptsField.text = value.ToString() ?? "";
			}
		}

		public int raceCount
		{
			set
			{
				progressField.SetCount(value);
			}
		}

		public int raceTotal
		{
			set
			{
				progressField.SetTotal(value);
			}
		}

		public float time
		{
			get
			{
				return m_time;
			}
			set
			{
				m_time = value;
				if (m_time <= 0f)
				{
					timeField.text = "--:--:--";
				}
				else
				{
					timeField.text = Format.SecondsToTime(m_time, 2, p_use_ms: true);
				}
			}
		}

		public void SetProgress(int p_count, int p_total)
		{
			raceCount = p_count + 1;
			raceTotal = p_total;
			progressField.right = ((p_total <= 0) ? "--/--" : (p_count + 1 + "/" + p_total));
		}

		public void SetQualifyTime(float p_time)
		{
			qualifyTimeField.text = ((p_time <= 0f) ? "NONE" : Format.SecondsToTime(p_time, 2, p_use_ms: true));
		}

		public void SetQualifySuccess(int p_state, float p_delay = 0f)
		{
			qualifySuccessFade.transition = 1f;
			qualifyFailFade.transition = 1f;
			if (p_state == 1)
			{
				qualifySuccessFade.Fade(1f, 0f, 0.3f, p_delay);
			}
			if (p_state == 2)
			{
				qualifyFailFade.Fade(1f, 0f, 0.3f, p_delay);
			}
		}

		public void Clear()
		{
			listField.Clear();
		}

		public void SetExitEnabled(bool p_flag)
		{
			if ((bool)exitNav)
			{
				exitNav.gameObject.SetActive(p_flag);
			}
		}

		public void SetLeadersEnabled(bool p_flag)
		{
			if ((bool)leadersNav)
			{
				leadersNav.gameObject.SetActive(p_flag);
			}
		}

		public void SetBackEnabled(bool p_flag)
		{
			if ((bool)backNav)
			{
				backNav.gameObject.SetActive(p_flag);
			}
		}

		public void Add(DRLCampaignRace p_race)
		{
			if (p_race == null)
			{
				Debug.LogWarning("UICampaignOverviewView> Add - Invalid Race");
				return;
			}
			UICardButtonCampaignMap uICardButtonCampaignMap = listField.Push<UICardButtonCampaignMap>();
			uICardButtonCampaignMap.notification = "campaign.campaign-map-card";
			uICardButtonCampaignMap.Set(p_race);
			uICardButtonCampaignMap.SetLeaderboard(0);
			uICardButtonCampaignMap.SetResult("");
			uICardButtonCampaignMap.Clear();
		}
	}
}
