using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class FNProcess : FNBatch
	{
		[SerializeField]
		private List<Flow> m_targets;

		public List<Flow> targets
		{
			get
			{
				if (m_targets != null)
				{
					return m_targets;
				}
				return m_targets = new List<Flow>();
			}
		}

		internal override void OnInitialize()
		{
			base.OnInitialize();
			for (int i = 0; i < targets.Count; i++)
			{
				Flow flow = targets[i];
				if ((bool)flow)
				{
					flow.Reset();
					flow.Run();
				}
			}
		}

		protected override int GetCount()
		{
			return targets.Count;
		}

		public override bool IsComplete(int p_index)
		{
			if (p_index < 0)
			{
				return false;
			}
			if (p_index >= targets.Count)
			{
				return false;
			}
			Flow flow = targets[p_index];
			if (!flow)
			{
				return false;
			}
			return flow.complete;
		}
	}
}
