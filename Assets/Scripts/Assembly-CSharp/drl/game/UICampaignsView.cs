using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class UICampaignsView : UIScreenView
	{
		public ListComponent listField;

		public void Clear()
		{
			listField.Clear();
		}

		public void Add(DRLCampaign p_data)
		{
			if (!p_data)
			{
				Debug.LogWarning("UICampaignsView> Add - Invalid Campaign");
				return;
			}
			GameFlagTag component = p_data.GetComponent<GameFlagTag>();
			if (!component || !component.Match(GameFlag.Development))
			{
				UICardButtonCampaign uICardButtonCampaign = listField.Push<UICardButtonCampaign>();
				if ((bool)p_data)
				{
					uICardButtonCampaign.fadeField = p_data.tournament;
					uICardButtonCampaign.labelField.enabled = !p_data.tournament;
				}
				uICardButtonCampaign.notification = "campaign.campaign-card";
				uICardButtonCampaign.Set(p_data);
			}
		}

		public void Set(List<DRLCampaign> p_list)
		{
			Clear();
			for (int i = 0; i < p_list.Count; i++)
			{
				Add(p_list[i]);
			}
		}
	}
}
