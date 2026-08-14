using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class UIDroneMotorSchematic : MonoBehaviour
	{
		public Text rpmLabel;

		public Text thrustLabel;

		public Text torqueLabel;

		public Text voltageLabel;

		public Image rpmChart;

		private float m_rpm = float.MaxValue;

		private float m_thrust = float.MaxValue;

		private float m_torque = float.MaxValue;

		private float m_voltage = float.MaxValue;

		private float m_rpmRatio = float.MaxValue;

		public bool showRpm
		{
			get
			{
				if (rpmLabel == null)
				{
					return false;
				}
				return rpmLabel.enabled;
			}
			set
			{
				if (rpmLabel != null)
				{
					rpmLabel.enabled = value;
				}
			}
		}

		public bool showThrust
		{
			get
			{
				if (thrustLabel == null)
				{
					return false;
				}
				return thrustLabel.enabled;
			}
			set
			{
				if (thrustLabel != null)
				{
					thrustLabel.enabled = value;
				}
			}
		}

		public bool showTorque
		{
			get
			{
				if (torqueLabel == null)
				{
					return false;
				}
				return torqueLabel.enabled;
			}
			set
			{
				if (torqueLabel != null)
				{
					torqueLabel.enabled = value;
				}
			}
		}

		public bool showVoltage
		{
			get
			{
				if (voltageLabel == null)
				{
					return false;
				}
				return voltageLabel.enabled;
			}
			set
			{
				if (voltageLabel != null)
				{
					voltageLabel.enabled = value;
				}
			}
		}

		public float rpm
		{
			get
			{
				return m_rpm;
			}
			set
			{
				if (rpmLabel != null && rpmLabel.enabled && m_rpm != value)
				{
					rpmLabel.text = FormatNumber(Mathf.Abs(value), 0);
				}
				m_rpm = Mathf.Abs(value);
			}
		}

		public float thrust
		{
			get
			{
				return m_thrust;
			}
			set
			{
				if (thrustLabel != null && thrustLabel.enabled && m_thrust != value)
				{
					thrustLabel.text = FormatNumber(Mathf.Abs(value), 0);
				}
				m_thrust = Mathf.Abs(value);
			}
		}

		public float torque
		{
			get
			{
				return m_torque;
			}
			set
			{
				if (torqueLabel != null && torqueLabel.enabled && m_torque != value)
				{
					torqueLabel.text = FormatNumber(value, 2);
				}
				m_torque = value;
			}
		}

		public float voltage
		{
			get
			{
				return m_voltage;
			}
			set
			{
				if (voltageLabel != null && voltageLabel.enabled && m_voltage != value)
				{
					voltageLabel.text = FormatNumber(value, 1);
				}
				m_voltage = value;
			}
		}

		public float rpmRatio
		{
			get
			{
				return m_rpmRatio;
			}
			set
			{
				if (rpmChart != null && rpmChart.enabled && m_rpmRatio != value)
				{
					rpmChart.fillAmount = value;
				}
				m_rpmRatio = value;
			}
		}

		public string FormatNumber(float p_value, int p_decimals)
		{
			if (p_decimals < 1)
			{
				return ((int)p_value).ToString();
			}
			switch (p_decimals)
			{
			case 1:
				return ((float)(int)(p_value * 10f) * 0.1f).ToString();
			case 2:
				return ((float)(int)(p_value * 100f) * 0.01f).ToString();
			case 3:
				return ((float)(int)(p_value * 1000f) * 0.001f).ToString();
			case 4:
				return ((float)(int)(p_value * 10000f) * 0.0001f).ToString();
			default:
			{
				int num = (int)Mathf.Pow(10f, p_decimals);
				return ((float)(int)(p_value * (float)num) * (1f / (float)num)).ToString();
			}
			}
		}
	}
}
