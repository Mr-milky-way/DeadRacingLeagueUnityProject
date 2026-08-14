using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class SwitcherComponent : MonoBehaviour
	{
		public List<Component> targets;

		[SerializeField]
		private WrapMode m_wrap = WrapMode.Once;

		[SerializeField]
		private int m_index;

		public float duration = 0.2f;

		public Component current => Get<Component>(index);

		public WrapMode wrap
		{
			get
			{
				return m_wrap;
			}
			set
			{
				m_wrap = value;
				Refresh();
			}
		}

		public int index
		{
			get
			{
				return m_index;
			}
			set
			{
				m_index = value;
				Refresh();
			}
		}

		public T GetCurrent<T>() where T : Component
		{
			return Get<T>(index);
		}

		public T Get<T>(int p_index) where T : Component
		{
			if (p_index >= 0)
			{
				if (p_index < targets.Count)
				{
					return targets[p_index].GetComponent<T>();
				}
				return null;
			}
			return null;
		}

		protected void Awake()
		{
			Refresh(p_force: true);
		}

		protected void Refresh(bool p_force = false)
		{
			int num = Mathf.Max(0, m_index);
			int count = targets.Count;
			if (count <= 1)
			{
				num = Mathf.Clamp(num, 0, count - 1);
			}
			if (count > 1)
			{
				switch (wrap)
				{
				case WrapMode.Default:
				case WrapMode.Once:
				case WrapMode.ClampForever:
					num = Mathf.Clamp(num, 0, count - 1);
					break;
				case WrapMode.Loop:
					num %= count;
					break;
				case WrapMode.PingPong:
					num %= 2 * (count - 1);
					if (num >= count)
					{
						num = count - num % count - 2;
					}
					break;
				}
			}
			if (num >= count)
			{
				Debug.LogWarning("SwitcherComponent> [" + base.name + "] index out of range [" + num + "/" + count + "]");
			}
			else
			{
				for (int i = 0; i < count; i++)
				{
					Component p_target = targets[i];
					SetEnabled(p_target, i == num, p_force ? 0f : duration);
				}
			}
		}

		protected void SetEnabled(Component p_target, bool p_flag, float p_duration)
		{
			if (!p_target)
			{
				return;
			}
			if (p_target is FadeComponent)
			{
				(p_target as FadeComponent).Fade(p_flag ? 1f : (-0.1f), p_duration);
			}
			else if (p_target is FadeSlideComponent)
			{
				FadeSlideComponent fadeSlideComponent = p_target as FadeSlideComponent;
				if (p_flag)
				{
					fadeSlideComponent.FadeIn(p_duration);
				}
				else
				{
					fadeSlideComponent.FadeOut(p_duration);
				}
			}
			else if (p_target is FadeResizeComponent)
			{
				(p_target as FadeResizeComponent).Fade(p_flag ? 1f : (-0.1f), p_duration);
			}
			else if (p_target is Graphic)
			{
				(p_target as Graphic).enabled = p_flag;
			}
			else if (p_target is Behaviour)
			{
				(p_target as Behaviour).enabled = p_flag;
			}
			else
			{
				p_target.gameObject.SetActive(p_flag);
			}
		}
	}
}
