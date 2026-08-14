using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UITournamentDAWCView : UIScreenView
	{
		public ListComponent listSF1;

		public ListComponent listSF2;

		public ListComponent listF;

		public Text sf1Title;

		public Text sf2Title;

		public Text fTitle;

		public Color titleInactiveColor;

		public VideoPlayer videoPlayer;

		public VideoClip videoClip;

		public FadeComponent videoFade;

		[Header("Feedback:")]
		public FadeComponent feedbackFade;

		public GameObject feedbackNoResults;

		public GameObject feedbackLoading;

		public GameObject feedbackPending;

		public void Set(DRLTournamentPlacementsData p_results)
		{
			Clear();
			for (int i = 0; i < p_results.semi1.Length; i++)
			{
				listSF1.Push<DRLTournamentDAWCItem>().Set(p_results.semi1[i]);
			}
			for (int j = 0; j < p_results.semi2.Length; j++)
			{
				listSF2.Push<DRLTournamentDAWCItem>().Set(p_results.semi2[j]);
			}
			for (int k = 0; k < p_results.finals.Length; k++)
			{
				listF.Push<DRLTournamentDAWCItem>().Set(p_results.finals[k]);
			}
			SetFeedback(UITournamentLeaderboardFeedbackType.None);
		}

		public void Clear()
		{
			for (int i = 0; i < listSF1.Count; i++)
			{
				listSF1.Get<DRLTournamentDAWCItem>(i).Clear();
			}
			for (int j = 0; j < listSF2.Count; j++)
			{
				listSF2.Get<DRLTournamentDAWCItem>(j).Clear();
			}
			for (int k = 0; k < listF.Count; k++)
			{
				listF.Get<DRLTournamentDAWCItem>(k).Clear();
			}
			listSF1.Clear();
			listSF2.Clear();
			listF.Clear();
		}

		public void SetActiveRoundtitle(int p_activeRound)
		{
			sf1Title.color = titleInactiveColor;
			sf2Title.color = titleInactiveColor;
			fTitle.color = titleInactiveColor;
			switch (p_activeRound)
			{
			case 2:
				sf1Title.color = Color.white;
				break;
			case 3:
				sf2Title.color = Color.white;
				break;
			case 4:
				fTitle.color = Color.white;
				break;
			}
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

		public void StartVideo(bool p_randomStart = false)
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
