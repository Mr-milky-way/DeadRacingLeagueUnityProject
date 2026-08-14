using System;
using System.Collections.Generic;
using UnityEngine;
using drl.sim;

namespace thelab.core
{
	[Serializable]
	public class FNNotification : FNBatch
	{
		[Serializable]
		public class Trigger : FNTriggerElement
		{
			public enum Action
			{
				Send = 0,
				Receive = 1
			}

			public SimulationFlowModule module;

			public Action mode;

			public string notification;

			private string m_incomingNotification = "";

			protected override bool IsOn()
			{
				return false;
			}

			internal override void Update()
			{
				if (completed)
				{
					return;
				}
				switch (mode)
				{
				case Action.Receive:
					if (m_incomingNotification == notification)
					{
						completed = true;
					}
					break;
				case Action.Send:
					completed = true;
					break;
				}
			}

			public override void Reset()
			{
				base.Reset();
				m_incomingNotification = "";
			}

			public void SetIncomingNotification(string p_notification)
			{
				m_incomingNotification = p_notification;
			}
		}

		[SerializeField]
		private SimulationFlowModule m_module;

		[SerializeField]
		private List<Trigger> m_triggers;

		[HideInInspector]
		public SimulationFlowModule module
		{
			get
			{
				if (!m_module)
				{
					return Hierarchy.FindReverse<SimulationFlowModule>(base.transform);
				}
				return m_module;
			}
		}

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
				triggers[i].module = module;
			}
		}

		internal override void OnMessage(string p_event, params object[] p_data)
		{
			for (int i = 0; i < triggers.Count; i++)
			{
				triggers[i].SetIncomingNotification(p_event);
			}
		}

		protected override void UpdateItem(int p_index)
		{
			if (p_index < 0 || p_index >= triggers.Count)
			{
				return;
			}
			Trigger trigger = triggers[p_index];
			if (trigger != null)
			{
				if (trigger.mode == Trigger.Action.Send)
				{
					module.ui.Notify(trigger.notification, 0f);
				}
				trigger.Update();
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
