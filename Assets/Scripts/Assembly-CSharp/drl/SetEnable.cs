using UnityEngine;

namespace drl
{
	public class SetEnable : MonoBehaviour
	{
		public bool value;

		public float delay;

		protected void Awake()
		{
			if (delay > 0f)
			{
				Invoke(value ? "Enable" : "Disable", delay);
			}
			else
			{
				base.gameObject.SetActive(value);
			}
		}

		public void Enable()
		{
			base.gameObject.SetActive(value: true);
		}

		public void Disable()
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
