using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICampaignResultController : Controller<DRLApp>
	{
		public List<DRLRaceResultData> results;

		public GameController game => base.app.controller.game;

		public UICampaignResultView view => AssertLocal<UICampaignResultView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!view.current)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					DRLCampaign data = view.m_data;
					if (!data)
					{
						Debug.LogWarning("UICampaignResultController> Invalid Campaign Data");
						break;
					}
					Debug.Log("UICampaignResultController> Open - campaign[" + data.label + "]");
					view.Init();
					view.ToggleExitButton(game != null);
					CampaignResultsModel campaign = base.app.model.storage.state.player.results.campaign;
					PlayerStateModel player = base.app.model.storage.state.player;
					float raceTime = campaign.GetRaceTime(data);
					string p_labelText = data.label + " COMPLETED";
					view.Set(player.profile.username.ToUpper(), player.profile.photo, player.profile.color, p_labelText, raceTime);
					view.Show(0.5f);
				}
				break;
			case "campaign.close.results@click":
				game.Exit();
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			}
		}
	}
}
