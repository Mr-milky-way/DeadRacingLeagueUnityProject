using System;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	[HideInInspector]
	public class FlowNode : MonoBehaviour
	{
		public Flow flow;

		public FlowStatus status;

		public string label;

		[SerializeField]
		[HideInInspector]
		private string m_guid;

		public bool finished
		{
			get
			{
				if (status != FlowStatus.Complete)
				{
					return status == FlowStatus.Fail;
				}
				return true;
			}
		}

		internal virtual bool hasContent => false;

		internal string guid
		{
			get
			{
				if (!string.IsNullOrEmpty(m_guid))
				{
					return m_guid;
				}
				return m_guid = GetHashCode().ToString("x6");
			}
		}

		internal void Step()
		{
			switch (status)
			{
			case FlowStatus.Idle:
				OnInitialize();
				status = FlowStatus.Running;
				break;
			case FlowStatus.Running:
				status = (flow.skipHandler.Skip ? OnSkip() : OnUpdate());
				if (status == FlowStatus.Complete)
				{
					OnComplete();
				}
				if (status == FlowStatus.Fail)
				{
					OnFail();
				}
				break;
			}
		}

		internal virtual void OnInitialize()
		{
		}

		internal virtual FlowStatus OnUpdate()
		{
			return FlowStatus.Complete;
		}

		internal virtual void OnComplete()
		{
		}

		internal virtual void OnFail()
		{
		}

		internal virtual void OnMessage(string p_event, params object[] p_data)
		{
		}

		public virtual FlowStatus OnSkip()
		{
			return OnUpdate();
		}
	}
}
