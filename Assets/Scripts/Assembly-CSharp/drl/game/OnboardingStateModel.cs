using System.Collections.Generic;
using System.Linq;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class OnboardingStateModel : Model<DRLApp>
	{
		private List<DRLOnboardingProgressionData> m_onboardingProgress;

		public PlayerStateModel parent => AssertParent<PlayerStateModel>("parent");

		public DataFlow data => parent.data;

		public bool skipOnboarding
		{
			get
			{
				return data.Get("onboarding-started", d: false);
			}
			set
			{
				data.Set("onboarding-started", value);
				Refresh();
			}
		}

		public bool finishedOrientation
		{
			get
			{
				return data.Get("onboarding-orientation", d: false);
			}
			set
			{
				data.Set("onboarding-orientation", value);
				Refresh();
			}
		}

		public int beginnerProgress
		{
			get
			{
				return data.Get("onboarding-progress-beginner", 0);
			}
			set
			{
				data.Set("onboarding-progress-beginner", value);
				Refresh();
			}
		}

		public int intermediateProgress
		{
			get
			{
				return data.Get("onboarding-progress-intermediate", 0);
			}
			set
			{
				data.Set("onboarding-progress-intermediate", value);
				Refresh();
			}
		}

		public int proProgress
		{
			get
			{
				return data.Get("onboarding-progress-pro", 0);
			}
			set
			{
				data.Set("onboarding-progress-pro", value);
				Refresh();
			}
		}

		public int proMissionsProgress
		{
			get
			{
				return data.Get("onboarding-progress-proMissions", 0);
			}
			set
			{
				data.Set("onboarding-progress-proMissions", value);
				Refresh();
			}
		}

		public bool trackStatus
		{
			get
			{
				return data.Get("track-completed", d: false);
			}
			set
			{
				data.Set("track-completed", value);
			}
		}

		public string missionName
		{
			get
			{
				return data.Get<string>("mission-name");
			}
			set
			{
				data.Set("mission-name", value);
			}
		}

		public string mapGUID
		{
			get
			{
				return data.Get<string>("map-GUID");
			}
			set
			{
				data.Set("map-GUID", value);
			}
		}

		public string trackGUID
		{
			get
			{
				return data.Get<string>("track-GUID");
			}
			set
			{
				data.Set("track-GUID", value);
			}
		}

		public DRLOnboardingProgressionData[] beginnerStepsCompleted
		{
			get
			{
				return Serialize.FromJson<DRLOnboardingProgressionData[]>(onboardingBeginnerProgressData);
			}
			set
			{
				string v = Serialize.ToJson(value);
				data.Set("onboarding-progress-steps-beginner", v);
				data.Set("onboarding-beginner-progressData", v);
				Refresh();
			}
		}

		public DRLOnboardingProgressionData[] intermediateStepsCompleted
		{
			get
			{
				return Serialize.FromJson<DRLOnboardingProgressionData[]>(onboardingIntermediateProgressData);
			}
			set
			{
				string v = Serialize.ToJson(value);
				data.Set("onboarding-progress-steps-intermediate", v);
				data.Set("onboarding-intermediate-progressData", v);
				Refresh();
			}
		}

		public DRLOnboardingProgressionData[] proStepsCompleted
		{
			get
			{
				return Serialize.FromJson<DRLOnboardingProgressionData[]>(onboardingProProgressData);
			}
			set
			{
				string v = Serialize.ToJson(value);
				data.Set("onboarding-progress-steps-pro", v);
				data.Set("onboarding-pro-progressData", v);
				Refresh();
			}
		}

		public bool clickedProMissions
		{
			get
			{
				return data.Get("onboarding-clicked-mission", d: false);
			}
			set
			{
				data.Set("onboarding-clicked-mission", value);
				Refresh();
			}
		}

		public DRLOnboardingProgressionData activeProgress { get; set; }

		public List<DRLOnboardingProgressionData> progress
		{
			get
			{
				if (m_onboardingProgress == null)
				{
					m_onboardingProgress = new List<DRLOnboardingProgressionData>();
				}
				switch (base.app.model.onboarding.activeOnboarding.mode)
				{
				case OnboardingCampaignMode.Beginner:
					onboardingProgressData = onboardingBeginnerProgressData;
					break;
				case OnboardingCampaignMode.Intermediate:
					onboardingProgressData = onboardingIntermediateProgressData;
					break;
				case OnboardingCampaignMode.Pro:
					onboardingProgressData = onboardingProProgressData;
					break;
				}
				DRLOnboardingProgressionData[] array = Serialize.FromJson<DRLOnboardingProgressionData[]>(onboardingProgressData);
				if (array != null)
				{
					m_onboardingProgress.Clear();
					m_onboardingProgress = array.ToList();
				}
				return m_onboardingProgress;
			}
			set
			{
				m_onboardingProgress = value;
				if (m_onboardingProgress != null)
				{
					string text = (onboardingProgressData = Serialize.ToJson(m_onboardingProgress.ToArray()));
					switch (base.app.model.onboarding.activeOnboarding.mode)
					{
					case OnboardingCampaignMode.Beginner:
						onboardingBeginnerProgressData = text;
						break;
					case OnboardingCampaignMode.Intermediate:
						onboardingIntermediateProgressData = text;
						break;
					case OnboardingCampaignMode.Pro:
						onboardingProProgressData = text;
						break;
					}
				}
			}
		}

		public string onboardingProgressData
		{
			get
			{
				return data.Get("onboarding-progressData", "");
			}
			set
			{
				data.Set("onboarding-progressData", value);
				Refresh();
			}
		}

		public string onboardingBeginnerProgressData
		{
			get
			{
				return data.Get("onboarding-beginner-progressData", "");
			}
			set
			{
				data.Set("onboarding-beginner-progressData", value);
				Refresh();
			}
		}

		public string onboardingIntermediateProgressData
		{
			get
			{
				return data.Get("onboarding-intermediate-progressData", "");
			}
			set
			{
				data.Set("onboarding-intermediate-progressData", value);
				Refresh();
			}
		}

		public string onboardingProProgressData
		{
			get
			{
				return data.Get("onboarding-pro-progressData", "");
			}
			set
			{
				data.Set("onboarding-pro-progressData", value);
				Refresh();
			}
		}

		public FCProfileData.Betaflight.Preset RCSensivity
		{
			get
			{
				return data.Get("onboarding-sensitivity", FCProfileData.Betaflight.HighPresets[RCI.GetControllerStateType(ControllerStateType.XBox)]);
			}
			set
			{
				data.Set("onboarding-sensitivity", value);
				Refresh();
			}
		}

		public void SetOnboardingProgress(int p_step, bool p_complete)
		{
			List<DRLOnboardingProgressionData> list = new List<DRLOnboardingProgressionData>();
			List<DRLOnboardingProgressionData> list2 = progress;
			DRLOnboardingProgressionData dRLOnboardingProgressionData = null;
			list.Add(list2[p_step]);
			dRLOnboardingProgressionData = list[p_step];
			if (dRLOnboardingProgressionData.missionName != null && list2[p_step].missionName != null && list2[p_step].missionName == dRLOnboardingProgressionData.missionName)
			{
				list[p_step].trackStatus = p_complete;
				list[p_step].mapGUID = dRLOnboardingProgressionData.mapGUID;
				list[p_step].trackGUID = dRLOnboardingProgressionData.trackGUID;
				list[p_step].missionName = dRLOnboardingProgressionData.missionName;
			}
			for (int i = 0; i < list2.Count; i++)
			{
				if (list2[i].mapGUID == dRLOnboardingProgressionData.mapGUID && list2[i].trackGUID == dRLOnboardingProgressionData.trackGUID)
				{
					list[i].trackStatus = p_complete;
					list[i].mapGUID = dRLOnboardingProgressionData.mapGUID;
					list[i].trackGUID = dRLOnboardingProgressionData.trackGUID;
				}
				else
				{
					list[i].trackStatus = p_complete;
				}
			}
			switch (base.app.model.onboarding.activeOnboarding.mode)
			{
			case OnboardingCampaignMode.Beginner:
				onboardingBeginnerProgressData = Serialize.ToJson(list.ToArray());
				beginnerStepsCompleted = list.ToArray();
				break;
			case OnboardingCampaignMode.Intermediate:
				onboardingIntermediateProgressData = Serialize.ToJson(list.ToArray());
				intermediateStepsCompleted = list.ToArray();
				break;
			case OnboardingCampaignMode.Pro:
				onboardingProProgressData = Serialize.ToJson(list.ToArray());
				proStepsCompleted = list.ToArray();
				break;
			}
		}

		public void Refresh()
		{
			if ((bool)parent)
			{
				parent.Refresh();
			}
		}
	}
}
