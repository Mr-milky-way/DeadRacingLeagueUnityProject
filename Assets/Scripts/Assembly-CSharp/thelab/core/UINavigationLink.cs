using UnityEngine;

namespace thelab.core
{
	public class UINavigationLink : UINavigation
	{
		public Component link;

		public UINavigationLink backProxy;

		public override void OnFocus()
		{
			bool flag = (bool)link && link.gameObject.activeInHierarchy;
			UINavigation uINavigation = (flag ? link.GetComponent<UINavigation>() : null);
			flag = (uINavigation ? uINavigation.enabled : flag);
			if ((bool)backProxy)
			{
				backProxy.link = callee;
			}
			UINavigation.Focus(flag ? link : callee);
		}
	}
}
