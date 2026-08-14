using UnityEngine;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLSliderView : SliderView
	{
		public UINavigation handleNav;

		[SerializeField]
		private UINavigation m_nav;

		public FadeComponent sliderFocusFade;

		private float m_keyspeed;

		public float rate = 1f;

		public float rateControllerRawAxis = 1f;

		private bool m_stateLock;

		public UINavigation nav
		{
			get
			{
				if (!m_nav)
				{
					return m_nav = GetComponent<UINavigation>();
				}
				return m_nav;
			}
		}

		public float value
		{
			get
			{
				if (!(slider != null))
				{
					return 0f;
				}
				return slider.value;
			}
			set
			{
				if (slider != null)
				{
					slider.value = value;
				}
			}
		}

		protected void Update()
		{
			if (UINavigation.focus == handleNav)
			{
				float num = rate;
				bool flag = false;
				if (!slider)
				{
					flag = true;
				}
				if (DRLUINavigationSystem.IsButton())
				{
					flag = true;
				}
				if (flag)
				{
					this.TimerRunOnce(delegate
					{
						UINavigation.focus = nav;
						Notify("ui.slider.handle@unfocus");
					}, 1f / 60f);
					slider.interactable = false;
					return;
				}
				float num2 = 0f;
				float num3 = 1f;
				float p = 8f;
				float num4 = 0f;
				float num5 = Mathf.Abs(1f - num4);
				bool flag2 = false;
				switch (slider.direction)
				{
				case Slider.Direction.LeftToRight:
				case Slider.Direction.RightToLeft:
					if (DRLUINavigationSystem.controllerNavEnabled)
					{
						if (RCI.GetButton(ConsoleButtons.DPadRight))
						{
							num2 = num5 * m_keyspeed * 0.2f;
							p = 1f;
							flag2 = true;
						}
						else if (RCI.GetButton(ConsoleButtons.DPadLeft))
						{
							num2 = (0f - num5) * m_keyspeed * 0.2f;
							p = 1f;
							flag2 = true;
						}
						else if (RCI.HasNavigationController)
						{
							num2 = RCI.GetRawAxis(RawAxis.LeftStickX, RCI.navigationController);
							if (Mathf.Abs(num2) > 0.001f)
							{
								num = rateControllerRawAxis;
							}
						}
					}
					if (DRLUINavigationSystem.keyboardNavEnabled && Input.GetKey(KeyCode.LeftArrow))
					{
						num2 = (0f - num5) * m_keyspeed * 0.2f;
						p = 1f;
						flag2 = true;
					}
					if (DRLUINavigationSystem.keyboardNavEnabled && Input.GetKey(KeyCode.RightArrow))
					{
						num2 = num5 * m_keyspeed * 0.2f;
						p = 1f;
						flag2 = true;
					}
					break;
				case Slider.Direction.BottomToTop:
				case Slider.Direction.TopToBottom:
					if (DRLUINavigationSystem.controllerNavEnabled && RCI.HasNavigationController)
					{
						num2 = RCI.GetRawAxis(RawAxis.LeftStickY, RCI.navigationController);
						if (Mathf.Abs(num2) > 0.001f)
						{
							num = rateControllerRawAxis;
						}
					}
					num2 = Mathf.Pow(Mathf.Abs(num2), 8f);
					if (DRLUINavigationSystem.keyboardNavEnabled && Input.GetKey(KeyCode.UpArrow))
					{
						num2 = (0f - num5) * m_keyspeed * 0.2f;
						p = 1f;
						flag2 = true;
					}
					if (DRLUINavigationSystem.keyboardNavEnabled && Input.GetKey(KeyCode.DownArrow))
					{
						num2 = num5 * m_keyspeed * 0.2f;
						p = 1f;
						flag2 = true;
					}
					if (DRLUINavigationSystem.controllerNavEnabled && (RCI.GetButtonDown(ConsoleButtons.DPadDown) || RCI.GetButtonDown(ConsoleButtons.DPadUp)))
					{
						num2 = num5 * m_keyspeed * 0.2f;
						p = 1f;
						flag2 = true;
					}
					break;
				}
				switch (slider.direction)
				{
				case Slider.Direction.LeftToRight:
					num3 = 1f;
					break;
				case Slider.Direction.RightToLeft:
					num3 = -1f;
					break;
				case Slider.Direction.BottomToTop:
					num3 = -1f;
					break;
				case Slider.Direction.TopToBottom:
					num3 = 1f;
					break;
				}
				if (flag2)
				{
					m_keyspeed += Time.unscaledDeltaTime;
					m_keyspeed = Mathf.Clamp01(m_keyspeed);
				}
				else
				{
					m_keyspeed = 0f;
				}
				float num6 = ((num2 < 0f) ? (-1f) : 1f);
				num2 = Mathf.Pow(Mathf.Abs(num2), p);
				float num7 = num6 * num2 * num3 * 2f * Time.unscaledDeltaTime * num;
				if (Mathf.Abs(num2) <= 0.001f)
				{
					num7 = 0f;
				}
				if (slider.enabled && Mathf.Abs(num7) > 0f)
				{
					slider.normalizedValue += num7;
				}
			}
			else if ((bool)sliderFocusFade)
			{
				if (sliderFocusFade.pulse)
				{
					sliderFocusFade.FadeOut();
				}
				sliderFocusFade.pulse = false;
			}
		}

		public override void OnUnfocus()
		{
			base.OnUnfocus();
			if ((bool)slider)
			{
				slider.interactable = false;
			}
		}

		protected override void OnState(string p_state)
		{
			if (m_stateLock)
			{
				return;
			}
			m_stateLock = true;
			base.OnState(p_state);
			bool flag = false;
			switch (p_state)
			{
			case "over":
				flag = Cursor.visible;
				break;
			case "lclick":
				flag = true;
				break;
			}
			if (flag && (bool)handleNav)
			{
				UINavigation.focus = handleNav;
				if ((bool)slider)
				{
					slider.interactable = false;
					Timer.Set(slider, "interactable", 1f / 30f, true);
				}
				if ((bool)sliderFocusFade)
				{
					sliderFocusFade.pulse = true;
				}
			}
			m_stateLock = false;
		}
	}
}
