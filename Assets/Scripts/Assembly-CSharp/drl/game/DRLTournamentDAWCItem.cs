using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class DRLTournamentDAWCItem : MonoBehaviour
	{
		public GameObject activeLayout;

		public GameObject inactiveLayout;

		public Text rankField;

		public Text usernameField;

		public Image rankImage;

		public Image userColor;

		public UITruncateText textTruncate;

		public void Set(DRLPlacementsUserData p_data)
		{
			if (string.IsNullOrEmpty(p_data.username))
			{
				inactiveLayout.SetActive(value: true);
				activeLayout.SetActive(value: false);
				return;
			}
			rankField.text = p_data.position.ToString();
			usernameField.text = p_data.username.ToUpper();
			rankImage.color = p_data.profileColor;
			userColor.color = p_data.profileColor;
			inactiveLayout.SetActive(value: false);
			activeLayout.SetActive(value: true);
			this.TimerRunOnce(delegate
			{
				textTruncate.enabled = true;
			}, 0.05f);
		}

		public void Clear()
		{
			rankField.text = "";
			usernameField.text = "";
			rankImage.color = Color.white;
			userColor.color = Color.white;
			inactiveLayout.SetActive(value: true);
			activeLayout.SetActive(value: false);
		}
	}
}
