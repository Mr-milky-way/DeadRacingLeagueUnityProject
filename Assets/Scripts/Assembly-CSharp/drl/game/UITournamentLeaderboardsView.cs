using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UITournamentLeaderboardsView : UIScreenView
	{
		public ListComponent raceListField;

		public FadeComponent raceListFade;

		public DRLPagePickerView racePageField;

		public List<GameObject> feedbacks;

		public FadeComponent feedbackFade;

		public Text progressField;

		public GameObject nextButton;

		public GameObject restartButton;

		public GameObject backButton;

		public UINavigation restartButtonNav;

		public UINavigation nextButtonNav;

		public UINavigation backButtonNav;

		public RectTransform headerRect;

		public RectTransform standingsRect;

		public VideoPlayer videoPlayer;

		public VideoClip videoClip;

		public FadeComponent videoFade;

		[HideInInspector]
		public DRLTournamentRoundData round;

		[HideInInspector]
		public bool openedFromTheBrackets;

		[HideInInspector]
		public RaceController race;

		[HideInInspector]
		public UITournamentLeaderboardFeedbackType status;

		private string mLoadingWithDotsStr;

		private string mLoadingStr;

		[HideInInspector]
		public bool isReplayLoading;

		public bool loading => status == UITournamentLeaderboardFeedbackType.Loading;

		public float progress
		{
			set
			{
				progressField.text = ((value <= 0f) ? loadingWithDotsLocalized : (loadingLocalized + " " + Mathf.FloorToInt(Mathf.Clamp01(value) * 100f) + "%"));
			}
		}

		private string loadingWithDotsLocalized
		{
			get
			{
				if (string.IsNullOrEmpty(mLoadingWithDotsStr))
				{
					mLoadingWithDotsStr = base.app.model.storage.locale.Get("ui.common.loading-w-dots", "LOADING...");
				}
				return mLoadingWithDotsStr;
			}
		}

		private string loadingLocalized
		{
			get
			{
				if (string.IsNullOrEmpty(mLoadingStr))
				{
					mLoadingStr = base.app.model.storage.locale.Get("leaderboard.progress-loading", "LOADING <color=red>/</color>");
				}
				return mLoadingStr;
			}
		}

		public void ClearPages()
		{
			FadeComponent fade = racePageField.fade;
			if ((bool)fade)
			{
				fade.FadeOut(0.3f);
			}
			RunOnce(delegate
			{
				racePageField.listField.Clear();
			}, 0.35f);
		}

		public void Clear()
		{
			raceListField.Clear();
		}

		public void SetFeedback(UITournamentLeaderboardFeedbackType p_type, bool p_hide_list, float p_delay)
		{
			float feedback_alpha = ((p_type == UITournamentLeaderboardFeedbackType.None) ? (-0.1f) : 1f);
			float content_alpha = ((p_type == UITournamentLeaderboardFeedbackType.None) ? 1f : (p_hide_list ? (-0.1f) : 1f));
			FadeComponent content_fade = raceListFade;
			status = p_type;
			if (status == UITournamentLeaderboardFeedbackType.Loading)
			{
				progress = 0f;
			}
			Action action = delegate
			{
				feedbackFade.Fade(feedback_alpha, 0.3f, 0.05f, Cubic.Out);
				content_fade.Fade(content_alpha, 0.3f, 0f, Cubic.Out);
				if (p_type != UITournamentLeaderboardFeedbackType.None)
				{
					int num = (int)p_type;
					for (int i = 0; i < feedbacks.Count; i++)
					{
						feedbacks[i].SetActive(i == num);
					}
				}
			};
			if (p_delay <= 0f)
			{
				action();
			}
			else
			{
				RunOnce(p_delay, action);
			}
		}

		public void SetFeedback(UITournamentLeaderboardFeedbackType p_type, bool p_hide_list)
		{
			SetFeedback(p_type, p_hide_list, 0f);
		}

		public void SetFeedback(UITournamentLeaderboardFeedbackType p_type)
		{
			SetFeedback(p_type, p_hide_list: true, 0f);
		}

		public void SetCapturedBackground(RenderTexture p_capturedBackground)
		{
			if (!(p_capturedBackground == null))
			{
				base.app.view.ui.screens.SetStaticBackground(p_capturedBackground);
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
					float num2 = UnityEngine.Random.Range(0f, num - 1f);
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
