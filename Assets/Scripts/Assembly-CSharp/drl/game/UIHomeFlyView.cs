using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class UIHomeFlyView : UIScreenView
	{
		public List<GameObject> virtualSeasonExcluded;

		public void SetVirtualSeasonLayout()
		{
			if (virtualSeasonExcluded != null)
			{
				for (int i = 0; i < virtualSeasonExcluded.Count; i++)
				{
					virtualSeasonExcluded[i].SetActive(value: false);
				}
			}
		}

		public void SetCardEnabled(UICardButtonLarge p_card, bool p_flag, string p_label)
		{
			Debug.Log($"UIHomeFlyView> SetCardEnabled / card [{p_card}] flag [{p_flag}] lb [{p_label}]");
			bool flag = p_card.notification.Contains("multiplayer");
			Component component = p_card.Find<Component>("backgrounds.disabled");
			VerticalLayoutGroup verticalLayoutGroup = p_card.Find<VerticalLayoutGroup>("content.body");
			UIStatusView uIStatusView = p_card.Find<UIStatusView>("content.status");
			if ((bool)component)
			{
				component.gameObject.SetActive(!p_flag);
				uIStatusView.SetWarning(p_label);
				uIStatusView.fade.alpha = ((!p_flag) ? 1f : 0f);
				RectOffset padding = verticalLayoutGroup.padding;
				verticalLayoutGroup.enabled = p_flag;
				padding.top = (flag ? (-90) : 0);
				verticalLayoutGroup.padding = padding;
				verticalLayoutGroup.enabled = !p_flag;
			}
		}
	}
}
