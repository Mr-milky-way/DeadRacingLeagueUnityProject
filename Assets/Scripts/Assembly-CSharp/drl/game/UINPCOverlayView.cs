using UnityEngine;
using UnityEngine.UI;
using drl.sim;

namespace drl.game
{
	public class UINPCOverlayView : UIScreenView
	{
		public UINPCOverlay npcOverlay;

		public Text descriptionField;

		public RawImage descriptionImage;

		public Text questTitle;

		public Text missionTitle;

		public GameObject backButton;

		public GameObject exitButton;

		public string description
		{
			set
			{
				descriptionField.text = value;
			}
		}

		public string missionText
		{
			set
			{
				missionTitle.text = value;
			}
		}

		public string questText
		{
			set
			{
				questTitle.text = value;
			}
		}

		public Texture image
		{
			set
			{
				descriptionImage.texture = value;
				descriptionImage.gameObject.SetActive(value != null);
			}
		}

		public void SetState(NPCStateType p_type, bool p_is_left, ControllerStateType p_controller)
		{
			npcOverlay.controller = p_controller;
			npcOverlay.SetState(p_type, p_is_left);
		}
	}
}
