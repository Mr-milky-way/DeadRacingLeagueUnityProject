using System;
using UnityEngine;

namespace thelab.core
{
	public class UIScreen : Container
	{
		public enum Mode
		{
			Alpha = 0,
			Animation = 1,
			Custom = 2
		}

		[SerializeField]
		[HideInInspector]
		private float m_transition;

		public bool open;

		[HideInInspector]
		public Mode mode;

		[HideInInspector]
		public string title;

		[HideInInspector]
		public int order;

		private Animation m_animation;

		[HideInInspector]
		public AnimationClip clip;

		[HideInInspector]
		public ScreenEventCallback OnEvent;

		public float transition
		{
			get
			{
				return m_transition;
			}
			set
			{
				m_transition = value;
				OnTransition(m_transition);
			}
		}

		public Animation animation
		{
			get
			{
				if (!m_animation)
				{
					return m_animation = GetComponent<Animation>();
				}
				return m_animation;
			}
		}

		public virtual void Fade(float p_transition, float p_duration = 0.3f, float p_delay = 0f, Easing p_easing = null, Action<UIScreen> p_callback = null)
		{
			ScreenEventType screenEventType = ((p_transition <= 0f) ? ScreenEventType.HideStart : ScreenEventType.ShowStart);
			open = screenEventType == ScreenEventType.ShowStart;
			Dispatch(screenEventType, p_callback);
			Tween.Kill(this, "transition");
			if (p_duration <= 0f)
			{
				transition = p_transition;
				Dispatch(p_transition, p_callback);
			}
			else
			{
				Tween.Add(this, "transition", p_transition, p_duration, p_delay, (p_easing == null) ? new Easing(Cubic.Out) : p_easing).onComplete = delegate
				{
					Dispatch(p_transition, p_callback);
				};
			}
		}

		public void Show(float p_duration = 0.3f, float p_delay = 0f, Easing p_easing = null, Action<UIScreen> p_callback = null)
		{
			Fade(1f, p_duration, p_delay, p_easing, p_callback);
		}

		public void Hide(float p_duration = 0.3f, float p_delay = 0f, Easing p_easing = null, Action<UIScreen> p_callback = null)
		{
			Fade(-0.1f, p_duration, p_delay, p_easing, p_callback);
		}

		protected virtual void OnTransition(float p_value)
		{
			switch (mode)
			{
			case Mode.Alpha:
				base.alpha = p_value;
				break;
			case Mode.Animation:
				if ((bool)animation)
				{
					string text = (clip ? clip.name : "fade");
					if ((bool)animation.GetClip(text))
					{
						AnimationState animationState = animation[text];
						bool flag = animationState.enabled;
						float weight = animationState.weight;
						animationState.enabled = true;
						animationState.normalizedTime = p_value;
						animationState.weight = 1f;
						animation.Sample();
						animationState.enabled = flag;
						animationState.weight = weight;
					}
				}
				break;
			case Mode.Custom:
				break;
			}
		}

		protected void Dispatch(float p_transition, Action<UIScreen> p_callback)
		{
			p_callback?.Invoke(this);
			if (OnEvent != null)
			{
				OnEvent.Invoke(new ScreenEvent(ScreenEventType.Transition, this));
				ScreenEventType screenEventType = ((p_transition <= 0f) ? ScreenEventType.Hide : ((p_transition >= 1f) ? ScreenEventType.Show : ScreenEventType.Transition));
				if (screenEventType != ScreenEventType.Transition)
				{
					OnEvent.Invoke(new ScreenEvent(screenEventType, this));
				}
			}
		}

		protected void Dispatch(ScreenEventType p_type, Action<UIScreen> p_callback)
		{
			p_callback?.Invoke(this);
			if (OnEvent != null)
			{
				OnEvent.Invoke(new ScreenEvent(p_type, this));
			}
		}
	}
}
