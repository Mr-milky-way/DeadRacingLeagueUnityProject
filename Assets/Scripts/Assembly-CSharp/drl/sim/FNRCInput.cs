using System;
using System.Collections.Generic;
using UnityEngine;
using drl.sim.rci;
using thelab.core;

namespace drl.sim
{
	[Serializable]
	public class FNRCInput : FNBatch
	{
		[Serializable]
		public class Trigger : FNTriggerElement
		{
			public enum Mode
			{
				Assigned = 0,
				Raw = 1
			}

			public Mode mode;

			public AssignedAxis axisAssigned;

			public RawAxis axisRaw;

			[SerializeField]
			internal float[] m_range;

			public float[] range
			{
				get
				{
					if (m_range != null)
					{
						return m_range;
					}
					return m_range = new float[2] { -1f, 1f };
				}
			}

			protected override bool IsOn()
			{
				Mode mode = this.mode;
				if ((uint)mode <= 1u)
				{
					float num = ((this.mode == Mode.Assigned) ? RCI.GetAssignedAxis(axisAssigned) : RCI.GetRawAxis(axisRaw));
					float num2 = range[0];
					float num3 = range[1];
					if (num < num2)
					{
						return false;
					}
					if (num > num3)
					{
						return false;
					}
					return true;
				}
				return false;
			}
		}

		[SerializeField]
		private List<Trigger> m_triggers;

		public List<Trigger> triggers
		{
			get
			{
				if (m_triggers != null)
				{
					return m_triggers;
				}
				return m_triggers = new List<Trigger>();
			}
		}

		protected override int GetCount()
		{
			return triggers.Count;
		}

		internal override void OnInitialize()
		{
			base.OnInitialize();
			for (int i = 0; i < triggers.Count; i++)
			{
				triggers[i].Reset();
			}
		}

		protected override void UpdateItem(int p_index)
		{
			if (p_index >= 0 && p_index < triggers.Count)
			{
				triggers[p_index]?.Update();
			}
		}

		public override object GetItem(int p_index)
		{
			if (p_index < 0)
			{
				return null;
			}
			if (p_index >= triggers.Count)
			{
				return null;
			}
			return triggers[p_index];
		}

		public override bool IsComplete(int p_index)
		{
			if (p_index < 0)
			{
				return false;
			}
			if (p_index >= triggers.Count)
			{
				return false;
			}
			return triggers[p_index]?.completed ?? false;
		}
	}
}
