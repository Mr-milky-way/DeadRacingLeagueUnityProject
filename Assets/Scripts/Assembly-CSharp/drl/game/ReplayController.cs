using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class ReplayController : GameTypeController
	{
		public UIReplayView view;

		public ReplayModel model => AssertLocal<ReplayModel>("model");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			base.OnNotification(p_event, p_target, p_data);
			switch (p_event)
			{
			case "viewer.controls.nav.exit@click":
				if ((bool)base.app.model.game && (bool)view)
				{
					view.StopPlayback();
					if (base.app.model.game.type != GameFlag.Replay)
					{
						base.game.Exit();
					}
					else if (base.app.arguments.lastLeaderboard == DRLAppArguments.LeaderboardType.campaign && base.app.arguments.leaderboardsCampaign != null)
					{
						UITryoutsLeadersView uITryoutsLeadersView = base.app.view.ui.screens.Open<UITryoutsLeadersView>("tryouts-leaders-screen");
						uITryoutsLeadersView.data = base.app.arguments.leaderboardsCampaign.campaign;
						uITryoutsLeadersView.AllowNext(p_flag: false);
						base.app.view.ui.screens.Open("leaderboards-screen", 0.2f);
					}
					else
					{
						base.app.view.ui.screens.Open("leaderboards-screen", 0f);
					}
				}
				break;
			case "garage.edit.fly.ready":
			{
				base.enabled = false;
				base.app.arguments.game.players.Clear();
				base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
				base.app.arguments.game.mode = GameFlag.SinglePlayer;
				base.app.arguments.game.type = GameFlag.Sandbox;
				DRLMapTrack dRLMapTrack = null;
				switch ((int)p_data[2])
				{
				case 0:
					dRLMapTrack = base.app.model.storage.library.FindByGUID<DRLMapTrack>("MT-9ea");
					break;
				case 1:
					dRLMapTrack = base.app.model.storage.library.FindByGUID<DRLMapTrack>("MT-1a7");
					break;
				}
				DRLMap map = dRLMapTrack.map;
				base.app.view.audio.PlayUIStartGame();
				base.app.arguments.game.map = map;
				base.app.arguments.game.track = dRLMapTrack;
				base.app.arguments.game.podium = dRLMapTrack.podium;
				base.app.arguments.game.fcMode = base.app.model.storage.state.player.activeFCMode;
				base.app.arguments.game.allowCrash = false;
				base.app.view.audio.SceneMainToGame(1.6f);
				base.app.view.ui.fade.FadeIn(1.5f);
				Activity.RunOnce(base.app.scene.Load, 1f);
				break;
			}
			case "garage.edit.done":
				base.app.view.ui.screens.Return();
				break;
			}
		}

		protected override void LoadDrones()
		{
			List<GamePlayerData> players = base.game.model.players;
			base.app.model.game.replay.player.Clear();
			if (ReplayFile.EnableVersion2)
			{
				List<ReplayFile> list = new List<ReplayFile>();
				for (int i = 0; i < players.Count; i++)
				{
					GamePlayerData gamePlayerData = players[i];
					list.Add(gamePlayerData.replayV2);
				}
				Debug.Log("ReplayController> LoadDrones - Loading [" + list.Count + "] replay files");
				base.app.model.game.replay.player.SetClips(list);
			}
			else
			{
				List<BlackboxData> list2 = new List<BlackboxData>();
				for (int j = 0; j < players.Count; j++)
				{
					GamePlayerData gamePlayerData2 = players[j];
					list2.Add(gamePlayerData2.replay);
					Debug.Log("ReplayController> LoadDrones - Loading [" + list2.Count + "] clips");
					base.app.model.game.replay.player.SetClips(list2);
				}
			}
			base.app.view.ui.screens.Open<UISpectateView>("game-spectate-screen").tournamentContext = base.app.inTournament;
		}

		public void Run()
		{
			base.game.model.level.radio.enabled = false;
			Activity.RunOnce(delegate
			{
				if ((bool)view)
				{
					view.EnableControls(p_focus: true);
				}
			}, 1f / 30f);
			SetGameReady();
		}

		protected override void PlayIntroAnimation()
		{
			Run();
		}

		protected override void OnIntroAnimationComplete()
		{
			StopIntroAnimations();
		}

		public override bool OnGameCommand(GameCommand p_command)
		{
			if (p_command == null)
			{
				return true;
			}
			if (p_command.type == GameCommandType.Pause)
			{
				Notify("spectate.pause-command");
				if ((bool)view)
				{
					if (view.ControlsEnabled())
					{
						view.PlaybackPause();
					}
					else
					{
						view.PlaybackUnpause();
					}
				}
				return false;
			}
			return true;
		}
	}
}
