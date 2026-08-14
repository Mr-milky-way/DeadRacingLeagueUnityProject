using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class UIFlareProgressGroup : MonoBehaviour
	{
		public List<UIFlareProgressItem> list;

		private FadeComponent m_fade;

		private MonoActivity m_fade_timer;

		private MonoActivity m_blink_timer;

		public FadeComponent fade
		{
			get
			{
				if (!m_fade)
				{
					return m_fade = GetComponent<FadeComponent>();
				}
				return m_fade;
			}
		}

		public void Clear()
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Clear();
			}
			if (m_fade_timer != null)
			{
				m_fade_timer.Stop();
			}
		}

		public void SetProgress(float p_progress)
		{
			Clear();
			float num = Mathf.Clamp(p_progress, 0f, list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				UIFlareProgressItem uIFlareProgressItem = list[i];
				if ((bool)uIFlareProgressItem)
				{
					if (num <= 0f)
					{
						break;
					}
					if (num >= 1f)
					{
						uIFlareProgressItem.SetProgress(1f);
					}
					else
					{
						num = Mathf.Round(num * 10f) / 10f;
						num = ((num < 0.5f) ? 0f : 0.5f);
						uIFlareProgressItem.FadeProgress(num);
					}
					num -= 1f;
				}
			}
			num = Mathf.Floor(p_progress);
			if (num >= (float)list.Count)
			{
				Blink(0f, 999999f);
			}
		}

		public void FadeProgress(float p_progress, float p_delay_step, bool p_clear = true)
		{
			if (p_clear)
			{
				Clear();
			}
			float num = Mathf.Clamp(p_progress, 0f, list.Count);
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				UIFlareProgressItem uIFlareProgressItem = list[i];
				if ((bool)uIFlareProgressItem)
				{
					if (num <= 0f)
					{
						break;
					}
					if (num >= 1f)
					{
						num4 = uIFlareProgressItem.FadeProgress(1f, num2, p_clear);
					}
					else
					{
						num = Mathf.Round(num * 10f) / 10f;
						num = ((num < 0.5f) ? 0f : 0.5f);
						num4 = uIFlareProgressItem.FadeProgress(num, num2, p_clear);
					}
					num2 += p_delay_step;
					num -= 1f;
				}
			}
			num3 = num4 + num2 - p_delay_step;
			num = Mathf.Floor(p_progress);
			if (m_blink_timer != null)
			{
				m_blink_timer.Stop();
			}
			if (num >= (float)list.Count)
			{
				m_blink_timer = this.MonoActivityRunOnce(delegate
				{
					Blink(0f, 999999f, 0f, p_clear);
				}, num3 + 0.25f);
			}
		}

		public void FadeProgress(float p_progress, float p_delay, float p_delay_step, bool p_clear = true)
		{
			m_fade_timer = this.MonoActivityRunOnce(delegate
			{
				FadeProgress(p_progress, p_delay_step, p_clear);
			}, p_delay);
		}

		public void Blink(float p_delay = 0f, float p_duration = 0f, float p_delay_step = 0f, bool p_clear = true)
		{
			float num = p_delay;
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i])
				{
					list[i].Blink(num, p_duration, p_clear);
					num += p_delay_step;
				}
			}
		}
	}
}
