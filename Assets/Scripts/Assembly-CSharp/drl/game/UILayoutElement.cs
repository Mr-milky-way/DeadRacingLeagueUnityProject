using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class UILayoutElement<T> : Tag<T>
	{
		public int index;

		public void Destroy()
		{
			if ((bool)base.gameObject)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
