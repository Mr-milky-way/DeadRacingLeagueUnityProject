using System;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DroneESC : DronePart
	{
		public float maxAmpere = 20f;

		[NonSerialized]
		public float temperature;

		public float input;

		public float legacyInput;

		public float voltage;

		public float amperes;

		[SerializeField]
		private DroneMotor m_motor;

		private bool m_hasMotor;

		public float output => input * voltage;

		public DroneMotor motor
		{
			get
			{
				if (m_hasMotor)
				{
					return m_motor;
				}
				if ((bool)m_motor)
				{
					m_hasMotor = true;
					return m_motor;
				}
				m_motor = Hierarchy.Find<DroneMotor>(base.transform);
				if ((bool)m_motor)
				{
					m_hasMotor = true;
					return m_motor;
				}
				return null;
			}
			set
			{
				m_motor = value;
				m_hasMotor = m_motor != null;
			}
		}

		public bool hasMotor => m_hasMotor;

		public override string GetPrefix()
		{
			return "E";
		}
	}
}
