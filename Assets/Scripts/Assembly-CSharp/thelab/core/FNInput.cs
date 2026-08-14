using System;
using System.Collections.Generic;
using UnityEngine;
using drl.sim.rci;

namespace thelab.core
{
	[Serializable]
	public class FNInput : FNBatch
	{
		[Serializable]
		public class Trigger : FNTriggerElement
		{
			public enum Mode
			{
				Key = 0,
				Axis = 1,
				UI = 2,
				Button = 3
			}

			public Mode mode;

			public EventComponent uiTarget;

			public UIEventType uiEvent;

			public KeyCode key;

			public ConsoleButtons button;

			public string axis;

			[SerializeField]
			internal float[] m_range;

			[Range(-1f, 1f)]
			public float value;

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
				bool flag = false;
				switch (mode)
				{
				case Mode.Key:
					flag = ((key == KeyCode.None) ? Input.anyKey : Input.GetKey(key));
					value = (flag ? 1f : 0f);
					return flag;
				case Mode.Button:
					flag = ((button == ConsoleButtons.None) ? RCI.GetAnyButton() : RCI.GetButtonDown(button));
					value = (flag ? 1f : 0f);
					return flag;
				case Mode.Axis:
				{
					float num = Input.GetAxis(axis);
					value = Mathf.Clamp(num, -1f, 1f);
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
				case Mode.UI:
					if (!uiTarget)
					{
						return false;
					}
					switch (uiEvent)
					{
					case UIEventType.Down:
					case UIEventType.Up:
						flag = uiTarget.down;
						break;
					case UIEventType.Enter:
					case UIEventType.Exit:
						flag = uiTarget.over;
						break;
					}
					value = (flag ? 1f : 0f);
					return uiEvent switch
					{
						UIEventType.Down => uiTarget.down, 
						UIEventType.Up => uiTarget.down, 
						UIEventType.Enter => uiTarget.over, 
						UIEventType.Exit => uiTarget.over, 
						_ => false, 
					};
				default:
					return false;
				}
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
