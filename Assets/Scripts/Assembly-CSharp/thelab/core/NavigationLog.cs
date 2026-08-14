using UnityEngine;

namespace thelab.core
{
	public class NavigationLog : MonoBehaviour, IFocusHandler
	{
		public void OnFocus()
		{
		}

		public void OnUnfocus()
		{
		}

		public void Click()
		{
			Debug.Log(base.name + " Clicked");
		}
	}
}
