using System;
using UnityEngine;

namespace drl.game
{
	[Serializable]
	public class OnboardingStep
	{
		public enum OnboardingStepType
		{
			None = -1,
			Mission = 0,
			Race = 1
		}

		public bool completed;

		public OnboardingStepType type;

		[Header("Mission Step Data:")]
		public DRLQuest quest;

		public DRLMission mission;

		[Header("Race Step Data")]
		public string mapGuid;

		public string trackGuid;

		public string opponentReplayId;
	}
}
