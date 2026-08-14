using UnityEngine;
using thelab.mvc;

namespace thelab.core
{
	public class UINavigationLinkList : UINavigation
	{
		public Component[] links;

		public UINavigationLink backProxy;

		public override void OnFocus()
		{
			if (links == null || links.Length == 0)
			{
				UINavigation.Focus(callee);
			}
			int num = ((links != null) ? links.Length : 0);
			Component component = null;
			for (int i = 0; i < num; i++)
			{
				if (!links[i] || !links[i].gameObject.activeInHierarchy)
				{
					continue;
				}
				component = links[i];
				if (!component)
				{
					continue;
				}
				UIElementView component2 = component.GetComponent<UIElementView>();
				if (!(component2 != null) || component2.interactable)
				{
					UINavigation component3 = component.GetComponent<UINavigation>();
					if (!component3 || component3.enabled)
					{
						break;
					}
					if (i + 1 >= num)
					{
						component = null;
						break;
					}
				}
			}
			if ((bool)backProxy)
			{
				backProxy.link = callee;
			}
			UINavigation.Focus(component ? component : callee);
		}
	}
}
