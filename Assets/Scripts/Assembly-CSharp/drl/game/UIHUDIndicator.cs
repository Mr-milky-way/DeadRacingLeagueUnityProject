using UnityEngine;

namespace drl.game
{
	public class UIHUDIndicator : MonoBehaviour
	{
		public GameObject rollRight;

		public GameObject rollLeft;

		public GameObject hover;

		public GameObject poleRight;

		public GameObject poleLeft;

		public GameObject climb;

		public GameObject yawRight;

		public GameObject yawLeft;

		public GameObject pitchForward;

		public GameObject pitchBackwards;

		public void Show(Indicator p_indicator)
		{
			Clear();
			switch (p_indicator)
			{
			case Indicator.RollRight:
				rollRight.SetActive(value: true);
				break;
			case Indicator.RollLeft:
				rollLeft.SetActive(value: true);
				break;
			case Indicator.Hover:
				hover.SetActive(value: true);
				break;
			case Indicator.PoleRight:
				poleRight.SetActive(value: true);
				break;
			case Indicator.PoleLeft:
				poleLeft.SetActive(value: true);
				break;
			case Indicator.Climb:
				climb.SetActive(value: true);
				break;
			case Indicator.YawRight:
				yawRight.SetActive(value: true);
				break;
			case Indicator.YawLeft:
				yawLeft.SetActive(value: true);
				break;
			case Indicator.PitchForward:
				pitchForward.SetActive(value: true);
				break;
			case Indicator.PitchBackwards:
				pitchBackwards.SetActive(value: true);
				break;
			}
		}

		public void Hide(Indicator p_indicator)
		{
			switch (p_indicator)
			{
			case Indicator.RollRight:
				rollRight.SetActive(value: false);
				break;
			case Indicator.RollLeft:
				rollLeft.SetActive(value: false);
				break;
			case Indicator.Hover:
				hover.SetActive(value: false);
				break;
			case Indicator.PoleRight:
				poleRight.SetActive(value: false);
				break;
			case Indicator.PoleLeft:
				poleLeft.SetActive(value: false);
				break;
			case Indicator.Climb:
				climb.SetActive(value: false);
				break;
			case Indicator.YawRight:
				yawRight.SetActive(value: false);
				break;
			case Indicator.YawLeft:
				yawLeft.SetActive(value: false);
				break;
			case Indicator.PitchForward:
				pitchForward.SetActive(value: false);
				break;
			case Indicator.PitchBackwards:
				pitchBackwards.SetActive(value: false);
				break;
			}
		}

		public void Clear()
		{
			rollRight.SetActive(value: false);
			rollLeft.SetActive(value: false);
			hover.SetActive(value: false);
			poleRight.SetActive(value: false);
			poleLeft.SetActive(value: false);
			climb.SetActive(value: false);
			yawLeft.SetActive(value: false);
			yawRight.SetActive(value: false);
			pitchBackwards.SetActive(value: false);
			pitchForward.SetActive(value: false);
		}
	}
}
