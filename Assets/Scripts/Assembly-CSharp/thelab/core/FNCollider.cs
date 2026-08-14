using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class FNCollider : FNBatch
	{
		[Serializable]
		public class Trigger : FNTriggerElement
		{
			public enum Mode
			{
				Collision = 0,
				Trigger = 1
			}

			public Mode mode;

			public ColliderEventComponent target;

			public bool hit;

			protected bool m_has_events;

			internal override void Initialize()
			{
				if ((bool)target)
				{
					m_on = target.hit;
					base.on = (hit = target.hit);
					base.Initialize();
					SetupEvents();
				}
			}

			protected void SetupEvents()
			{
				if (!m_has_events)
				{
					m_has_events = true;
					if ((bool)target)
					{
						target.callback.AddListener(OnColliderEvent);
					}
				}
			}

			public void ClearEvents()
			{
				m_has_events = false;
				if ((bool)target)
				{
					target.callback.RemoveAllListeners();
				}
			}

			protected void OnColliderEvent(ColliderEvent p_event)
			{
				switch (p_event.type)
				{
				case ColliderEvent.Type.Enter:
					base.on = (hit = true);
					break;
				case ColliderEvent.Type.Exit:
					base.on = (hit = false);
					break;
				}
			}

			protected override bool IsOn()
			{
				return hit;
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

		public void LoadFromContainer(Transform p_container, Trigger.Mode p_mode, FNTriggerType p_type)
		{
			if (!p_container)
			{
				return;
			}
			triggers.Clear();
			int childCount = p_container.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = p_container.GetChild(i);
				if (!child)
				{
					continue;
				}
				ColliderEventComponent colliderEventComponent = child.GetComponent<ColliderEventComponent>();
				FNColliderElementHolder component = child.GetComponent<FNColliderElementHolder>();
				if ((bool)component)
				{
					colliderEventComponent = component.targetCollider;
				}
				if (!colliderEventComponent)
				{
					ColliderEventComponent[] componentsInChildren = child.GetComponentsInChildren<ColliderEventComponent>();
					foreach (ColliderEventComponent colliderEventComponent2 in componentsInChildren)
					{
						Collider component2 = colliderEventComponent2.GetComponent<Collider>();
						if (component2 != null && component2.isTrigger)
						{
							colliderEventComponent = colliderEventComponent2;
							break;
						}
					}
				}
				Trigger trigger = new Trigger();
				trigger.mode = p_mode;
				trigger.target = colliderEventComponent;
				trigger.type = p_type;
				triggers.Add(trigger);
			}
		}

		public void LoadFromContainer(Transform p_container, Trigger.Mode p_mode)
		{
			LoadFromContainer(p_container, p_mode, FNTriggerType.SwitchOn);
		}

		public void LoadFromContainer(Transform p_container)
		{
			LoadFromContainer(p_container, Trigger.Mode.Trigger);
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

		protected override void UpdateItem(int p_index)
		{
			if (p_index >= 0 && p_index < triggers.Count)
			{
				triggers[p_index]?.Update();
			}
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

		public override void Reset()
		{
			base.completed.Clear();
			base.Reset();
			base.completed = new List<int>();
			for (int i = 0; i < triggers.Count; i++)
			{
				triggers[i].hit = false;
				triggers[i].Reset();
				triggers[i].ClearEvents();
				if ((bool)triggers[i].target)
				{
					triggers[i].target.data = new ColliderEventComponent.Data();
					triggers[i].target.hit = false;
				}
			}
		}

		public override void SetComplete(int p_index, bool p_flag)
		{
			if (p_index >= 0 && p_index < triggers.Count)
			{
				Trigger trigger = triggers[p_index];
				if (trigger != null)
				{
					trigger.completed = p_flag;
				}
			}
		}
	}
}
