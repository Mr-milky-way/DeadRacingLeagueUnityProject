using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentCollectablesCompleteView : UIScreenView
	{
		[HideInInspector]
		public RaceController race;

		public FadeComponent headerFade;

		public RectTransform header;

		public DRLTournamentRaceEndStandings standings;

		public Text roundTitle;

		public Text heatTitle;

		public Text courseHighScoreField;

		public UIElementView watchButton;

		public UIElementView spectateButton;

		public UIElementView exitButton;

		public UIElementView nextButton;

		public VideoPlayer videoPlayer;

		public VideoClip videoClip;

		public FadeComponent videoFade;

		[Header("Feedback:")]
		public FadeComponent feedbackFade;

		public GameObject feedbackNoResults;

		public GameObject feedbackLoading;

		public GameObject feedbackPending;

		public void SetTitle(string p_round_name, int p_activeHeatIdx, int p_heatCount, bool p_isSuddenDeath, bool p_isGoldenHeat)
		{
			roundTitle.text = p_round_name.ToUpper();
			string text = "";
			if (p_activeHeatIdx < p_heatCount)
			{
				text = base.app.model.storage.locale.Get("vdrl.label.heat", "HEAT") + " " + p_activeHeatIdx;
			}
			else
			{
				if (p_isSuddenDeath)
				{
					text = base.app.model.storage.locale.Get("vdrl.label.sudden-death", "SUDDEN DEATH");
				}
				if (p_isGoldenHeat)
				{
					text = base.app.model.storage.locale.Get("vdrl.label.golden-heat", "GOLDEN HEAT");
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				text = base.app.model.storage.locale.Get("vdrl.label.heat", "HEAT") + " " + p_activeHeatIdx;
			}
			heatTitle.text = text.ToUpper();
			LayoutRebuilder.ForceRebuildLayoutImmediate(header);
		}

		public void Set(DRLTournamentHeatData p_data)
		{
			if (p_data != null)
			{
				standings.Set(p_data.results);
				LayoutRebuilder.ForceRebuildLayoutImmediate(header);
				FadeIn();
				courseHighScoreField.text = Format.MsToTime(p_data.highscore, "m\\:ss\\.fff");
				nextButton.interactable = nextButton.interactable || p_data.resultsArrived;
			}
		}

		public void FadeIn()
		{
			if (headerFade.alpha < 0.4f)
			{
				headerFade.FadeIn();
			}
			if (standings.fade.alpha < 0.4f)
			{
				standings.fade.FadeIn();
			}
		}

		public void FadeOut()
		{
			headerFade.FadeOut(0f);
			standings.fade.FadeOut(0f);
		}

		public void SetFeedback(UITournamentLeaderboardFeedbackType p_feedback)
		{
			switch (p_feedback)
			{
			case UITournamentLeaderboardFeedbackType.None:
				feedbackFade.FadeOut();
				feedbackLoading.SetActive(value: false);
				feedbackNoResults.SetActive(value: false);
				break;
			case UITournamentLeaderboardFeedbackType.Loading:
				feedbackLoading.SetActive(value: true);
				feedbackNoResults.SetActive(value: false);
				feedbackPending.SetActive(value: false);
				feedbackFade.FadeIn();
				break;
			case UITournamentLeaderboardFeedbackType.NoResult:
				feedbackLoading.SetActive(value: false);
				feedbackNoResults.SetActive(value: true);
				feedbackPending.SetActive(value: false);
				feedbackFade.FadeIn();
				break;
			case UITournamentLeaderboardFeedbackType.Pending:
				feedbackLoading.SetActive(value: false);
				feedbackNoResults.SetActive(value: false);
				feedbackPending.SetActive(value: true);
				feedbackFade.FadeIn();
				break;
			}
		}

		public void StartVideo(bool p_randomStart = true)
		{
			if ((bool)videoPlayer && (bool)videoClip)
			{
				videoPlayer.clip = videoClip;
				if (p_randomStart)
				{
					float num = (float)videoPlayer.length;
					float num2 = Random.Range(0f, num - 1f);
					videoPlayer.time = num2;
				}
				StartClip();
			}
		}

		public void StopVideo()
		{
			if ((bool)videoPlayer)
			{
				videoPlayer.Stop();
				videoFade.FadeOut(0f);
				videoPlayer.clip = null;
				videoPlayer.targetTexture.Release();
			}
		}

		private void StartClip()
		{
			videoPlayer.Play();
			videoFade.FadeIn();
		}
	}
}
