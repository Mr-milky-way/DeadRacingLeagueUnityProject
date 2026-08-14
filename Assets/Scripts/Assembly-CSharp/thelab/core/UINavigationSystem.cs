using System;
using UnityEngine;
using drl.sim.rci;

namespace thelab.core
{
	public class UINavigationSystem : MonoBehaviour
	{
		public UINavigation focus;

		public float[] delay = new float[4] { 0.25f, 0.2f, 0.15f, 0.1f };

		public bool keepFocus;

		public bool unfocusOnMouseExit = true;

		public Component first;

		private float m_elapsed;

		private int m_step;

		public static Func<UINavigation> OnValidateFocus;

		public static void Focus(Transform p_target)
		{
			UINavigation f = null;
			Hierarchy.Traverse(p_target, delegate(UINavigation it)
			{
				if ((bool)f)
				{
					return false;
				}
				if (!it.isActiveAndEnabled)
				{
					return false;
				}
				f = it;
				return true;
			});
			if ((bool)f)
			{
				UINavigation.focus = f;
			}
		}

		public void Focus(Component p_target)
		{
			UINavigation.Focus(p_target);
		}

		protected virtual void Start()
		{
			m_elapsed = 0f;
			m_step = 0;
			if ((bool)first)
			{
				Focus(first);
			}
		}

		protected void Update()
		{
			if (!base.enabled)
			{
				return;
			}
			UINavigation f = UINavigation.focus;
			focus = f;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			if (IsNavigationUp())
			{
				flag = true;
				flag2 = true;
			}
			if (IsNavigationDown())
			{
				flag = true;
				flag3 = true;
			}
			if (IsNavigationLeft())
			{
				flag = true;
				flag4 = true;
			}
			if (IsNavigationRight())
			{
				flag = true;
				flag5 = true;
			}
			if (IsClick())
			{
				flag = true;
				flag6 = true;
			}
			if (!flag)
			{
				m_step = 0;
			}
			int num = Mathf.Clamp(m_step, 0, delay.Length - 1);
			float num2 = ((num < 0) ? 0.2f : delay[num]);
			if (IsTransmitter())
			{
				num2 = 0.2f;
			}
			if (keepFocus && !unfocusOnMouseExit && !f)
			{
				m_elapsed = num2;
				f = Hierarchy.Find<UINavigation>(base.transform);
				if ((bool)f)
				{
					f.Focus();
				}
				return;
			}
			m_elapsed += Time.unscaledDeltaTime;
			if (m_elapsed < num2)
			{
				return;
			}
			if (flag && f == null)
			{
				this.TimerRunOnce(delegate
				{
					if (!(UINavigation.focus != null))
					{
						if (OnValidateFocus != null)
						{
							f = OnValidateFocus();
						}
						if (!f)
						{
							UINavigation.RestoreLastFocus();
							f = UINavigation.focus;
						}
						else
						{
							UINavigation.focus = f;
						}
						if (!f)
						{
							UINavigation uINavigation = ((OnValidateFocus == null) ? null : OnValidateFocus());
							if (uINavigation != null)
							{
								f = uINavigation;
							}
						}
					}
				}, 0.05f);
			}
			if (f != null)
			{
				if (flag2)
				{
					f.FocusUp();
					m_elapsed = 0f;
					m_step++;
				}
				if (flag3)
				{
					f.FocusDown();
					m_elapsed = 0f;
					m_step++;
				}
				if (flag4)
				{
					f.FocusLeft();
					m_elapsed = 0f;
					m_step++;
				}
				if (flag5)
				{
					f.FocusRight();
					m_elapsed = 0f;
					m_step++;
				}
				if (flag6)
				{
					f.Click();
					m_elapsed = 0f;
					m_step++;
				}
			}
		}

		protected virtual bool IsNavigationUp()
		{
			if (Input.GetKey(KeyCode.UpArrow))
			{
				return true;
			}
			return RCI.GetAxisTrigger(RawAxis.LeftStickY, isPositiveSign: true);
		}

		protected virtual bool IsNavigationDown()
		{
			if (Input.GetKey(KeyCode.DownArrow))
			{
				return true;
			}
			return RCI.GetAxisTrigger(RawAxis.LeftStickY, isPositiveSign: false);
		}

		protected virtual bool IsNavigationLeft()
		{
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				return true;
			}
			return RCI.GetAxisTrigger(RawAxis.RightStickX, isPositiveSign: false);
		}

		protected virtual bool IsNavigationRight()
		{
			if (Input.GetKey(KeyCode.RightArrow))
			{
				return true;
			}
			return RCI.GetAxisTrigger(RawAxis.RightStickX, isPositiveSign: true);
		}

		protected virtual bool IsTransmitter()
		{
			return RCI.IsUsingTransmitterSettings();
		}

		protected virtual bool IsClick()
		{
			if (!Input.GetKeyUp(KeyCode.JoystickButton0))
			{
				return Input.GetKeyUp(KeyCode.Return);
			}
			return true;
		}
	}
}
