using System;
using UnityEngine;
using UnityEngine.Events;

namespace thelab.core
{
	[Serializable]
	public class FSMCallback : UnityEvent<FSM>
	{
		public void Call(FSM p_target)
		{
			int persistentEventCount = GetPersistentEventCount();
			for (int i = 0; i < persistentEventCount; i++)
			{
				UnityEngine.Object persistentTarget = GetPersistentTarget(i);
				string persistentMethodName = GetPersistentMethodName(i);
				Reflection<object>.Invoke(persistentTarget, persistentMethodName, p_target);
			}
		}
	}
}
