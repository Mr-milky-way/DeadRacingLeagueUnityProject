using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class DRLTournamentStandingsHeatHeaderItem : MonoBehaviour
	{
		public Text heatField;

		public GameObject heatTitle;

		public Image sdIconImage;

		public Image ghIconImage;

		public GameObject sdIcon;

		public GameObject ghIcon;

		public Color inactiveHeatColor;

		public Color activeHeatColor;

		public void SetHeatTitle(string p_title)
		{
			heatTitle.SetActive(value: true);
			sdIcon.SetActive(value: false);
			ghIcon.SetActive(value: false);
			heatField.text = p_title;
		}

		public void SetHeatSD()
		{
			heatTitle.SetActive(value: false);
			sdIcon.SetActive(value: true);
			ghIcon.SetActive(value: false);
		}

		public void SetHeatGH()
		{
			heatTitle.SetActive(value: false);
			sdIcon.SetActive(value: false);
			ghIcon.SetActive(value: true);
		}

		public void SetHeatActive(bool p_active)
		{
			heatField.color = (p_active ? activeHeatColor : inactiveHeatColor);
			sdIconImage.color = (p_active ? activeHeatColor : inactiveHeatColor);
			ghIconImage.color = (p_active ? activeHeatColor : inactiveHeatColor);
		}
	}
}
