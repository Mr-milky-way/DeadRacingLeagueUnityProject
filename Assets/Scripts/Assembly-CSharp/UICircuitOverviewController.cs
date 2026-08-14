using UnityEngine;
using drl;
using drl.backend;
using drl.game;
using thelab.core;
using thelab.mvc;

public class UICircuitOverviewController : Controller<DRLApp>
{
	public UICircuitOverviewView view => AssertLocal<UICircuitOverviewView>("view");

	private CircuitStateModel model => base.app.model.storage.state.player.circuits;

	public override void OnNotification(string p_event, Object p_target, params object[] p_data)
	{
		if (base.app.view.ui.screens.current != view.screen || p_event == null)
		{
			return;
		}
		switch (p_event)
		{
		case "ui.screen@open":
			if (p_data[0] as UIScreen != view.screen)
			{
				break;
			}
			if (view.caller == null || !(view.caller is UICircuitSelectionController))
			{
				if (base.app.inGame && model.activeCircuit != null)
				{
					view.circuitData = model.activeCircuit;
				}
				if (base.app.inGame)
				{
					view.circuitSelectionButton.SetActive(value: true);
				}
			}
			else
			{
				view.circuitSelectionButton.SetActive(value: false);
			}
			view.caller = null;
			view.Set();
			RefreshCards();
			break;
		case "circuits.opponent-form.event@click":
			OnFormNotification(p_target, p_change: false);
			break;
		case "circuits.opponent-form.event@change":
			OnFormNotification(p_target, p_change: true);
			break;
		case "circiuits.circuit-map@click":
		{
			base.enabled = false;
			UICircuitMapItemView uICircuitMapItemView = (UICircuitMapItemView)p_target;
			if (uICircuitMapItemView == null || uICircuitMapItemView.isLocked)
			{
				base.enabled = true;
			}
			else if (uICircuitMapItemView.isComplete)
			{
				if (model.GetCircuitProgress(view.circuitData.guid).finished)
				{
					base.app.view.ui.dialog.Open(DialogType.Warning, "CIRCUIT RESET", "This will restart the full circuit. Do you wish to restart the circuit?".ToUpper(), new string[2] { "YES", "NO" }, null, "circuit-reset", delegate(string p_id, int p_option)
					{
						if (!(p_id != "circuit-reset") && p_option == 1)
						{
							model.ResetProgress(view.circuitData.guid);
							view.Set();
							UINavigation.Focus(view.mapCardsListField.Get<UICircuitMapItemView>(0).GetComponent<UINavigation>());
						}
					});
				}
				base.enabled = true;
			}
			else
			{
				StartMap(uICircuitMapItemView.map, uICircuitMapItemView.track, uICircuitMapItemView.customMapData);
			}
			break;
		}
		case "ui.screen.return@click":
			base.app.view.ui.screens.Return();
			break;
		case "circuits.circuit-overview.exit@click":
			base.enabled = false;
			base.app.view.audio.PlayUIGenericSuccess();
			base.app.controller.game.Exit();
			break;
		case "circuits.circuit-overview.selection@click":
			base.app.view.ui.screens.Open("circuits-selection-screen");
			break;
		case "circuits.circuit-overview.leader@click":
		{
			CircuitStateModel.CircuitsProgressData circuitProgress = model.GetCircuitProgress(view.circuitData.guid);
			if (circuitProgress == null || circuitProgress.drlOfficial)
			{
				UILeaderboardsView uILeaderboardsView = base.app.view.ui.screens.Open<UILeaderboardsView>("leaderboards-screen");
				uILeaderboardsView.screen.title = base.app.model.storage.locale.Get("home.card.leaders.drl", "DRL LEADERS");
				uILeaderboardsView.gameTypeFlag = GameFlag.Race;
				uILeaderboardsView.circuit = view.circuitData;
				base.app.arguments.lastLeaderboard = DRLAppArguments.LeaderboardType.drl;
			}
			else
			{
				UILeaderboardsView uILeaderboardsView2 = base.app.view.ui.screens.Open<UILeaderboardsView>("leaderboards-screen");
				uILeaderboardsView2.screen.title = base.app.model.storage.locale.Get("home.card.leaders.open", "OPEN CLASS LEADERS").Replace("\n", " ");
				uILeaderboardsView2.gameTypeFlag = GameFlag.Race;
				uILeaderboardsView2.circuit = view.circuitData;
				base.app.arguments.lastLeaderboard = DRLAppArguments.LeaderboardType.open;
			}
			break;
		}
		}
	}

	private void OnFormNotification(Object p_target, bool p_change)
	{
		string text = (p_target ? p_target.name : "");
		bool flag = p_change;
		switch (text)
		{
		case "opponent":
			if (flag)
			{
				switch (view.opponentMode)
				{
				case CircuitsOpponentMode.Off:
					view.SetOpponentDifficultyFlag(p_flag: false);
					model.opponentMode = CircuitsOpponentMode.Off;
					break;
				case CircuitsOpponentMode.On:
					view.SetOpponentDifficultyFlag(p_flag: true);
					model.opponentMode = CircuitsOpponentMode.On;
					break;
				}
			}
			break;
		case "difficulty":
			if (flag)
			{
				model.opponentDifficulty = view.opponentDifficulty;
			}
			break;
		case "reset":
			if (!flag)
			{
				model.ResetProgress(view.circuitData.guid);
				view.Set();
				RefreshCards();
			}
			break;
		}
	}

