using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class UIDroneSchematic : MonoBehaviour
	{
		public Text rpmLabel;

		public Text thrustLabel;

		public Text torqueLabel;

		public Text voltageLabel;

		public UIDroneMotorSchematic[] motors;

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
				for (int i = 0; i < motors.Length; i++)
				{
					motors[i].showRpm = value;
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
				for (int i = 0; i < motors.Length; i++)
				{
					motors[i].showThrust = value;
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
				for (int i = 0; i < motors.Length; i++)
				{
					motors[i].showTorque = value;
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
				for (int i = 0; i < motors.Length; i++)
				{
					motors[i].showVoltage = value;
				}
			}
		}

		public void SetRpm(int p_motor, float p_rpm, float p_rpmRatio)
		{
			if (p_motor >= 0 && p_motor <= motors.Length)
			{
				motors[p_motor].rpm = p_rpm;
				motors[p_motor].rpmRatio = p_rpmRatio;
			}
		}

		public void SetThrust(int p_motor, float p_thrust)
		{
			if (p_motor >= 0 && p_motor <= motors.Length)
			{
				motors[p_motor].thrust = p_thrust;
			}
		}

		public void SetTorque(int p_motor, float p_torque)
		{
			if (p_motor >= 0 && p_motor <= motors.Length)
			{
				motors[p_motor].torque = p_torque;
			}
		}

		public void SetVoltage(int p_motor, float p_voltage)
		{
			if (p_motor >= 0 && p_motor <= motors.Length)
			{
				motors[p_motor].voltage = p_voltage;
			}
		}
	}
}
