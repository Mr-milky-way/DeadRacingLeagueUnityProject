using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIHUDTimeout : MonoBehaviour
	{
		public GameObject countdownContainer;

		public Text cntMinField;

		public Text cntSecField;

		public Text cntTimeoutField;

		public GameObject timeoutContainer;

		public Text tmtMinField;

		public Text tmtSecField;

		public FadeComponent fade;

		private Activity m_timerActivity;

		public float offset;

		private StringBuilder m_sb = new StringBuilder();

		private float m_countdown;

		private float m_timeout;

		public float countdown
		{
			get
			{
				return m_countdown;
			}
			set
			{
				m_countdown = value;
				m_countdown += offset;
				TimeSpan timeSpan = new TimeSpan(0, 0, (int)m_countdown);
				countdownContainer.SetActive(m_countdown > 0f);
				if (m_countdown <= 0f)
				{
					m_countdown = 0f;
					return;
				}
				m_sb.Clear();
				int num = Mathf.FloorToInt((float)timeSpan.TotalDays);
				int num2 = Mathf.FloorToInt(timeSpan.Hours);
				if (num > 0)
				{
					if (num2 > 0)
					{
						m_sb.Append(Format.IntToString(num, p_lead_zero: false));
						m_sb.Append("D ");
						m_sb.Append(Format.IntToString(num2));
						m_sb.Append("H");
					}
					else
					{
						m_sb.Append(Format.IntToString(num, p_lead_zero: false));
						m_sb.Append(" DAY");
						if (num > 1)
						{
							m_sb.Append("S");
						}
					}
				}
				else
				{
					int n = Mathf.FloorToInt(timeSpan.Minutes);
					m_sb.Append(Format.IntToString(num2));
					m_sb.Append(":");
					m_sb.Append(Format.IntToString(n));
					m_sb.Append(":");
					m_sb.Append(Format.IntToString(timeSpan.Seconds));
				}
				cntTimeoutField.text = m_sb.ToString();
			}
		}

		public float timeout
		{
			get
			{
				return m_timeout;
			}
			set
			{
				m_timeout = value;
				timeoutContainer.SetActive(m_timeout > 0f);
				if (m_timeout < 0f)
				{
					m_timeout = 0f;
				}
				string[] array = Format.SecondsToTime(m_timeout, 2, p_use_ms: true).Split(':');
				tmtMinField.text = array[0];
				tmtSecField.text = array[1];
			}
		}

		public void StartCountdown(float p_time)
		{
			if (p_time <= 0f)
			{
				return;
			}
			fade.FadeIn(0f);
			countdown = p_time;
			float c = p_time;
			StopTimer();
			m_timerActivity = ((Component)this).TimerRun((Action)delegate
			{
				countdown = c;
				c -= Time.unscaledDeltaTime;
				if (c <= 0f)
				{
					StopCountdown();
				}
			}, p_time, 0f);
		}

		public void StartTimeout(float p_time)
		{
			if (p_time <= 0f)
			{
				return;
			}
			fade.FadeIn(0f);
			timeout = p_time;
			float c = p_time;
			StopTimer();
			m_timerActivity = ((Component)this).TimerRun((Action)delegate
			{
				timeout = c;
				c -= Time.unscaledDeltaTime;
				if (c <= 0f)
				{
					StopTimeout();
				}
			}, p_time, 0f);
		}

		private void StopTimer()
		{
			if (m_timerActivity != null)
			{
				m_timerActivity.Stop();
				m_timerActivity = null;
			}
		}

		public void StopCountdown()
		{
			StopTimer();
			countdown = -1f;
			fade.FadeOut(0f);
		}

		public void StopTimeout()
		{
			StopTimer();
			timeout = -1f;
			fade.FadeOut(0f);
		}
	}
}
