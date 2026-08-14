using System.Collections.Generic;
using UnityEngine;

public class AnimatorEvents : MonoBehaviour
{
	public List<Object> targets;

	public void EnableEvent()
	{
		EnableDisable(p_state: true);
	}

	public void DisableEvent()
	{
		EnableDisable(p_state: false);
	}

	private void EnableDisable(bool p_state)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			Object obj = targets[i];
			if (!obj)
			{
				continue;
			}
			if (obj is GameObject)
			{
				((GameObject)obj).SetActive(p_state);
			}
			else if (obj is Behaviour)
			{
				((Behaviour)obj).enabled = p_state;
				if (obj is Animator)
				{
					Animator obj2 = obj as Animator;
					obj2.Play(obj2.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, p_state ? 0f : 1f);
				}
			}
		}
	}
}
