using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class UIGameViewerInformationLayer : MonoBehaviour
	{
		public FadeComponent fade;

		public GameObject timeContainer;

		public Text timeMinField;

		public Text timeSecField;

		public Text timeMsField;

		public GameObject userContainer;

		public Image userBackground;

		public RawImage userPhoto;

		public Text userNameField;

		public GameObject layoutsContainer;

		public Transform droneLayout;

		public List<Image> droneLayoutMotors;

		public UIControllerOverlay controller;

		public float time
		{
			set
			{
				string[] array = Format.SecondsToTime(value, 2, p_use_ms: true).Split(':');
				timeMinField.text = array[0];
				timeSecField.text = array[1];
				timeMsField.text = array[2];
			}
		}

		public float[] rpm
		{
			set
			{
				float[] array = ((value == null) ? new float[0] : value);
				List<Image> list = droneLayoutMotors;
				int num = Mathf.Min(list.Count, array.Length);
				for (int i = 0; i < list.Count; i++)
				{
					list[i].fillAmount = 0f;
				}
				for (int j = 0; j < num; j++)
				{
					list[j].fillAmount = array[j];
				}
			}
		}

		public void SetRaceStatsVisible(bool p_flag)
		{
			if ((bool)timeContainer)
			{
				timeContainer.SetActive(p_flag);
			}
		}

		public void SetMotorsVisible(bool p_flag)
		{
			if ((bool)layoutsContainer)
			{
				layoutsContainer.SetActive(p_flag);
			}
		}

		public void SetUser(string p_name, Texture2D p_photo, Color p_color)
		{
			userNameField.text = p_name;
			userBackground.color = p_color;
			userPhoto.enabled = p_photo != null;
			userPhoto.texture = p_photo;
		}

		public void SetUser(GamePlayerData p_data)
		{
			if (p_data != null)
			{
				SetUser(p_data.name.ToUpper(), p_data.photo, p_data.color);
			}
		}

		public void SetUserVisible(bool p_flag)
		{
			if ((bool)userContainer)
			{
				userContainer.SetActive(p_flag);
			}
		}
	}
}
