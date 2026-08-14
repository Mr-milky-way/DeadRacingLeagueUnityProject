using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLIntStepperView : IntStepperView
	{
		public Image arrow;

		protected bool m_focused;

		public List<float> rates = new List<float>(new float[7] { 0f, 0.8f, 0.5f, 0.2f, 0.2f, 0.2f, 0.1f });

		private int m_rate_current;

		private float m_rate_elapsed;

		protected override void OnChange()
		{
			base.OnChange();
			if ((bool)arrow)
			{
				RectTransform obj = arrow.transform as RectTransform;
				Vector2 sizeDelta = obj.sizeDelta;
				sizeDelta.y = 25f;
				obj.sizeDelta = sizeDelta;
				sizeDelta.y = 15f;
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
	}
}
