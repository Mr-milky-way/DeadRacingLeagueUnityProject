using UnityEngine;

namespace thelab.core
{
	public class ActivityBehaviour : MonoBehaviour
	{
		public ActivityManager manager
		{
			get
			{
				if (!Application.isPlaying)
				{
					return null;
				}
				return ActivityManager.instance;
			}
		}

		private void OnEnable()
		{
			if (Application.isPlaying)
			{
				Activity.Add(this);
			}
		}

		private void OnDisable()
		{
			if (Application.isPlaying)
			{
				Activity.Remove(this);
			}
		}

		private void OnDestroy()
		{
			if (Application.isPlaying)
			{
				Activity.Remove(this);
			}
		}
	}
}
