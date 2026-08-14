using UnityEngine;

namespace thelab.mvc
{
	public class AnimatorView<T> : AnimatorView where T : BaseApplication
	{
		public new T app => (T)base.app;
	}
	public class AnimatorView : StateMachineBehaviour
	{
		public string notification;

		public bool enter = true;

		public bool update;

		public bool exit = true;

		public bool move = true;

		public bool ik;

		public bool begin = true;

		public bool end = true;

		private BaseApplication m_app;

		public BaseApplication app
		{
			get
			{
				if (!(m_app == null))
				{
					return m_app;
				}
				return m_app = Object.FindObjectOfType<BaseApplication>();
			}
		}

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (!(app == null) && enter)
			{
				app.Notify(notification + "@animator-enter", animator, stateInfo, layerIndex);
			}
		}

		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (!(app == null) && update)
			{
				app.Notify(notification + "@animator-update", animator, stateInfo, layerIndex);
			}
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (!(app == null) && exit)
			{
				app.Notify(notification + "@animator-exit", animator, stateInfo, layerIndex);
			}
		}

		public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (!(app == null) && move)
			{
				app.Notify(notification + "@animator-move", animator, stateInfo, layerIndex);
			}
		}

		public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (!(app == null) && ik)
			{
				app.Notify(notification + "@animator-ik", animator, stateInfo, layerIndex);
			}
		}

		public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
		{
			if (!(app == null) && begin)
			{
				app.Notify(notification + "@fsm-enter", animator, stateMachinePathHash);
			}
		}

		public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
		{
			if (!(app == null) && end)
			{
				app.Notify(notification + "@fsm-exit", animator, stateMachinePathHash);
			}
		}
	}
}