	protected void RefreshCards()
	{
		if (!base.validContext || base.app.model.service == null)
		{
			return;
		}
		bool value = model.GetCircuitProgress(view.circuitData.guid)?.drlOfficial ?? true;
		base.app.model.service.GetLeaderboardCircuit(view.circuitData.guid, 1, 1, -1, value, 0, delegate(DRLCircuitsResult result)
		{
			if (!base.validContext || result == null || result.leaderboard == null || result.leaderboard.Length == 0)
			{
				view.SetLeader(null);
			}
			else
			{
				view.SetLeader(result.leaderboard[0]);
			}
		});
	}

	public void StartMap(DRLMap p_map, DRLMapTrack p_track, MapData p_customData = null)
	{
		OpponentModeType omt = ((view.opponentMode == CircuitsOpponentMode.On) ? OpponentModeType.Leader : OpponentModeType.Off);
		int p_trackIdx = 0;
		DRLCircuitData circuitData = view.circuitData;
		int p_circuitDifficulty = (int)((base.app.model.storage.state.player.circuits.opponentMode == CircuitsOpponentMode.Off) ? ((CircuitsOpponentDifficulty)(-1)) : base.app.model.storage.state.player.circuits.opponentDifficulty);
		DRLCircuitMapData[] maps = circuitData.maps;
		for (int i = 0; i < maps.Length; i++)
		{
			if (p_customData != null && p_customData.guid == maps[i].trackId)
			{
				p_trackIdx = i;
				break;
			}
			if (p_track.guid == maps[i].trackId)
			{
				p_trackIdx = i;
				break;
			}
		}
		if (p_customData != null && p_map != null)
		{
			p_map.data = p_customData;
		}
		model.SetInProgress(view.circuitData, p_trackIdx);
		if (base.app.inGame)
		{
			base.app.arguments.game?.RemoveGhostPlayers();
		}
		if (omt == OpponentModeType.Off)
		{
			Notify("fly.map-track-overview.ready", new MapLoadData(p_map, p_track, p_customData, omt));
			return;
		}
		view.status.gameObject.SetActive(value: true);
		view.status.SetLoading(0f);
		view.status.fade.FadeIn(0.2f);
		view.scroll.enabled = false;
		Transform p = view.status.transform.parent;
		view.status.transform.SetParent(base.app.view.ui.screens.transform, worldPositionStays: false);
		RectTransform component = view.status.GetComponent<RectTransform>();
		if (component != null)
		{
			component.anchoredPosition = new Vector2(300f, -20f);
		}
		ServiceModel sm = base.app.model.service;
		sm.opponent.Load(omt, p_map, p_track, 1, 7, p_drone_official: true, p_custom_physics: false, delegate
		{
			switch (sm.opponent.status)
			{
			case OpponentModel.Status.Error:
				view.status.SetWarning("LOADING FAILED!");
				view.status.fade.FadeOut(0.5f, 2f);
				base.app.view.audio.PlayUIGenericError();
				sm.opponent.Cancel();
				view.scroll.enabled = true;
				base.enabled = true;
				break;
			case OpponentModel.Status.NoResults:
				view.status.SetWarning("NO OPPONENTS FOUND!");
				sm.opponent.Cancel();
				Notify(1.5f, "fly.map-track-overview.ready", new MapLoadData(p_map, p_track, p_customData, omt));
				view.status.gameObject.SetActive(value: false);
				view.scroll.enabled = true;
				base.enabled = true;
				break;
			case OpponentModel.Status.ByPass:
				Notify("fly.map-track-overview.ready", new MapLoadData(p_map, p_track, p_customData, omt));
				view.status.gameObject.SetActive(value: false);
				view.scroll.enabled = true;
				break;
			case OpponentModel.Status.Progress:
				view.status.SetLoading(sm.opponent.progress);
				break;
			case OpponentModel.Status.Complete:
				view.scroll.enabled = true;
				this.TimerRunOnce(delegate
				{
					if (base.validContext && !(view == null) && !(view.status == null))
					{
						view.status.gameObject.SetActive(value: false);
						view.status.transform.SetParent(p, worldPositionStays: true);
					}
				}, 0.5f);
				Notify(1f / 60f, "fly.map-track-overview.ready", new MapLoadData(p_map, p_track, p_customData, omt, sm.opponent.ghostRecords, sm.opponent.ghostRecordsV2));
				break;
			case OpponentModel.Status.ManifestSuccess:
				view.status.SetLoading(0f);
				base.app.view.audio.PlayUIGenericSuccess();
				break;
			}
		}, circuitData.guid, p_circuitDifficulty);
	}
}
