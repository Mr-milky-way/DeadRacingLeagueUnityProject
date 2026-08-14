using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIHUDRaceLayer : MonoBehaviour
	{
		public GameObject timeContainer;

		public Text timeMinField;

		public Text timeSecField;

		public Text timeMsField;

		public GameObject positionContainer;

		public Text positionCountField;

		public Text positionTotalField;

		public GameObject lapContainer;

		public Text lapCountField;

		public Text lapTotalField;

		public GameObject gateContainer;

		public Text gateCountField;

		public Text gateTotalField;

		public FadeComponent fade;

		public GameObject promo;

		[HideInInspector]
		public bool raceStatsEnabled;

		private bool m_position_enabled;

		private int m_position_total;

		private int m_c_nms = -1;

		private int m_c_ns;

		private int m_c_nm;

		public Text speed;

		private string[] nsc3;

		private string[] nsc2;

		public bool positionEnabled
		{
			get
			{
				return m_position_enabled;
			}
			set
			{
				m_position_enabled = value;
				bool active = m_position_enabled && m_position_total > 1;
				positionContainer.SetActive(active);
			}
		}

		public float time
		{
			set
			{
				if (value <= 0f)
				{
					m_c_nms = -1;
					m_c_ns = -1;
					m_c_nm = -1;
				}
				int num = Mathf.FloorToInt(value * 1000f) % 1000;
				int num2 = Mathf.FloorToInt(value) % 60;
				int num3 = Mathf.FloorToInt(value / 60f) % 60;
				if (num != m_c_nms)
				{
					m_c_nms = num;
					timeMsField.text = GetCachedNumberString(num, 3);
				}
				if (num2 != m_c_ns)
				{
					m_c_ns = num2;
					timeSecField.text = GetCachedNumberString(num2, 2);
				}
				if (num3 != m_c_nm)
				{
					m_c_nm = num3;
					timeMinField.text = GetCachedNumberString(num3, 2);
				}
			}
		}

		public string GetCachedNumberString(int p_value, int p_digits)
		{
			if (nsc3 == null)
			{
				nsc3 = new string[1000];
				for (int i = 0; i < nsc3.Length; i++)
				{
					nsc3[i] = i.ToString("000");
				}
			}
			if (nsc2 == null)
			{
				nsc2 = new string[100];
				for (int j = 0; j < nsc2.Length; j++)
				{
					nsc2[j] = j.ToString("00");
				}
			}
			return p_digits switch
			{
				3 => nsc3[p_value], 
				2 => nsc2[p_value], 
				_ => "", 
			};
		}

		public void SetSpeed(float p_kph)
		{
			int p_value = Mathf.RoundToInt(p_kph);
			speed.text = GetCachedNumberString(p_value, 3);
		}

		public void SetPosition(int p_current, int p_total)
		{
			positionCountField.text = p_current.ToString("00");
			positionTotalField.text = p_total.ToString("00");
			positionContainer.SetActive(p_total > 1 && positionEnabled);
		}

		public void SetLap(int p_current, int p_total)
		{
			lapCountField.text = p_current.ToString("00");
			lapTotalField.text = p_total.ToString("00");
			m_position_total = p_total;
			lapContainer.SetActive(raceStatsEnabled && p_total > 1);
		}

		public void SetGate(int p_current, int p_total)
		{
			gateCountField.text = ((p_total > 99) ? p_current.ToString("000") : p_current.ToString("00"));
			gateTotalField.text = ((p_total > 99) ? p_total.ToString("000") : p_total.ToString("00"));
			gateContainer.SetActive(p_total > 0);
		}

		public void SetPromo(bool p_flag)
		{
			if ((bool)promo)
			{
				promo.SetActive(p_flag);
			}
		}

		public void Clear()
		{
			time = 0f;
			SetLap(0, 0);
			SetGate(0, 0);
			SetPosition(0, 0);
			SetPromo(p_flag: false);
		}
	}
}
