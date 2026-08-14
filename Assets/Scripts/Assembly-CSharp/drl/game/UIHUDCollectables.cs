using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIHUDCollectables : MonoBehaviour
	{
		public GameObject timeContainer;

		public Text timeMinField;

		public Text timeSecField;

		public Text timeMsField;

		public GameObject scoreContainer;

		public Text scoreField;

		public Text scoreTotalField;

		public FadeComponent fade;

		public Text speed;

		private int m_c_nms = -1;

		private int m_c_ns;

		private int m_c_nm;

		private string[] nsc3;

		private string[] nsc2;

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

		public void SetScore(int p_score)
		{
			scoreField.text = p_score.ToString("00");
		}

		public void SetScoreTotal(int p_score)
		{
			scoreTotalField.text = p_score.ToString();
		}

		public void Clear()
		{
			time = 0f;
			SetScore(0);
		}
	}
}
