using UnityEngine;

namespace drl.sim
{
	public class DSElectrical : DroneSensor
	{
		[SerializeField]
		public float m_currentDraw;

		[SerializeField]
		public float m_currentMax;

		[SerializeField]
		public float m_voltage;

		[SerializeField]
		public float m_voltageAvailable;

		[SerializeField]
		public float m_voltageMax;

		[SerializeField]
		public float m_voltageMin;

		[SerializeField]
		public float m_totalCapacity;

		[SerializeField]
		public float m_remainingCharge;

		public float currentDraw
		{
			get
			{
				if (!base.enabled)
				{
					return 0f;
				}
				return m_currentDraw;
			}
		}

		public float currentMax
		{
			get
			{
				if (!base.enabled)
				{
					return 0f;
				}
				return m_currentMax;
			}
		}

		public float voltage
		{
			get
			{
				if (!base.enabled)
				{
					return 0f;
				}
				return m_voltage;
			}
		}

		public float voltageAvailable
		{
			get
			{
				if (!base.enabled)
				{
					return 0f;
				}
				return m_voltageAvailable;
			}
		}

		public float voltageMax
		{
			get
			{
				if (!base.enabled)
				{
					return 0f;
				}
				return m_voltageMax;
			}
		}

		public float voltageMin
		{
			get
			{
				if (!base.enabled)
				{
					return 0f;
				}
				return m_voltageMin;
			}
		}

		public float totalCapacity
		{
			get
			{
				if (!base.enabled)
				{
					return 0f;
				}
				return m_totalCapacity;
			}
		}

		public float remainingCharge
		{
			get
			{
				if (!base.enabled)
				{
					return 0f;
				}
				return m_remainingCharge;
			}
		}

		protected override void OnInitialize()
		{
		}

		protected override void Refresh(float p_dt)
		{
		}
	}
}
