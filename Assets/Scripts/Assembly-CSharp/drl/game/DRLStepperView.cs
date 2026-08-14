using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLStepperView : StepperView
	{
		public Image arrow;

		public List<float> rates = new List<float>(new float[7] { 0f, 0.8f, 0.5f, 0.2f, 0.2f, 0.2f, 0.1f });

		public float arrowHeight = 35f;

		private int m_rate_current;

		private float m_rate_elapsed;

		protected bool m_focused;

		public float previewTimeout = 2f;

		private Activity m_preview_timer;

		public int previewCount;

		private RectTransform m_preview_container;

		private RectTransform m_preview_up_container;

		private Text m_preview_up_field;

		private FadeComponent m_preview_up_fade;

		private RectTransform m_preview_down_container;

		private Text m_preview_down_field;

		private FadeComponent m_preview_down_fade;

		protected RectTransform previewContainer
		{
			get
			{
				if (!m_preview_container)
				{
					return m_preview_container = base.transform.Find("preview") as RectTransform;
				}
				return m_preview_container;
			}
		}

		protected RectTransform previewUpContainer
		{
			get
			{
				if (!m_preview_up_container)
				{
					if (!previewContainer)
					{
						return null;
					}
					return m_preview_up_container = (RectTransform)previewContainer.Find("up");
				}
				return m_preview_up_container;
			}
		}

		protected Text previewUpField
		{
			get
			{
				if (!m_preview_up_field)
				{
					if (!previewUpContainer)
					{
						return null;
					}
					return m_preview_up_field = previewUpContainer.Find("field").GetComponent<Text>();
				}
				return m_preview_up_field;
			}
		}

		protected FadeComponent previewUpFade
		{
			get
			{
				if (!m_preview_up_fade)
				{
					if (!previewUpContainer)
					{
						return null;
					}
					return m_preview_up_fade = previewUpContainer.GetComponent<FadeComponent>();
				}
				return m_preview_up_fade;
			}
		}

		protected RectTransform previewDownContainer
		{
			get
			{
				if (!m_preview_down_container)
				{
					if (!previewContainer)
					{
						return null;
					}
					return m_preview_down_container = (RectTransform)previewContainer.Find("down");
				}
				return m_preview_down_container;
			}
		}

		protected Text previewDownField
		{
			get
			{
				if (!m_preview_down_field)
				{
					if (!previewUpContainer)
					{
						return null;
					}
					return m_preview_down_field = previewDownContainer.Find("field").GetComponent<Text>();
				}
				return m_preview_down_field;
			}
		}

		protected FadeComponent previewDownFade
		{
			get
			{
				if (!m_preview_down_fade)
				{
					if (!previewDownContainer)
					{
						return null;
					}
					return m_preview_down_fade = previewDownContainer.GetComponent<FadeComponent>();
				}
				return m_preview_down_fade;
			}
		}

		protected override void OnChange()
		{
			base.OnChange();
			if ((bool)arrow)
			{
				RectTransform obj = arrow.transform as RectTransform;
				Vector2 sizeDelta = obj.sizeDelta;
				sizeDelta.y = arrowHeight;
				obj.sizeDelta = sizeDelta;
				sizeDelta.y = arrowHeight - 10f;
				Tween.Add(obj, "sizeDelta", sizeDelta, 0.5f, Cubic.Out);
			}
		}

		public override void OnFocus()
		{
			base.OnFocus();
			m_focused = true;
		}

		public override void OnUnfocus()
		{
			base.OnFocus();
			m_focused = false;
			if ((bool)previewDownContainer)
			{
				previewDownContainer.gameObject.SetActive(value: false);
			}
			if ((bool)previewUpContainer)
			{
				previewUpContainer.gameObject.SetActive(value: false);
			}
		}

		protected override void Update()
		{
			if (!m_focused)
			{
				return;
			}
			base.Update();
			bool num = rates.Count > 0;
			bool hasNavigationController = RCI.HasNavigationController;
			bool flag = false;
			bool flag2 = false;
			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				flag2 = true;
			}
			if (Input.GetKeyDown(KeyCode.PageDown))
			{
				flag = true;
			}
			bool flag3 = false;
			bool flag4 = false;
			if (Input.GetKey(KeyCode.PageUp))
			{
				flag4 = true;
			}
			if (Input.GetKey(KeyCode.PageDown))
			{
				flag3 = true;
			}
			if (hasNavigationController && RCI.GetAxisTrigger(RawAxis.RightStickY, isPositiveSign: true))
			{
				flag2 = true;
			}
			if (hasNavigationController && RCI.GetAxisTrigger(RawAxis.RightStickY, isPositiveSign: false))
			{
				flag = true;
			}
			if (hasNavigationController && RCI.GetRawAxis(RawAxis.RightStickY, RCI.navigationController) >= 0.7f)
			{
				flag4 = true;
			}
			if (hasNavigationController && RCI.GetRawAxis(RawAxis.RightStickY, RCI.navigationController) <= -0.7f)
			{
				flag3 = true;
			}
			if (num)
			{
				if (!flag3 && !flag4)
				{
					m_rate_current = 0;
					m_rate_elapsed = 0f;
				}
				bool num2 = flag3 || flag4;
				int num3 = (m_rate_current = Mathf.Clamp(m_rate_current, 0, rates.Count - 1));
				float num4 = rates[num3];
				if (!num2)
				{
					return;
				}
				m_rate_elapsed += Time.unscaledDeltaTime;
				if (m_rate_elapsed >= num4)
				{
					m_rate_current++;
					m_rate_elapsed = 0f;
					if (flag4)
					{
						OnState("lclick");
					}
					if (flag3)
					{
						OnState("rclick");
					}
				}
			}
			else
			{
				if (flag2)
				{
					OnState("lclick");
				}
				if (flag)
				{
					OnState("rclick");
				}
			}
		}

		protected override void OnState(string p_state)
		{
			base.OnState(p_state);
			if (!previewUpContainer || !previewDownContainer)
			{
				return;
			}
			Text text = null;
			bool flag = false;
			int num = Mathf.Min(previewCount, max - min + 1);
			switch (p_state)
			{
			case "rclick":
				text = previewUpField;
				flag = false;
				break;
			case "lclick":
				text = previewDownField;
				flag = true;
				break;
			}
			int num2 = index;
			string text2 = "";
			Color color = (text ? text.color : Colorf.transparent);
			float num3 = ((previewCount <= 0) ? 0f : (1f / (float)previewCount));
			float num4 = 1f;
			int num5 = 0;
			for (int i = 0; i < num; i++)
			{
				num2 += (flag ? 1 : (-1));
				bool flag2 = false;
				if (num2 < min)
				{
					num2 = ((mode == Mode.Loop) ? max : (min - 1));
					flag2 = mode == Mode.Loop;
				}
				if (num2 > max)
				{
					num2 = ((mode == Mode.Loop) ? min : (max + 1));
					flag2 = mode == Mode.Loop;
				}
				string text3 = "";
				bool flag3 = num2 < 0 || num2 >= labels.Length;
				if (num2 >= min && num2 <= max)
				{
					text3 = (flag3 ? "" : labels[num2]);
				}
				if (!flag3)
				{
					num5 += ((!flag2) ? 1 : 2);
					Color p_color = color;
					p_color.a = num4;
					string text4 = (flag2 ? "---\n" : "");
					string text5 = "<color=" + Colorf.ToRGBAHex(p_color, "#") + ">" + text3 + "</color>\n";
					text2 = (flag ? (text2 + text4 + text5) : (text5 + text4 + text2));
					num4 -= num3;
					num4 = Mathf.Clamp(num4, 0.3f, 1f);
				}
			}
			float num6 = 13f;
			float num7 = Mathf.Max(0f, num5 - 1);
			float num8 = 0f;
			float num9 = Mathf.Max(0f, num5 - 1);
			float y = 46f + num6 * num7 + num8 * num9;
			switch (p_state)
			{
			case "rclick":
			{
				previewUpContainer.gameObject.SetActive(num5 > 0);
				Vector2 sizeDelta = previewUpContainer.sizeDelta;
				sizeDelta.y = y;
				previewUpContainer.sizeDelta = sizeDelta;
				previewDownContainer.gameObject.SetActive(value: false);
				previewUpFade.FadeIn(0.2f);
				break;
			}
			case "lclick":
			{
				previewUpContainer.gameObject.SetActive(value: false);
				previewDownContainer.gameObject.SetActive(num5 > 0);
				Vector2 sizeDelta = previewDownContainer.sizeDelta;
				sizeDelta.y = y;
				previewDownContainer.sizeDelta = sizeDelta;
				previewDownFade.FadeIn(0.2f);
				break;
			}
			}
			if ((bool)text)
			{
				text.text = text2;
			}
			if (m_preview_timer != null)
			{
				m_preview_timer.Stop();
			}
			m_preview_timer = Activity.RunOnce(delegate
			{
				previewUpFade.FadeOut(0.2f);
				previewDownFade.FadeOut(0.2f);
			}, previewTimeout);
		}
	}
	public class DRLStepperView<T> : StepperView<T>
	{
		public Image arrow;

		public List<float> rates = new List<float>(new float[7] { 0f, 0.8f, 0.5f, 0.2f, 0.2f, 0.2f, 0.1f });

		private int m_rate_current;

		private float m_rate_elapsed;

		protected bool m_focused;

		public float previewTimeout = 2f;

		private Activity m_preview_timer;

		public int previewCount;

		private RectTransform m_preview_container;

		private RectTransform m_preview_up_container;

		private Text m_preview_up_field;

		private FadeComponent m_preview_up_fade;

		private RectTransform m_preview_down_container;

		private Text m_preview_down_field;

		private FadeComponent m_preview_down_fade;

		protected RectTransform previewContainer
		{
			get
			{
				if (!m_preview_container)
				{
					return m_preview_container = base.transform.Find("preview") as RectTransform;
				}
				return m_preview_container;
			}
		}

		protected RectTransform previewUpContainer
		{
			get
			{
				if (!m_preview_up_container)
				{
					if (!previewContainer)
					{
						return null;
					}
					return m_preview_up_container = (RectTransform)previewContainer.Find("up");
				}
				return m_preview_up_container;
			}
		}

		protected Text previewUpField
		{
			get
			{
				if (!m_preview_up_field)
				{
					if (!previewUpContainer)
					{
						return null;
					}
					return m_preview_up_field = previewUpContainer.Find("field").GetComponent<Text>();
				}
				return m_preview_up_field;
			}
		}

		protected FadeComponent previewUpFade
		{
			get
			{
				if (!m_preview_up_fade)
				{
					if (!previewUpContainer)
					{
						return null;
					}
					return m_preview_up_fade = previewUpContainer.GetComponent<FadeComponent>();
				}
				return m_preview_up_fade;
			}
		}

		protected RectTransform previewDownContainer
		{
			get
			{
				if (!m_preview_down_container)
				{
					if (!previewContainer)
					{
						return null;
					}
					return m_preview_down_container = (RectTransform)previewContainer.Find("down");
				}
				return m_preview_down_container;
			}
		}

		protected Text previewDownField
		{
			get
			{
				if (!m_preview_down_field)
				{
					if (!previewUpContainer)
					{
						return null;
					}
					return m_preview_down_field = previewDownContainer.Find("field").GetComponent<Text>();
				}
				return m_preview_down_field;
			}
		}

		protected FadeComponent previewDownFade
		{
			get
			{
				if (!m_preview_down_fade)
				{
					if (!previewDownContainer)
					{
						return null;
					}
					return m_preview_down_fade = previewDownContainer.GetComponent<FadeComponent>();
				}
				return m_preview_down_fade;
			}
		}

		protected override void OnChange()
		{
			base.OnChange();
			if ((bool)arrow)
			{
				RectTransform obj = arrow.transform as RectTransform;
				Vector2 sizeDelta = obj.sizeDelta;
				sizeDelta.y = 35f;
				obj.sizeDelta = sizeDelta;
				sizeDelta.y = 25f;
				Tween.Add(obj, "sizeDelta", sizeDelta, 0.5f, Cubic.Out);
			}
		}

		public override void OnFocus()
		{
			base.OnFocus();
			m_focused = true;
		}

		public override void OnUnfocus()
		{
			base.OnFocus();
			m_focused = false;
			if ((bool)previewDownContainer)
			{
				previewDownContainer.gameObject.SetActive(value: false);
			}
			if ((bool)previewUpContainer)
			{
				previewUpContainer.gameObject.SetActive(value: false);
			}
		}

		protected override void Update()
		{
			if (!m_focused)
			{
				return;
			}
			base.Update();
			bool num = rates.Count > 0;
			_ = RCI.HasNavigationController;
			bool flag = false;
			bool flag2 = false;
			if (RCI.GetButtonDown(ConsoleButtons.ActionTopRow1))
			{
				flag2 = true;
			}
			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				flag2 = true;
			}
			if (Input.GetKeyDown(KeyCode.PageDown))
			{
				flag = true;
			}
			bool flag3 = false;
			bool flag4 = false;
			if (RCI.GetButton(ConsoleButtons.ActionTopRow1))
			{
				flag4 = true;
			}
			if (Input.GetKey(KeyCode.PageUp))
			{
				flag4 = true;
			}
			if (Input.GetKey(KeyCode.PageDown))
			{
				flag3 = true;
			}
			if (num)
			{
				if (!flag3 && !flag4)
				{
					m_rate_current = 0;
					m_rate_elapsed = 0f;
				}
				bool num2 = flag3 || flag4;
				int num3 = (m_rate_current = Mathf.Clamp(m_rate_current, 0, rates.Count - 1));
				float num4 = rates[num3];
				if (!num2)
				{
					return;
				}
				m_rate_elapsed += Time.unscaledDeltaTime;
				if (m_rate_elapsed >= num4)
				{
					m_rate_current++;
					m_rate_elapsed = 0f;
					if (flag4)
					{
						OnState("lclick");
					}
					if (flag3)
					{
						OnState("rclick");
					}
				}
			}
			else
			{
				if (flag2)
				{
					OnState("lclick");
				}
				if (flag)
				{
					OnState("rclick");
				}
			}
		}

		protected override void OnState(string p_state)
		{
			base.OnState(p_state);
			if (!previewUpContainer || !previewDownContainer)
			{
				return;
			}
			Text text = null;
			bool flag = false;
			int num = Mathf.Min(previewCount, max - min + 1);
			switch (p_state)
			{
			case "rclick":
				text = previewUpField;
				flag = false;
				break;
			case "lclick":
				text = previewDownField;
				flag = true;
				break;
			}
			int num2 = index;
			string text2 = "";
			Color color = (text ? text.color : Colorf.transparent);
			float num3 = ((previewCount <= 0) ? 0f : (1f / (float)previewCount));
			float num4 = 1f;
			int num5 = 0;
			for (int i = 0; i < num; i++)
			{
				num2 += (flag ? 1 : (-1));
				bool flag2 = false;
				if (num2 < min)
				{
					num2 = ((mode == Mode.Loop) ? max : (min - 1));
					flag2 = mode == Mode.Loop;
				}
				if (num2 > max)
				{
					num2 = ((mode == Mode.Loop) ? min : (max + 1));
					flag2 = mode == Mode.Loop;
				}
				string text3 = "";
				bool flag3 = num2 < 0 || num2 >= labels.Length;
				if (num2 >= min && num2 <= max)
				{
					text3 = (flag3 ? "" : labels[num2]);
				}
				if (!flag3)
				{
					num5 += ((!flag2) ? 1 : 2);
					Color p_color = color;
					p_color.a = num4;
					string text4 = (flag2 ? "---\n" : "");
					string text5 = "<color=" + Colorf.ToRGBAHex(p_color, "#") + ">" + text3 + "</color>\n";
					text2 = (flag ? (text2 + text4 + text5) : (text5 + text4 + text2));
					num4 -= num3;
					num4 = Mathf.Clamp(num4, 0.3f, 1f);
				}
			}
			float num6 = 13f;
			float num7 = Mathf.Max(0f, num5 - 1);
			float num8 = 0f;
			float num9 = Mathf.Max(0f, num5 - 1);
			float y = 46f + num6 * num7 + num8 * num9;
			switch (p_state)
			{
			case "rclick":
			{
				previewUpContainer.gameObject.SetActive(num5 > 0);
				Vector2 sizeDelta = previewUpContainer.sizeDelta;
				sizeDelta.y = y;
				previewUpContainer.sizeDelta = sizeDelta;
				previewDownContainer.gameObject.SetActive(value: false);
				previewUpFade.FadeIn(0.2f);
				break;
			}
			case "lclick":
			{
				previewUpContainer.gameObject.SetActive(value: false);
				previewDownContainer.gameObject.SetActive(num5 > 0);
				Vector2 sizeDelta = previewDownContainer.sizeDelta;
				sizeDelta.y = y;
				previewDownContainer.sizeDelta = sizeDelta;
				previewDownFade.FadeIn(0.2f);
				break;
			}
			}
			if ((bool)text)
			{
				text.text = text2;
			}
			if (m_preview_timer != null)
			{
				m_preview_timer.Stop();
			}
			m_preview_timer = Activity.RunOnce(delegate
			{
				previewUpFade.FadeOut(0.2f);
				previewDownFade.FadeOut(0.2f);
			}, previewTimeout);
		}
	}
}
