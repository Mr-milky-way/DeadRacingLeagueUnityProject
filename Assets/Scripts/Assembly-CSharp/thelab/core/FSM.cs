using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class FSM<T> : FSM
	{
		[Serializable]
		public class Transition
		{
			public T from;

			public T to;

			public FSMCallback OnEvent;
		}

		public class Log
		{
			public T state;

			public float time;
		}

		[SerializeField]
		private T m_state;

		private List<Log> m_log;

		private List<Transition> m_transitions;

		public FSMCallback OnStateEvent;

		public FSMCallback OnChangeEvent;

		public T state
		{
			get
			{
				return m_state;
			}
			set
			{
				if (EqualityComparer<T>.Default.Equals(m_state, value))
				{
					return;
				}
				T p_previous = m_state;
				m_state = value;
				if (Application.isPlaying)
				{
					Log log = new Log();
					log.state = p_previous;
					log.time = time;
					this.log.Add(log);
					OnChange(p_previous, m_state);
					if (OnChangeEvent != null)
					{
						OnChangeEvent.Call(this);
					}
				}
			}
		}

		public T previous
		{
			get
			{
				if (log.Count > 0)
				{
					return log[log.Count - 1].state;
				}
				return default(T);
			}
		}

		public List<Log> log
		{
			get
			{
				if (m_log != null)
				{
					return m_log;
				}
				return m_log = new List<Log>();
			}
		}

		public List<Transition> transitions
		{
			get
			{
				if (m_transitions != null)
				{
					return m_transitions;
				}
				return m_transitions = new List<Transition>();
			}
		}

		protected override void Awake()
		{
			if (m_state == null)
			{
				m_state = default(T);
			}
			time = 0f;
			Activity.Add(this);
		}

		public void Set(T p_state)
		{
			state = p_state;
		}

		public void Delay(T p_state, float p_time)
		{
			Activity.RunOnce(delegate
			{
				state = p_state;
			}, p_time);
		}

		public override void Clear()
		{
			state = default(T);
			time = 0f;
			log.Clear();
		}

		public override Type GetStateType()
		{
			return typeof(T);
		}

		protected virtual void OnChange(T p_previous, T p_next)
		{
		}

		protected virtual void OnState(T p_state)
		{
		}

		public override void OnUpdate()
		{
			time += Time.unscaledDeltaTime;
			if (state != null)
			{
				OnState(state);
				if (OnStateEvent != null)
				{
					OnStateEvent.Invoke(this);
				}
			}
		}
	}
	public class FSM : MonoBehaviour, IUpdateable
	{
		public float time;

		protected virtual void Awake()
		{
		}

		public virtual void Clear()
		{
		}

		public virtual Type GetStateType()
		{
			return null;
		}

		public virtual void OnUpdate()
		{
		}
	}
}
