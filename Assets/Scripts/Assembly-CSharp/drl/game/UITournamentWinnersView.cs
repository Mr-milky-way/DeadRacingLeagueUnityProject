using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UITournamentWinnersView : UIScreenView
	{
		public GameObject flexbox;

		public GameObject regular;

		public List<UILeaderboardItemView> regularList;

		public List<UILeaderboardItemView> flexboxItems;

		public FadeComponent listFade;

		public List<GameObject> feedbacks;

		public FadeComponent feedbackFade;

		public Text progressField;

		public DRLPagePickerView pageField;

		public Text title;

		public GameObject header;

		public RectTransform container;

		public RectTransform content;

		public GameObject nextButton;

		[NonSerialized]
		public int overridePage = -1;

		public UILeaderboardFeedbackType status;

		public bool allowNext { get; set; }

		public bool loading => status == UILeaderboardFeedbackType.Loading;

		public float progress
		{
			set
			{
				progressField.text = ((value <= 0f) ? "LOADING..." : ("LOADING <color=red>/</color> " + Mathf.FloorToInt(Mathf.Clamp01(value) * 100f) + "%"));
			}
		}

		public void Set(int p_index, DRLLeaderboardData p_data)
		{
		}

		public void SetFeedback(UILeaderboardFeedbackType p_type, bool p_hide_list, float p_delay)
		{
			if (!feedbackFade)
			{
				return;
			}
			float feedback_alpha = ((p_type == UILeaderboardFeedbackType.None) ? (-0.1f) : 1f);
			float content_alpha = ((p_type == UILeaderboardFeedbackType.None) ? 1f : (p_hide_list ? (-0.1f) : 1f));
			FadeComponent content_fade = listFade;
			status = p_type;
			if (status == UILeaderboardFeedbackType.Loading)
			{
				progress = 0f;
			}
			Action action = delegate
			{
				feedbackFade.Fade(feedback_alpha, 0.3f, 0.05f, Cubic.Out);
				content_fade.Fade(content_alpha, 0.3f, 0f, Cubic.Out);
				if (p_type != UILeaderboardFeedbackType.None)
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

		public void Clear()
		{
			foreach (UILeaderboardItemView flexboxItem in flexboxItems)
			{
				flexboxItem.Clear(0f, fadeOut: false);
			}
			foreach (UILeaderboardItemView regular in regularList)
			{
				regular.Clear(0f, fadeOut: false);
			}
		}

		public void SetFeedback(UILeaderboardFeedbackType p_type, bool p_hide_list)
		{
			SetFeedback(p_type, p_hide_list, 0f);
		}

		public void SetFeedback(UILeaderboardFeedbackType p_type)
		{
			SetFeedback(p_type, p_hide_list: true, 0f);
		}
	}
}
