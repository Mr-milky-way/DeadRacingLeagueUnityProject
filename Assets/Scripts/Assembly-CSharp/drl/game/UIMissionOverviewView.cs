using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIMissionOverviewView : UIScreenView
	{
		public DRLQuest quest;

		public DRLMission mission;

		public Text questNameField;

		public RawImage questBackgroundField;

		public UICardButtonMission missionCard;

		public UICardButtonLesson lessonCard;

		public Text descriptionField;

		public UICardButtonDroneRig droneCard;

		public RectTransform modeBeginner;

		public RectTransform modeIntermediate;

		public RectTransform modePro;

		public RectTransform modeStabilized;

		public RectTransform modeHorizon;

		public Vector2 modeSizeSmall = new Vector2(320f, 155f);

		public Vector2 modeSizeLarge = new Vector2(320f, 320f);

		public DRLDroneRig defaultDrone;

		[Range(0f, 1f)]
		public float inactiveFMFadeAmount = 0.15f;

		public UINavigation onboardingNavBtn;

		public UINavigation startNavBtn;

		public void Set(DRLQuest p_quest, DRLMission p_mission)
		{
			quest = p_quest;
			mission = p_mission;
			if (missionCard != null)
			{
				missionCard.Set(mission);
			}
			else
			{
				if (!(lessonCard != null))
				{
					return;
				}
				lessonCard.Set(p_mission, p_quest);
			}
			DRLMap map = mission.map;
			Texture texture = (map ? map.blur : null);
			questNameField.text = quest.title.ToUpper();
			questBackgroundField.texture = texture;
			questBackgroundField.enabled = texture;
			descriptionField.text = mission.description;
			DRLDroneRig drone = p_mission.GetDrone(0);
			if (!drone)
			{
				drone = defaultDrone;
			}
			droneCard.gameObject.SetActive(drone != null);
			droneCard.Set(drone);
			droneCard.GetComponent<UINavigation>().enabled = mission.drone.Count > 1;
			onboardingNavBtn.gameObject.SetActive(base.app.inOnboarding);
			startNavBtn.gameObject.SetActive(!base.app.inOnboarding);
			List<FCMode> flightModes = p_mission.flightModes;
			if (flightModes.Count > 0)
			{
				bool has_beginner = flightModes.Contains(FCMode.Beginner);
				bool has_intermediate = flightModes.Contains(FCMode.Intermediate);
				bool has_pro = flightModes.Contains(FCMode.Pro);
				bool has_stabilized = flightModes.Contains(FCMode.Stabilized);
				bool has_horizon = flightModes.Contains(FCMode.Horizon);
				ShowFlightMode(has_beginner, has_intermediate, has_pro, has_stabilized, has_horizon);
			}
			else
			{
				ShowFlightMode(has_beginner: true, has_intermediate: false, has_pro: false, has_stabilized: false, has_horizon: false);
			}
		}

		private void ShowFlightMode(bool has_beginner, bool has_intermediate, bool has_pro, bool has_stabilized, bool has_horizon)
		{
			modeBeginner.gameObject.SetActive(value: false);
			modeBeginner.GetComponent<FadeComponent>().Fade(inactiveFMFadeAmount, 0f);
			modeIntermediate.gameObject.SetActive(value: false);
			modeIntermediate.GetComponent<FadeComponent>().Fade(inactiveFMFadeAmount, 0f);
			modePro.gameObject.SetActive(value: false);
			modePro.GetComponent<FadeComponent>().Fade(inactiveFMFadeAmount, 0f);
			modeStabilized.gameObject.SetActive(value: false);
			modeStabilized.GetComponent<FadeComponent>().Fade(inactiveFMFadeAmount, 0f);
			modeHorizon.gameObject.SetActive(value: false);
			modeHorizon.GetComponent<FadeComponent>().Fade(inactiveFMFadeAmount, 0f);
			if (has_beginner)
			{
				modeBeginner.sizeDelta = modeSizeLarge;
				modeBeginner.gameObject.SetActive(value: true);
				modeBeginner.GetComponent<FadeComponent>().FadeIn(0f);
			}
			else if (has_intermediate && !has_pro)
			{
				modeIntermediate.sizeDelta = modeSizeLarge;
				modeIntermediate.gameObject.SetActive(value: true);
				modeIntermediate.GetComponent<FadeComponent>().FadeIn(0f);
			}
			else if (has_pro && !has_intermediate)
			{
				modePro.sizeDelta = modeSizeLarge;
				modePro.gameObject.SetActive(value: true);
				modePro.GetComponent<FadeComponent>().FadeIn(0f);
			}
			else if (has_pro && has_intermediate)
			{
				modePro.sizeDelta = modeSizeSmall;
				modeIntermediate.sizeDelta = modeSizeSmall;
				modePro.gameObject.SetActive(value: true);
				modeIntermediate.gameObject.SetActive(value: true);
				if (base.app.model.storage.state.player.activeFCModeMissions == FCMode.Pro)
				{
					modePro.GetComponent<FadeComponent>().FadeIn(0f);
					modeIntermediate.GetComponent<FadeComponent>().Fade(inactiveFMFadeAmount, 0f);
				}
				else
				{
					modeIntermediate.GetComponent<FadeComponent>().FadeIn(0f);
					modePro.GetComponent<FadeComponent>().Fade(inactiveFMFadeAmount, 0f);
				}
			}
			else if (has_stabilized)
			{
				modeStabilized.sizeDelta = modeSizeLarge;
				modeStabilized.gameObject.SetActive(value: true);
				modeStabilized.GetComponent<FadeComponent>().FadeIn(0f);
			}
			else if (has_horizon)
			{
				modeHorizon.sizeDelta = modeSizeLarge;
				modeHorizon.gameObject.SetActive(value: true);
				modeHorizon.GetComponent<FadeComponent>().FadeIn(0f);
			}
		}
	}
}
