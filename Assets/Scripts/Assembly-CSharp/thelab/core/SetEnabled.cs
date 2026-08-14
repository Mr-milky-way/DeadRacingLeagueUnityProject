using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class SetEnabled : MonoBehaviour
	{
		[Serializable]
		public class Target
		{
			public UnityEngine.Object target;

			public bool flag;

			public void Apply()
			{
				if ((bool)target)
				{
					if (target is Behaviour)
					{
						((Behaviour)target).enabled = flag;
					}
					if (target is GameObject)
					{
						((GameObject)target).SetActive(flag);
					}
				}
			}
		}

		public List<Target> targets;

		protected void Awake()
		{
			Apply();
		}

		protected void Start()
		{
		}

		public void Apply()
		{
			if (base.enabled)
			{
				for (int i = 0; i < targets.Count; i++)
				{
					targets[i].Apply();
				}
			}
		}
	}
}
