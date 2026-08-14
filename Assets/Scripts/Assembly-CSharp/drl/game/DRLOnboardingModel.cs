using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLOnboardingModel : Model<DRLApp>
	{
		private FCProfileData playerProfile;

		private int m_step;

		public bool hasFailed;

		public bool loading;

		public bool firstStart;

		public Texture2D botAvatar;

		public OnboardingRaceReplayData[] replayData;

		private DRLOnboarding m_onboarding;

		private DRLOnboardingProgressionData _drlOnboardingProgressionData;

		public bool inProgress => m_onboarding != null;

		public bool skipOnboarding
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.skipOnboarding;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.skipOnboarding = value;
			}
		}

		public bool hasFinishedOrientation
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.finishedOrientation;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.finishedOrientation = value;
			}
		}

		public int step
		{
			get
			{
				return m_step;
			}
			private set
			{
				m_step = value;
			}
		}

		public int currentStep
		{
			get
			{
				return m_step = Mathf.Clamp(m_step, 0, m_onboarding.steps.Count - 1);
			}
			set
			{
				m_step = value;
				Mathf.Clamp(currentStep, 0, m_onboarding.steps.Count - 1);
			}
		}

		public bool hasProgress
		{
			get
			{
				if (!inProgress)
				{
					return false;
				}
				return m_onboarding.mode switch
				{
					OnboardingCampaignMode.Beginner => beginnerProgress > 0, 
					OnboardingCampaignMode.Intermediate => intermediateProgress > 0, 
					OnboardingCampaignMode.Pro => proProgress > 0, 
					_ => false, 
				};
			}
		}

		public DRLOnboarding activeOnboarding
		{
			get
			{
				return m_onboarding;
			}
			set
			{
				m_onboarding = value;
			}
		}

		public DRLOnboarding beginnerOnboarding => base.app.model.storage.GetOnboardingCampaigns(OnboardingCampaignMode.Beginner);

		public DRLOnboarding intermediateOnboarding => base.app.model.storage.GetOnboardingCampaigns(OnboardingCampaignMode.Intermediate);

		public DRLOnboarding proOnboarding => base.app.model.storage.GetOnboardingCampaigns(OnboardingCampaignMode.Pro);

		public int beginnerProgress
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.beginnerProgress;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.beginnerProgress = value;
			}
		}

		public int intermediateProgress
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.intermediateProgress;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.intermediateProgress = value;
			}
		}

		public int proProgress
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.proProgress;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.proProgress = value;
			}
		}

		public int proMissionsProgress
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.proMissionsProgress;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.proMissionsProgress = value;
			}
		}

		public DRLOnboardingProgressionData[] beginnerStepComplete
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.beginnerStepsCompleted;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.beginnerStepsCompleted = value;
			}
		}

		public DRLOnboardingProgressionData[] intermediateStepComplete
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.intermediateStepsCompleted;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.intermediateStepsCompleted = value;
			}
		}

		public DRLOnboardingProgressionData[] proStepComplete
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.proStepsCompleted;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.proStepsCompleted = value;
			}
		}

		public bool trackStatus
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.trackStatus;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.trackStatus = value;
			}
		}

		public string missionName
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.missionName;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.missionName = value;
			}
		}

		public string mapGUID
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.mapGUID;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.mapGUID = value;
			}
		}

		public string trackGUID
		{
			get
			{
				return base.app.model.storage.state.player.onboarding.trackGUID;
			}
			set
			{
				base.app.model.storage.state.player.onboarding.trackGUID = value;
			}
		}

		public int GetProgress()
		{
			if (!inProgress)
			{
				return 0;
			}
			int num = 0;
			int num2 = 0;
			OnboardingStateModel onboarding = base.app.model.storage.state.player.onboarding;
			List<DRLOnboardingProgressionData> progress = onboarding.progress;
			if (progress == null || progress.Count < activeOnboarding.steps.Count)
			{
				List<DRLOnboardingProgressionData> list = new List<DRLOnboardingProgressionData>();
				for (int i = 0; i < activeOnboarding.steps.Count; i++)
				{
					OnboardingStep onboardingStep = activeOnboarding.steps[i];
					if (onboardingStep.mission != null)
					{
						DRLOnboardingProgressionData item = new DRLOnboardingProgressionData
						{
							missionName = onboardingStep.mission.title,
							trackGUID = onboardingStep.trackGuid,
							mapGUID = onboardingStep.mapGuid,
							trackStatus = onboardingStep.completed
						};
						list.Add(item);
					}
					else
					{
						DRLOnboardingProgressionData item2 = new DRLOnboardingProgressionData
						{
							trackGUID = onboardingStep.trackGuid,
							mapGUID = onboardingStep.mapGuid,
							trackStatus = onboardingStep.completed,
							missionName = ""
						};
						list.Add(item2);
					}
					onboarding.progress = list;
				}
				switch (activeOnboarding.mode)
				{
				case OnboardingCampaignMode.Beginner:
					beginnerStepComplete = list.ToArray();
					break;
				case OnboardingCampaignMode.Intermediate:
					intermediateStepComplete = list.ToArray();
					break;
				case OnboardingCampaignMode.Pro:
					proStepComplete = list.ToArray();
					break;
				}
			}
			switch (activeOnboarding.mode)
			{
			case OnboardingCampaignMode.Beginner:
				if (beginnerStepComplete != null)
				{
					int num4 = beginnerStepComplete.Length;
					for (int num5 = 0; num5 < num4; num5++)
					{
						if (beginnerStepComplete[num5].trackStatus)
						{
							num++;
						}
					}
				}
				num2 += beginnerOnboarding.steps.Count((OnboardingStep variable) => variable.completed);
				break;
			case OnboardingCampaignMode.Intermediate:
				if (intermediateStepComplete != null)
				{
					int num6 = intermediateStepComplete.Length;
					for (int num7 = 0; num7 < num6; num7++)
					{
						if (intermediateStepComplete[num7].trackStatus)
						{
							num++;
						}
					}
				}
				num2 += intermediateOnboarding.steps.Count((OnboardingStep variable) => variable.completed);
				break;
			case OnboardingCampaignMode.Pro:
				if (proStepComplete != null)
				{
					int num3 = proStepComplete.Length;
					for (int j = 0; j < num3; j++)
					{
						if (proStepComplete[j].trackStatus)
						{
							num++;
						}
					}
				}
				num2 += proOnboarding.steps.Count((OnboardingStep variable) => variable.completed);
				break;
			}
			if (num2 <= num)
			{
				return num;
			}
			return num2;
		}

		public void SetProgress(int p_progress)
		{
			if (inProgress)
			{
				p_progress = Mathf.Clamp(p_progress, 0, m_onboarding.steps.Count);
				switch (m_onboarding.mode)
				{
				case OnboardingCampaignMode.Beginner:
					beginnerProgress = p_progress;
					break;
				case OnboardingCampaignMode.Intermediate:
					intermediateProgress = p_progress;
					break;
				case OnboardingCampaignMode.Pro:
					proProgress = p_progress;
					break;
				}
			}
		}

		public void SetStepComplete(OnboardingCampaignMode p_mode, int p_step)
		{
			OnboardingStateModel onboarding = base.app.model.storage.state.player.onboarding;
			List<DRLOnboardingProgressionData> progress = onboarding.progress;
			new List<DRLOnboardingProgressionData>();
			if (p_step > activeOnboarding.steps.Count || p_step < 0)
			{
				Debug.LogError("Step index out of range: " + p_step);
				return;
			}
			activeOnboarding.steps[p_step].completed = true;
			onboarding.progress[p_step].trackStatus = true;
			trackStatus = true;
			switch (p_mode)
			{
			case OnboardingCampaignMode.Beginner:
			{
				if (p_step >= 0 && p_step < progress.Count)
				{
					progress[p_step].trackStatus = true;
				}
				if (p_step >= 0 && p_step < m_onboarding.steps.Count)
				{
					m_onboarding.steps[p_step].completed = true;
				}
				DRLOnboardingProgressionData[] beginnerStepsCompleted = onboarding.beginnerStepsCompleted;
				beginnerStepsCompleted[p_step].trackStatus = true;
				onboarding.beginnerStepsCompleted = beginnerStepsCompleted;
				beginnerOnboarding.steps[p_step].completed = true;
				break;
			}
			case OnboardingCampaignMode.Intermediate:
			{
				if (p_step >= 0 && p_step < progress.Count)
				{
					progress[p_step].trackStatus = true;
				}
				if (p_step >= 0 && p_step < m_onboarding.steps.Count)
				{
					m_onboarding.steps[p_step].completed = true;
				}
				DRLOnboardingProgressionData[] intermediateStepsCompleted = onboarding.intermediateStepsCompleted;
				intermediateStepsCompleted[p_step].trackStatus = true;
				onboarding.intermediateStepsCompleted = intermediateStepsCompleted;
				intermediateOnboarding.steps[p_step].completed = true;
				break;
			}
			case OnboardingCampaignMode.Pro:
			{
				if (p_step >= 0 && p_step < progress.Count)
				{
					progress[p_step].trackStatus = true;
				}
				if (p_step >= 0 && p_step < m_onboarding.steps.Count)
				{
					m_onboarding.steps[p_step].completed = true;
				}
				DRLOnboardingProgressionData[] proStepsCompleted = onboarding.proStepsCompleted;
				proStepsCompleted[p_step].trackStatus = true;
				onboarding.proStepsCompleted = proStepsCompleted;
				proOnboarding.steps[p_step].completed = true;
				break;
			}
			}
			GetProgress();
		}

		public int GetProgress(OnboardingCampaignMode p_onboarding)
		{
			int num = 0;
			int num2 = 0;
			switch (p_onboarding)
			{
			case OnboardingCampaignMode.Beginner:
				foreach (OnboardingStep step in beginnerOnboarding.steps)
				{
					if (step.completed)
					{
						num++;
					}
				}
				num2 += beginnerOnboarding.steps.Count((OnboardingStep variable) => variable.completed);
				break;
			case OnboardingCampaignMode.Intermediate:
				foreach (OnboardingStep step2 in intermediateOnboarding.steps)
				{
					if (step2.completed)
					{
						num++;
					}
				}
				num2 += intermediateOnboarding.steps.Count((OnboardingStep variable) => variable.completed);
				break;
			case OnboardingCampaignMode.Pro:
				foreach (OnboardingStep step3 in proOnboarding.steps)
				{
					if (step3.completed)
					{
						num++;
					}
				}
				num2 += proOnboarding.steps.Count((OnboardingStep variable) => variable.completed);
				break;
			}
			if (num2 <= num)
			{
				return num;
			}
			return num2;
		}

		public int GetMissionsProgress(OnboardingCampaignMode p_onboarding)
		{
			int num = 0;
			int num2 = 0;
			switch (p_onboarding)
			{
			case OnboardingCampaignMode.Beginner:
				foreach (OnboardingStep step in beginnerOnboarding.steps)
				{
					if (step.completed && step.type == OnboardingStep.OnboardingStepType.Mission)
					{
						num++;
					}
				}
				num2 += beginnerOnboarding.steps.Count((OnboardingStep variable) => variable.completed && variable.type == OnboardingStep.OnboardingStepType.Mission);
				break;
			case OnboardingCampaignMode.Intermediate:
				foreach (OnboardingStep step2 in intermediateOnboarding.steps)
				{
					if (step2.completed && step2.type == OnboardingStep.OnboardingStepType.Mission)
					{
						num++;
					}
				}
				num2 += intermediateOnboarding.steps.Count((OnboardingStep variable) => variable.completed && variable.type == OnboardingStep.OnboardingStepType.Mission);
				break;
			case OnboardingCampaignMode.Pro:
				foreach (OnboardingStep step3 in proOnboarding.steps)
				{
					if (step3.completed && step3.type == OnboardingStep.OnboardingStepType.Mission)
					{
						num++;
					}
				}
				num2 += proOnboarding.steps.Count((OnboardingStep variable) => variable.completed && variable.type == OnboardingStep.OnboardingStepType.Mission);
				break;
			}
			if (num2 <= num)
			{
				return num;
			}
			return num2;
		}

		public int GetRaceProgress(OnboardingCampaignMode p_onboarding)
		{
			int num = 0;
			OnboardingStateModel csm = base.app.model.storage.state.player.onboarding;
			switch (p_onboarding)
			{
			case OnboardingCampaignMode.Beginner:
				num += beginnerOnboarding.steps.Where((OnboardingStep t, int index) => (t.completed || csm.beginnerStepsCompleted[index].trackStatus) && t.type == OnboardingStep.OnboardingStepType.Race).Count();
				break;
			case OnboardingCampaignMode.Intermediate:
				num += intermediateOnboarding.steps.Where((OnboardingStep t, int index) => (t.completed || csm.intermediateStepsCompleted[index].trackStatus) && t.type == OnboardingStep.OnboardingStepType.Race).Count();
				break;
			case OnboardingCampaignMode.Pro:
				num += proOnboarding.steps.Where((OnboardingStep t, int index) => (t.completed || csm.proStepsCompleted[index].trackStatus) && t.type == OnboardingStep.OnboardingStepType.Race).Count();
				break;
			}
			return num;
		}

		public bool IsStepComplete(int m_currentStep)
		{
			OnboardingStateModel onboarding = base.app.model.storage.state.player.onboarding;
			switch (activeOnboarding.mode)
			{
			case OnboardingCampaignMode.Beginner:
				if (m_currentStep >= 0 && m_currentStep < m_onboarding.steps.Count)
				{
					if (!m_onboarding.steps[m_currentStep].completed)
					{
						return onboarding.beginnerStepsCompleted[m_currentStep].trackStatus;
					}
					return true;
				}
				break;
			case OnboardingCampaignMode.Intermediate:
				if (m_currentStep >= 0 && m_currentStep < m_onboarding.steps.Count)
				{
					if (!m_onboarding.steps[m_currentStep].completed)
					{
						return onboarding.intermediateStepsCompleted[m_currentStep].trackStatus;
					}
					return true;
				}
				break;
			case OnboardingCampaignMode.Pro:
				if (m_currentStep >= 0 && m_currentStep < m_onboarding.steps.Count)
				{
					if (!m_onboarding.steps[m_currentStep].completed)
					{
						return onboarding.proStepsCompleted[m_currentStep].trackStatus;
					}
					return true;
				}
				break;
			}
			return false;
		}

		public void RefreshCompletedSteps()
		{
			if (beginnerOnboarding == null || intermediateOnboarding == null || proOnboarding == null)
			{
				return;
			}
			OnboardingStateModel onboarding = base.app.model.storage.state.player.onboarding;
			if (beginnerStepComplete == null || beginnerStepComplete.Length < beginnerOnboarding.steps.Count || beginnerStepComplete[0] == null)
			{
				DRLOnboardingProgressionData[] array = new DRLOnboardingProgressionData[beginnerOnboarding.steps.Count];
				for (int i = 0; i < beginnerOnboarding.steps.Count; i++)
				{
					array[i] = new DRLOnboardingProgressionData
					{
						trackStatus = false,
						missionName = ((beginnerOnboarding.steps[i].mission == null) ? "" : beginnerOnboarding.steps[i].mission.title),
						trackGUID = beginnerOnboarding.steps[i].trackGuid,
						mapGUID = beginnerOnboarding.steps[i].mapGuid
					};
				}
				beginnerStepComplete = array;
				activeOnboarding = beginnerOnboarding;
				onboarding.onboardingBeginnerProgressData = Serialize.ToJson(array.ToArray());
			}
			if (intermediateStepComplete == null || intermediateStepComplete.Length < intermediateOnboarding.steps.Count || intermediateStepComplete[0] == null)
			{
				DRLOnboardingProgressionData[] array2 = new DRLOnboardingProgressionData[intermediateOnboarding.steps.Count];
				for (int j = 0; j < intermediateOnboarding.steps.Count; j++)
				{
					array2[j] = new DRLOnboardingProgressionData
					{
						trackStatus = false,
						missionName = ((intermediateOnboarding.steps[j].mission == null) ? "" : intermediateOnboarding.steps[j].mission.title),
						trackGUID = intermediateOnboarding.steps[j].trackGuid,
						mapGUID = intermediateOnboarding.steps[j].mapGuid
					};
				}
				intermediateStepComplete = array2;
				activeOnboarding = intermediateOnboarding;
				onboarding.onboardingIntermediateProgressData = Serialize.ToJson(array2.ToArray());
			}
			if (proStepComplete == null || proStepComplete.Length < proOnboarding.steps.Count || proStepComplete[0] == null)
			{
				DRLOnboardingProgressionData[] array3 = new DRLOnboardingProgressionData[proOnboarding.steps.Count];
				for (int k = 0; k < proOnboarding.steps.Count; k++)
				{
					array3[k] = new DRLOnboardingProgressionData
					{
						trackStatus = false,
						missionName = ((proOnboarding.steps[k].mission == null) ? "" : proOnboarding.steps[k].mission.title),
						trackGUID = proOnboarding.steps[k].trackGuid,
						mapGUID = proOnboarding.steps[k].mapGuid
					};
				}
				proStepComplete = array3;
				activeOnboarding = proOnboarding;
				onboarding.onboardingProProgressData = Serialize.ToJson(array3.ToArray());
			}
			onboarding = base.app.model.storage.state.player.onboarding;
			activeOnboarding = beginnerOnboarding;
			List<DRLOnboardingProgressionData> progress = onboarding.progress;
			for (int l = 0; l < beginnerStepComplete.Length; l++)
			{
				beginnerStepComplete[l].trackStatus = progress[l].trackStatus;
				if (!beginnerOnboarding.steps[l].completed)
				{
					beginnerOnboarding.steps[l].completed = progress[l].trackStatus;
				}
			}
			activeOnboarding = intermediateOnboarding;
			progress = onboarding.progress;
			for (int m = 0; m < intermediateStepComplete.Length; m++)
			{
				intermediateStepComplete[m].trackStatus = progress[m].trackStatus;
				if (!intermediateOnboarding.steps[m].completed)
				{
					intermediateOnboarding.steps[m].completed = progress[m].trackStatus;
				}
			}
			activeOnboarding = proOnboarding;
			progress = onboarding.progress;
			for (int n = 0; n < proStepComplete.Length; n++)
			{
				proStepComplete[n].trackStatus = progress[n].trackStatus;
				if (!proOnboarding.steps[n].completed)
				{
					proOnboarding.steps[n].completed = progress[n].trackStatus;
				}
			}
			beginnerProgress = onboarding.beginnerProgress;
			intermediateProgress = onboarding.intermediateProgress;
			proProgress = onboarding.proProgress;
		}

		public void ResetProgress(OnboardingCampaignMode p_onboarding)
		{
			OnboardingStateModel onboarding = base.app.model.storage.state.player.onboarding;
			switch (p_onboarding)
			{
			case OnboardingCampaignMode.Beginner:
			{
				foreach (OnboardingStep step in beginnerOnboarding.steps)
				{
					step.completed = false;
				}
				beginnerStepComplete = new DRLOnboardingProgressionData[beginnerOnboarding.steps.Count];
				for (int j = 0; j < beginnerOnboarding.steps.Count; j++)
				{
					_drlOnboardingProgressionData = new DRLOnboardingProgressionData
					{
						trackStatus = false,
						trackGUID = "",
						mapGUID = ""
					};
					beginnerStepComplete[j] = _drlOnboardingProgressionData;
				}
				foreach (OnboardingStep step2 in beginnerOnboarding.steps)
				{
					step2.completed = false;
				}
				onboarding.onboardingBeginnerProgressData = Serialize.ToJson(beginnerStepComplete.ToArray());
				break;
			}
			case OnboardingCampaignMode.Intermediate:
			{
				foreach (OnboardingStep step3 in intermediateOnboarding.steps)
				{
					step3.completed = false;
				}
				intermediateStepComplete = new DRLOnboardingProgressionData[intermediateOnboarding.steps.Count];
				for (int k = 0; k < intermediateOnboarding.steps.Count; k++)
				{
					_drlOnboardingProgressionData = new DRLOnboardingProgressionData
					{
						trackStatus = false,
						trackGUID = "",
						mapGUID = ""
					};
					intermediateStepComplete[k] = _drlOnboardingProgressionData;
				}
				onboarding.onboardingIntermediateProgressData = Serialize.ToJson(intermediateStepComplete.ToArray());
				break;
			}
			case OnboardingCampaignMode.Pro:
			{
				proStepComplete = new DRLOnboardingProgressionData[proOnboarding.steps.Count];
				for (int i = 0; i < proOnboarding.steps.Count; i++)
				{
					_drlOnboardingProgressionData = new DRLOnboardingProgressionData
					{
						trackStatus = false,
						trackGUID = "",
						mapGUID = ""
					};
					proStepComplete[i] = _drlOnboardingProgressionData;
				}
				foreach (OnboardingStep step4 in proOnboarding.steps)
				{
					step4.completed = false;
				}
				onboarding.onboardingProProgressData = Serialize.ToJson(proStepComplete.ToArray());
				break;
			}
			}
		}

		public void SetProgress(OnboardingCampaignMode p_onboarding)
		{
			OnboardingStateModel onboarding = base.app.model.storage.state.player.onboarding;
			_ = onboarding.progress;
			GetProgress();
			switch (p_onboarding)
			{
			case OnboardingCampaignMode.Beginner:
			{
				for (int j = 0; j < beginnerOnboarding.steps.Count; j++)
				{
					onboarding.progress[j].trackStatus = beginnerOnboarding.steps[j].completed;
					beginnerOnboarding.steps[j].completed = onboarding.progress[j].trackStatus;
				}
				break;
			}
			case OnboardingCampaignMode.Intermediate:
			{
				for (int k = 0; k < intermediateOnboarding.steps.Count; k++)
				{
					onboarding.progress[k].trackStatus = intermediateOnboarding.steps[k].completed;
					intermediateOnboarding.steps[k].completed = onboarding.progress[k].trackStatus;
				}
				break;
			}
			case OnboardingCampaignMode.Pro:
			{
				for (int i = 0; i < proOnboarding.steps.Count; i++)
				{
					onboarding.progress[i].trackStatus = proOnboarding.steps[i].completed;
					proOnboarding.steps[i].completed = onboarding.progress[i].trackStatus;
				}
				break;
			}
			}
			switch (activeOnboarding.mode)
			{
			case OnboardingCampaignMode.Beginner:
				beginnerStepComplete = onboarding.progress.ToArray();
				break;
			case OnboardingCampaignMode.Intermediate:
				intermediateStepComplete = onboarding.progress.ToArray();
				break;
			case OnboardingCampaignMode.Pro:
				proStepComplete = onboarding.progress.ToArray();
				break;
			}
		}

		public float GetProgressNormalized()
		{
			if (!inProgress)
			{
				return 0f;
			}
			int num = 0;
			switch (m_onboarding.mode)
			{
			case OnboardingCampaignMode.Beginner:
				num = beginnerProgress;
				break;
			case OnboardingCampaignMode.Intermediate:
				num = intermediateProgress;
				break;
			case OnboardingCampaignMode.Pro:
				num = proProgress;
				break;
			}
			return (float)num / (float)m_onboarding.steps.Count;
		}

		public void IncreaseProgress()
		{
		}

		public void DecreaseProgress()
		{
			if (inProgress)
			{
				int progress = GetProgress() - 1;
				SetProgress(progress);
			}
		}

		public void ResetProgress()
		{
			if (inProgress)
			{
				SetProgress(0);
			}
		}

		public void ResetAllProgress()
		{
			skipOnboarding = false;
			base.app.model.onboarding.skipOnboarding = false;
			firstStart = true;
			foreach (OnboardingCampaignMode value in EnumUtil.GetValues<OnboardingCampaignMode>())
			{
				ResetProgress(value);
			}
		}

		public void SetActiveStep(int p_step)
		{
			if (inProgress)
			{
				Mathf.Clamp(p_step, 0, m_onboarding.steps.Count - 1);
				step = p_step;
			}
		}

		public void SetOnboardingActive(OnboardingCampaignMode p_onboardingDifficulty)
		{
			switch (p_onboardingDifficulty)
			{
			case OnboardingCampaignMode.Beginner:
				activeOnboarding = beginnerOnboarding;
				break;
			case OnboardingCampaignMode.Intermediate:
				activeOnboarding = intermediateOnboarding;
				break;
			case OnboardingCampaignMode.Pro:
				activeOnboarding = proOnboarding;
				break;
			}
			step = Mathf.Clamp(step, currentStep, m_onboarding.steps.Count - 1);
			base.app.controller.onboarding.selectedDifficulty = p_onboardingDifficulty;
		}

		public int FirstIncompleteMission(DRLOnboarding p_onboarding)
		{
			for (int i = 0; i < p_onboarding.steps.Count; i++)
			{
				if (!p_onboarding.steps[i].completed)
				{
					return i;
				}
			}
			return 0;
		}

		public bool IsMissionStep(int index, DRLOnboarding p_onboarding)
		{
			if (p_onboarding == null)
			{
				p_onboarding = base.app.model.onboarding.activeOnboarding;
			}
			return p_onboarding.steps[index].type == OnboardingStep.OnboardingStepType.Mission;
		}

		public int GetCurrentRaceStep(DRLOnboarding p_onboarding, int currentStep)
		{
			if (p_onboarding == null)
			{
				p_onboarding = base.app.model.onboarding.activeOnboarding;
			}
			if (currentStep <= 0)
			{
				return 0;
			}
			int num = 0;
			for (int i = 0; i < currentStep; i++)
			{
				if (!IsMissionStep(i, p_onboarding))
				{
					num++;
				}
			}
			return num;
		}

		public int GetSelectedRaceStep(DRLOnboarding p_onboarding, int currentStep)
		{
			if (p_onboarding == null)
			{
				p_onboarding = base.app.model.onboarding.activeOnboarding;
			}
			if (currentStep <= 0)
			{
				return 0;
			}
			return GetTotalMissionSteps(p_onboarding) - currentStep;
		}

		public int GetTotalRaceSteps(DRLOnboarding p_onboarding)
		{
			if (p_onboarding == null)
			{
				p_onboarding = base.app.model.onboarding.activeOnboarding;
			}
			int num = 0;
			foreach (OnboardingStep step in p_onboarding.steps)
			{
				if (step.type == OnboardingStep.OnboardingStepType.Race)
				{
					num++;
				}
			}
			return num;
		}

		public int GetTotalMissionSteps(DRLOnboarding p_onboarding)
		{
			int num = 0;
			if (p_onboarding == null)
			{
				p_onboarding = base.app.model.onboarding.activeOnboarding;
			}
			foreach (OnboardingStep step in p_onboarding.steps)
			{
				if (step.type == OnboardingStep.OnboardingStepType.Mission)
				{
					num++;
				}
			}
			return num;
		}

		public bool IsCompleted()
		{
			if (!inProgress)
			{
				return false;
			}
			_ = step;
			_ = activeOnboarding.steps.Count;
			bool flag = true;
			for (int i = 0; i < activeOnboarding.steps.Count; i++)
			{
				flag = flag && activeOnboarding.steps[i].completed;
				if (!flag)
				{
					break;
				}
			}
			return flag;
		}

		public bool IsLastStep()
		{
			if (!inProgress)
			{
				return false;
			}
			return step == activeOnboarding.steps.Count - 1;
		}

		public bool WillRaceNext()
		{
			if (!inProgress)
			{
				return false;
			}
			if (activeOnboarding.steps[step].type == OnboardingStep.OnboardingStepType.Mission)
			{
				if (HasCompletedMissions() && step + 1 < activeOnboarding.steps.Count && activeOnboarding.steps[step + 1].type == OnboardingStep.OnboardingStepType.Race)
				{
					return step + 1 > activeOnboarding.steps.Count - 1;
				}
				return false;
			}
			return false;
		}

		public bool HasCompletedMissions()
		{
			if (!inProgress)
			{
				return false;
			}
			bool flag = true;
			for (int i = 0; i < activeOnboarding.steps.Count; i++)
			{
				if (activeOnboarding.steps[i].type == OnboardingStep.OnboardingStepType.Mission)
				{
					flag = activeOnboarding.steps[i].completed;
				}
				if (!flag)
				{
					break;
				}
			}
			return flag;
		}

		public void SetOnboardingInactive()
		{
			activeOnboarding = null;
			Notify("onboarding.campaign@stop");
		}

		public OnboardingStep.OnboardingStepType GetStepType()
		{
			if (!inProgress)
			{
				return OnboardingStep.OnboardingStepType.None;
			}
			return activeOnboarding.steps[step].type;
		}

		public void OnPersistency()
		{
			base.app.model.onboarding = this;
		}

		public bool hasProgressedOnboarding()
		{
			if (GetProgress(OnboardingCampaignMode.Beginner) > 0)
			{
				return true;
			}
			if (GetProgress(OnboardingCampaignMode.Intermediate) > 0)
			{
				return true;
			}
			return GetProgress(OnboardingCampaignMode.Pro) > 0;
		}

		public void GetDroneProfile(Drone drone)
		{
			playerProfile = drone.fc.profile;
		}

		public void SetPresetHigh(Drone drone)
		{
			FCProfileData active = base.app.model.storage.state.player.settings.tuning.GetActive();
			active.SetPreset(FCProfileData.Betaflight.MediumPresets[ControllerStateType.Taranis]);
			base.app.model.storage.state.player.settings.tuning.GetActive().SetPreset(FCProfileData.Betaflight.MediumPresets[ControllerStateType.Taranis]);
			drone.fc.profile = active;
			FCProfileData profile = base.app.model.storage.state.player.settings.tuning.GetProfile(0);
			base.app.model.storage.state.player.settings.tuning.profileActiveGUID = profile.guid;
		}

		public void ResetPreset(Drone drone)
		{
			drone.fc.profile = playerProfile;
		}
	}
}
