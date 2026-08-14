using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.sim
{
	public class DebugFlowModuleUI : FlowModuleUI
	{
		public Text dialogFieldRight;

		public Text dialogFieldLeft;

		public RawImage imageLeft;

		public FadeComponent dialogFade;

		public Text timer;

		public Text timerCountdown;

		public Text rangeTimer;

		public Text step;

		public RectTransform fadeRT;

		public FadeComponent controller;

		public FadeComponent npcOverlay;

		public Text npcOverlayText;

		public FadeComponent footer;

		public FadeComponent header;

		public GameObject rightMeter;

		public GameObject leftMeter;

		public AudioSource audio;

		private bool runningTimer;

		private DateTime timersClock;

		private void Update()
		{
			if (timer != null)
			{
				if (runningTimer)
				{
					timersClock = timersClock.AddSeconds(Time.deltaTime);
				}
				SetTimer(timersClock);
			}
		}

		public override void SetDialog(Texture p_image, string p_text)
		{
			if ((bool)imageLeft)
			{
				imageLeft.texture = p_image;
				imageLeft.gameObject.SetActive(p_image != null);
			}
			if ((bool)dialogFieldLeft)
			{
				dialogFieldLeft.text = p_text;
			}
		}

		public override void SetDialog(string p_text)
		{
			if ((bool)dialogFieldLeft)
			{
				dialogFieldLeft.text = p_text;
			}
		}

		public override void SetDialogNPC(string p_text)
		{
			if ((bool)dialogFieldRight)
			{
				dialogFieldRight.text = p_text;
			}
		}

		public override void SetDialog(Texture p_image)
		{
			if ((bool)imageLeft)
			{
				imageLeft.texture = p_image;
				imageLeft.enabled = p_image != null;
			}
		}

		public override void Show(ElementType p_type, float p_delay = 0f)
		{
			switch (p_type)
			{
			case ElementType.Dialog:
				dialogFade.FadeIn(0.3f, p_delay);
				break;
			case ElementType.Footer:
				footer.FadeIn(0.3f, p_delay);
				break;
			case ElementType.FooterController:
				Debug.Log("NotImplemented");
				break;
			case ElementType.FooterNPC:
				Debug.Log("NotImplemented");
				break;
			case ElementType.Header:
				header.FadeIn(0.3f, p_delay);
				break;
			case ElementType.HeaderStep:
				Debug.Log("NotImplemented");
				break;
			case ElementType.HeaderTimer:
				Debug.Log("NotImplemented");
				break;
			}
		}

		public override void Hide(ElementType p_type, float p_delay = 0f)
		{
			switch (p_type)
			{
			case ElementType.Dialog:
				dialogFade.FadeOut(0.3f, p_delay);
				break;
			case ElementType.Footer:
				footer.FadeOut(0.3f, p_delay);
				break;
			case ElementType.FooterController:
				Debug.Log("NotImplemented");
				break;
			case ElementType.FooterNPC:
				Debug.Log("NotImplemented");
				break;
			case ElementType.Header:
				header.FadeOut(0.3f, p_delay);
				break;
			case ElementType.HeaderStep:
				Debug.Log("NotImplemented");
				break;
			case ElementType.HeaderTimer:
				Debug.Log("NotImplemented");
				break;
			}
		}

		public override void FadeIn(float p_delay = 0f, float duration = 2f)
		{
			Vector2 anchoredPosition = fadeRT.anchoredPosition;
			anchoredPosition.x = 0f;
			Tween.Add(fadeRT, "anchoredPosition", anchoredPosition, 1f, p_delay, Cubic.Out);
			anchoredPosition.x = -3600f;
			fadeRT.anchoredPosition = anchoredPosition;
		}

		public override void FadeOut(float p_delay = 0f, float duration = 2f)
		{
			Vector2 anchoredPosition = fadeRT.anchoredPosition;
			anchoredPosition.x = 3600f;
			Tween.Add(fadeRT, "anchoredPosition", anchoredPosition, 1f, p_delay, Cubic.Out);
			anchoredPosition.x = 0f;
			fadeRT.anchoredPosition = anchoredPosition;
		}

		public override void SetStep(int p_step, int p_total)
		{
			step.text = string.Format("{0}/{1}", p_step.ToString("D2"), p_total.ToString("D2"));
		}

		public void ShowContoller()
		{
			controller.FadeIn(0.3f);
		}

		public void HideController()
		{
			controller.FadeOut(0.3f);
		}

		public void ShowNPCOverlay()
		{
			npcOverlay.FadeIn(0.3f);
		}

		public new void HideNPCOverlay()
		{
			npcOverlay.FadeOut(0.3f);
		}

		public void SetTextNPCOverlay(string text)
		{
			Color color = npcOverlayText.color;
			Tween.Add(p_to: Color.clear, p_target: npcOverlayText, p_property: "color", p_duration: 0.3f, p_delay: 0f, p_easing: Cubic.Out);
			Activity.RunOnce(delegate
			{
				npcOverlayText.text = text;
			}, 0.3f);
			Tween.Add(npcOverlayText, "color", color, 0.3f, 0.4f, Cubic.Out);
		}

		public void ShowHeader()
		{
			header.FadeIn(0.3f);
		}

		public void HideHeader()
		{
			header.FadeOut(0.3f);
		}

		public new void ShowFooter()
		{
			footer.FadeIn(0.3f);
		}

		public new void HideFooter()
		{
			footer.FadeOut(0.3f);
		}

		public void SetRightText(string text)
		{
			if ((bool)dialogFieldRight)
			{
				dialogFieldRight.text = text;
			}
		}

		public void SetRightGauge(float p_normalizedValue)
		{
			rightMeter.GetComponent<Slider>().normalizedValue = p_normalizedValue;
		}

		public void SetLeftGauge(float p_normalizedValue)
		{
			leftMeter.GetComponent<Slider>().normalizedValue = p_normalizedValue;
		}

		public void SetMissionProgress(float p_normalizedValue)
		{
			rightMeter.GetComponent<FadeComponent>().FadeIn(0.3f);
			SetRightGauge(p_normalizedValue);
		}

		public void ToggleTimer(bool toggle)
		{
			runningTimer = toggle;
		}

		public void SetTimer(DateTime time)
		{
			timer.text = string.Format("{0}:{1}:{2}", time.Minute, time.Second.ToString("D2"), Mathf.FloorToInt((float)time.Millisecond * 0.1f).ToString("D2"));
		}

		public void SetTimerCountdown(float time)
		{
			int num = (int)time / 60;
			int num2 = (int)time % 60;
			string arg = time.ToString("0.00", CultureInfo.InvariantCulture).Split('.')[1];
			timer.transform.parent.GetComponent<FadeComponent>().FadeOut(0.1f);
			timerCountdown.transform.parent.GetComponent<FadeComponent>().FadeIn(0.3f);
			timerCountdown.text = $"{num}:{num2}:{arg}";
		}

		public void SetRangeTimer(float time)
		{
			int num = (int)time / 60;
			int num2 = (int)time % 60;
			string arg = time.ToString("0.0", CultureInfo.InvariantCulture).Split('.')[1];
			rangeTimer.transform.parent.GetComponent<FadeComponent>().FadeIn(0.3f);
			rangeTimer.text = $"{num}:{num2}:{arg}";
		}

		public void HideControllerIcons()
		{
			Transform transform = controller.transform.GetChild(0).Find("left-stick");
			Transform transform2 = controller.transform.GetChild(0).Find("right-stick");
			for (int i = 0; i < transform.childCount - 1; i++)
			{
				transform.GetChild(i).gameObject.SetActive(value: false);
				transform2.GetChild(i).gameObject.SetActive(value: false);
			}
		}

		public void ShowControllerIcon(FNController.StickIcon icon)
		{
			Transform transform = controller.transform.GetChild(0).Find("left-stick");
			Transform transform2 = controller.transform.GetChild(0).Find("right-stick");
			HideControllerIcons();
			switch (icon)
			{
			case FNController.StickIcon.leftStick_right:
				transform.GetChild(0).gameObject.SetActive(value: true);
				break;
			case FNController.StickIcon.leftStick_left:
				transform.GetChild(1).gameObject.SetActive(value: true);
				break;
			case FNController.StickIcon.leftStick_up:
				transform.GetChild(2).gameObject.SetActive(value: true);
				break;
			case FNController.StickIcon.leftStick_down:
				transform.GetChild(3).gameObject.SetActive(value: true);
				break;
			case FNController.StickIcon.rightStick_right:
				transform2.GetChild(0).gameObject.SetActive(value: true);
				break;
			case FNController.StickIcon.rightStick_left:
				transform2.GetChild(1).gameObject.SetActive(value: true);
				break;
			case FNController.StickIcon.rightStick_up:
				transform2.GetChild(2).gameObject.SetActive(value: true);
				break;
			case FNController.StickIcon.rightStick_down:
				transform2.GetChild(3).gameObject.SetActive(value: true);
				break;
			case FNController.StickIcon.throttle:
				transform.GetChild(2).gameObject.SetActive(value: true);
				transform.GetChild(3).gameObject.SetActive(value: true);
				break;
			case FNController.StickIcon.yaw:
				transform.GetChild(0).gameObject.SetActive(value: true);
				transform.GetChild(1).gameObject.SetActive(value: true);
				break;
			case FNController.StickIcon.pitch:
				transform2.GetChild(2).gameObject.SetActive(value: true);
				transform2.GetChild(3).gameObject.SetActive(value: true);
				break;
			case FNController.StickIcon.roll:
				transform2.GetChild(0).gameObject.SetActive(value: true);
				transform2.GetChild(1).gameObject.SetActive(value: true);
				break;
			}
		}
	}
}
