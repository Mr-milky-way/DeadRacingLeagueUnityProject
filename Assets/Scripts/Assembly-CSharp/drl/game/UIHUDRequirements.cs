using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class UIHUDRequirements : MonoBehaviour
	{
		public enum timerLabel
		{
			ELAPSED = 0,
			COUNTDOWN = 1
		}

		public bool stepsCounterRequired;

		public bool progressBarRequired = true;

		public Texture stepsIconImageTexture;

		public int stepsCounterTotal = 10;

		public int progressBarTotal;

		public bool timerRequired = true;

		public string timerLabelString = "ELAPSED";

		public bool timer2Required;

		public bool timer2VisibleAtStart = true;

		public string timer2LabelString = "COUNTDOWN";

		public bool controllerVisibleAtStart = true;

		private void Start()
		{
			if (progressBarTotal == 0)
			{
				progressBarTotal = stepsCounterTotal;
			}
		}

		public string GetLocalisedTimerLabel1(Localization loc)
		{
			if (timerLabelString == timerLabel.ELAPSED.ToString())
			{
				return loc.Get("race-hud.training.timer-elapsed", timerLabelString);
			}
			if (timerLabelString == timerLabel.COUNTDOWN.ToString())
			{
				return loc.Get("race-hud.training.timer-countdown", timerLabelString);
			}
			return timerLabelString;
		}

		public string GetLocalisedTimerLabel2(Localization loc)
		{
			if (timer2LabelString == timerLabel.ELAPSED.ToString())
			{
				return loc.Get("race-hud.training.timer-elapsed", timer2LabelString);
			}
			if (timer2LabelString == timerLabel.COUNTDOWN.ToString())
			{
				return loc.Get("race-hud.training.timer-countdown", timer2LabelString);
			}
			return timer2LabelString;
		}
	}
}
