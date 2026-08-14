using UnityEngine;

namespace drl.game
{
	public class DRLOnlineComponents : MonoBehaviour
	{
		public Object[] targets;

		public void SetComponentsEnabled(bool p_flag)
		{
			if (targets == null || targets.Length == 0)
			{
				return;
			}
			for (int i = 0; i < targets.Length; i++)
			{
				if (!(targets[i] == null))
				{
					if (targets[i] is Behaviour)
					{
						((Behaviour)targets[i]).enabled = p_flag;
					}
					if (targets[i] is GameObject)
					{
						((GameObject)targets[i]).SetActive(p_flag);
					}
				}
			}
		}
	}
}
