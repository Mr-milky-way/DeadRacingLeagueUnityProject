using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class DRLTournamentStandingsHeatItemContent : MonoBehaviour
	{
		public GameObject placementTime;

		public GameObject placementGoldenHeat;

		public GameObject advanceSuddenDeath;

		public GameObject advanceGoldenHeat;

		public GameObject userColorStripe;

		public GameObject secondPlaceWinnerHighlight;

		public GameObject outlineFirstPlace;

		public GameObject watchButton;

		public FadeComponent watchButtonFade;

		public Text placementRegularField;

		public Text timeField;

		public Text placementGoldenHeatField;

		public Image userColorImage;

		public Image outlineImage;

		public Image secondHighlightImage;

		public UINavigation watchButtonNavigation;

		private Color m_color
		{
			set
			{
				userColorImage.color = value;
				outlineImage.color = value;
				secondHighlightImage.color = value;
			}
		}

		public void Set(string p_rank, string p_time, Color p_color, bool p_highlightSecond, bool p_personalBest = false, bool p_overallBest = false)
		{
			if (!string.IsNullOrEmpty(p_rank) && !string.IsNullOrEmpty(p_time))
			{
				m_color = p_color;
				placementTime.SetActive(value: true);
				placementGoldenHeat.SetActive(value: false);
				advanceSuddenDeath.SetActive(value: false);
				advanceGoldenHeat.SetActive(value: false);
				outlineFirstPlace.SetActive(value: false);
				userColorStripe.SetActive(value: false);
				placementRegularField.text = p_rank;
				timeField.text = p_time;
				if (p_rank == "1ST")
				{
					outlineFirstPlace.SetActive(value: true);
				}
				secondPlaceWinnerHighlight.SetActive(p_highlightSecond);
			}
		}

		public void SetReplayContent(bool p_available = false)
		{
			Clear();
			watchButton.SetActive(value: true);
			watchButtonFade.Fade(p_available ? 1f : 0.25f, 0f);
			watchButtonNavigation = watchButton.GetComponent<UINavigation>();
		}

		public void ClearReplayContent()
		{
			watchButton.SetActive(value: false);
			watchButtonFade.Fade(0f, 0f);
		}

		public void Set(string p_rank, Color p_color)
		{
			if (!string.IsNullOrEmpty(p_rank))
			{
				m_color = p_color;
				placementTime.SetActive(value: false);
				placementGoldenHeat.SetActive(value: true);
				advanceSuddenDeath.SetActive(value: false);
				advanceGoldenHeat.SetActive(value: false);
				outlineFirstPlace.SetActive(value: false);
				userColorStripe.SetActive(value: false);
				placementGoldenHeatField.text = p_rank;
				if (p_rank == "1ST")
				{
					placementGoldenHeatField.fontStyle = FontStyle.Bold;
					outlineFirstPlace.SetActive(value: true);
				}
				else
				{
					placementGoldenHeatField.fontStyle = FontStyle.Normal;
				}
			}
		}

		public void SetAdvance(bool p_goldenHeat = false)
		{
			placementTime.SetActive(value: false);
			placementGoldenHeat.SetActive(value: false);
			advanceSuddenDeath.SetActive(!p_goldenHeat);
			advanceGoldenHeat.SetActive(p_goldenHeat);
		}

		public void SetUserColorStripe(Color p_color, bool p_active = true)
		{
			m_color = p_color;
			userColorStripe.SetActive(p_active);
		}

		public void Clear()
		{
			placementTime.SetActive(value: false);
			placementGoldenHeat.SetActive(value: false);
			advanceSuddenDeath.SetActive(value: false);
			advanceGoldenHeat.SetActive(value: false);
			outlineFirstPlace.SetActive(value: false);
			userColorStripe.SetActive(value: false);
			watchButton.SetActive(value: false);
			watchButtonFade.Fade(0.25f, 0f);
		}
	}
}
