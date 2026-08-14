using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	[HideInInspector]
	public class FNBatch : FlowNode
	{
		public BatchCriteria criteria;

		[SerializeField]
		private List<int> m_completed;

		protected int m_current_element;

		public List<int> completed
		{
			get
			{
				if (m_completed != null)
				{
					return m_completed;
				}
				return m_completed = new List<int>();
			}
			set
			{
				m_completed = value;
			}
		}

		internal override bool hasContent => true;

		protected virtual int GetCount()
		{
			return 0;
		}

		public virtual bool IsComplete(int p_index)
		{
			return true;
		}

		public virtual void SetComplete(int p_index, bool p_flag)
		{
		}

		protected virtual void UpdateItem(int p_index)
		{
		}

		public virtual object GetItem(int p_index)
		{
			return null;
		}

		internal override void OnInitialize()
		{
			completed.Clear();
		}

		public virtual void Reset()
		{
			m_current_element = 0;
		}

		public int GetNextTrigger()
		{
			GetCount();
			switch (criteria)
			{
			case BatchCriteria.All:
				return -1;
			case BatchCriteria.Any:
				return -1;
			case BatchCriteria.Forward:
			case BatchCriteria.Backward:
				return m_current_element;
			default:
				return -1;
			}
		}

		internal override FlowStatus OnUpdate()
		{
			int count = GetCount();
			if (completed.Count < count)
			{
				for (int i = 0; i < count; i++)
				{
					if (!completed.Contains(i) && IsComplete(i) && !completed.Contains(i))
					{
						completed.Add(i);
						m_current_element++;
					}
				}
			}
			switch (criteria)
			{
			case BatchCriteria.All:
			case BatchCriteria.Any:
			{
				for (int j = 0; j < count; j++)
				{
					if (!completed.Contains(j))
					{
						UpdateItem(j);
					}
				}
				break;
			}
			case BatchCriteria.Forward:
			case BatchCriteria.Backward:
				UpdateItem((criteria == BatchCriteria.Backward) ? (completed.Count - 1 - m_current_element) : m_current_element);
				break;
			}
			switch (criteria)
			{
			case BatchCriteria.Any:
				if (completed.Count >= 1)
				{
					return FlowStatus.Complete;
				}
				break;
			case BatchCriteria.All:
				if (completed.Count >= count)
				{
					return FlowStatus.Complete;
				}
				break;
			case BatchCriteria.Forward:
			case BatchCriteria.Backward:
			{
				for (int k = 0; k < completed.Count; k++)
				{
					int num = ((criteria == BatchCriteria.Backward) ? (completed.Count - 1 - k) : k);
					if (num != completed[k])
					{
						SetComplete(num, p_flag: false);
						completed.RemoveAt(k--);
					}
				}
				if (completed.Count >= count)
				{
					return FlowStatus.Complete;
				}
				break;
			}
			}
			return FlowStatus.Running;
		}
	}
}
