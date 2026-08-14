using System;
using UnityEngine;
using thelab;

namespace drl.sim
{
	public class DroneBattery : DronePart
	{
		[Serializable]
		public class Cell
		{
			public AnimationCurve discharge = AnimationCurve.Linear(0f, 0f, 1f, 1f);

			private float m_min_cache = float.NaN;

			private float m_max_cache = float.NaN;

			public float capacity = 450f;

			public float resistance = 8f;

			public float mah;

			public float min
			{
				get
				{
					if (float.IsNaN(m_min_cache))
					{
						m_min_cache = ((discharge.keys.Length == 0) ? 0f : discharge.keys[0].value);
					}
					return m_min_cache;
				}
			}

			public float max
			{
				get
				{
					if (float.IsNaN(m_max_cache))
					{
						m_max_cache = ((discharge.keys.Length == 0) ? 0f : discharge.keys[discharge.keys.Length - 1].value);
					}
					return m_max_cache;
				}
			}

			public float voltage => discharge.Evaluate(ratio, p_cached: true);

			public float ratio
			{
				get
				{
					float num = capacity;
					if (!(num <= 0f))
					{
						return mah / num;
					}
					return 0f;
				}
			}

			public float GetVoltageDrop(float p_amperes)
			{
				return p_amperes * (resistance * 0.001f);
			}

			public void CacheCurves()
			{
				discharge.Cache(250);
				m_max_cache = float.NaN;
				m_min_cache = float.NaN;
			}
		}

		public Cell[] cells;

		public int rate = 50;

		private float m_max = float.NaN;

		private float m_min = float.NaN;

		private AnimationCurve m_discharge;

		private float m_resistance = float.NaN;

		private float m_cellResistance = float.NaN;

		private float m_capacity = float.NaN;

		protected float m_mah;

		public float max
		{
			get
			{
				if (float.IsNaN(m_max))
				{
					m_max = 0f;
					Cell[] array = cells;
					foreach (Cell cell in array)
					{
						m_max += cell.max;
					}
				}
				return m_max;
			}
		}

		public float min
		{
			get
			{
				if (float.IsNaN(m_min))
				{
					m_min = 0f;
					Cell[] array = cells;
					foreach (Cell cell in array)
					{
						m_min += cell.min;
					}
				}
				return m_min;
			}
		}

		public AnimationCurve discharge
		{
			get
			{
				if (m_discharge == null || m_discharge.keys.Length < 2)
				{
					m_discharge = new AnimationCurve(cells[0].discharge.keys);
					m_discharge.Cache(250);
				}
				return m_discharge;
			}
		}

		public float voltage => (float)cells.Length * discharge.Evaluate(ratio, p_cached: true);

		public float resistance
		{
			get
			{
				if (base.attached && base.drone.hasPhysics && !base.drone.physics.batterySag)
				{
					return 0.0001f;
				}
				if (float.IsNaN(m_resistance))
				{
					m_resistance = 0f;
					Cell[] array = cells;
					foreach (Cell cell in array)
					{
						m_resistance += cell.resistance;
					}
				}
				return m_resistance;
			}
			set
			{
				if (value != m_resistance)
				{
					m_resistance = ((value <= 0f) ? defaultResistance : value);
					m_cellResistance = ((cells.Length != 0) ? (m_resistance / (float)cells.Length) : 0f);
				}
			}
		}

		public float defaultResistance
		{
			get
			{
				float num = 0f;
				Cell[] array = cells;
				foreach (Cell cell in array)
				{
					num += cell.resistance;
				}
				return num;
			}
		}

		public float cellResistance
		{
			get
			{
				if (base.attached && base.drone.hasPhysics && !base.drone.physics.batterySag)
				{
					return 0.0001f;
				}
				if (float.IsNaN(m_cellResistance))
				{
					m_cellResistance = cells[0].resistance;
				}
				return m_resistance;
			}
			set
			{
				if (value != m_cellResistance)
				{
					m_cellResistance = ((value <= 0f) ? defaultCellResistance : value);
					m_resistance = m_cellResistance * (float)cells.Length;
				}
			}
		}

		public float defaultCellResistance => cells[0].resistance;

		public float capacity
		{
			get
			{
				if (float.IsNaN(m_capacity))
				{
					m_capacity = 0f;
					Cell[] array = cells;
					foreach (Cell cell in array)
					{
						m_capacity += cell.capacity;
					}
				}
				return m_capacity;
			}
			set
			{
				if (value != m_capacity)
				{
					float num = ratio;
					m_capacity = ((value <= 0f) ? defaultCapacity : value);
					mah = m_capacity * num;
				}
			}
		}

		public float defaultCapacity
		{
			get
			{
				float num = 0f;
				Cell[] array = cells;
				foreach (Cell cell in array)
				{
					num += cell.capacity;
				}
				return num;
			}
		}

		public float mah
		{
			get
			{
				if (base.attached && base.drone.hasPhysics && !base.drone.physics.batteryDrain)
				{
					return capacity;
				}
				return m_mah;
			}
			protected set
			{
				m_mah = value;
			}
		}

		public float ratio
		{
			get
			{
				if (base.attached && base.drone.hasPhysics && !base.drone.physics.batteryDrain)
				{
					return 1f;
				}
				if (!(capacity <= 0f))
				{
					return mah / capacity;
				}
				return 0f;
			}
		}

		public void CacheCellCurves()
		{
			for (int i = 0; i < cells.Length; i++)
			{
				cells[i].CacheCurves();
			}
		}

		public void Discharge(float p_amperes, float p_dt)
		{
			if (!base.attached || !base.drone.hasPhysics || base.drone.physics.batterySag)
			{
				mah = Mathf.Max(0f, mah - p_amperes * p_dt * (5f / 18f));
			}
		}

		public float GetVoltageDrop(float p_amperes)
		{
			return p_amperes * (resistance * 0.001f);
		}

		public void Recharge()
		{
			mah = capacity;
		}

		public override string GetPrefix()
		{
			return "B";
		}

		private void Start()
		{
			Recharge();
		}
	}
}
