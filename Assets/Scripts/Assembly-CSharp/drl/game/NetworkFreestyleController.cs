using UnityEngine;
using drl.network;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class NetworkFreestyleController : FreestyleController
	{
		private bool checkConnection;

		private Activity m_spectator_assert;

		public NetworkFreestyleModule network => AssertLocal<NetworkFreestyleModule>("network");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			base.OnNotification(p_event, p_target, p_data);
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "network.player.room@enter":
				break;
			case "game.ready":
				base.app.controller.game.input.pausePhysics = false;
				checkConnection = true;
				break;
			case "network.player.room@exit":
			{
				int p_player_id = (int)p_data[0];
				network.RemovePlayer(p_player_id);
				base.ui.hud.RefreshStandings(base.game.model.players);
				if (base.app.model.network.room != null && base.app.model.network.room.ActiveRacersCount == 0)
				{
					base.app.model.network.room.TrySwitchToRacer(base.app.model.network.room.Local);
				}
				break;
			}
			case "network.instantiate.local":
			{
				NetworkActor local2 = base.app.model.network.room.Local;
				network.AddDrone(local2);
				base.app.model.network.SendPlayerReady();
				break;
			}
			case "network.instantiate.remote":
			{
				NetworkActor p_player2 = (NetworkActor)p_data[0];
				network.AddDrone(p_player2);
				base.ui.hud.RefreshStandings(base.game.model.players);
				break;
			}
			case "network.drone.changed":
			{
				NetworkActor player = base.app.model.network.GetPlayer((int)p_data[0]);
				if (!player.IsLocal)
				{
					DroneRigData p_new = DroneRigData.FromJson((string)p_data[1]);
					GamePlayerData playerDataById3 = base.game.model.GetPlayerDataById(player.ID);
					_ = base.game.model.simulation;
					if (playerDataById3 != null && playerDataById3.drone != null)
					{
						base.app.model.storage.factory.Replace(p_new, playerDataById3.drone);
						base.app.controller.game.ApplyCommunityDroneToDrone(playerDataById3.drone);
					}
				}
				break;
			}
			case "network.player.racer":
			{
				NetworkActor networkActor = (NetworkActor)p_data[0];
				network.SetPlayerState(networkActor, GamePlayerType.Network);
				base.ui.hud.RefreshStandings(base.game.model.players);
				if (networkActor.IsLocal)
				{
					Pause(p_flag: false, p_pause_physics: false, p_open_pause_screen: false);
					base.app.view.ui.screens.Close("multiplayer-room-screen");
					base.app.view.ui.screens.Close("game-spectate-screen");
					base.app.view.ui.screens.manager.ClearHistory();
					base.app.view.ui.footer.Hide();
				}
				break;
			}
			case "network.player.spectator":
			{
				NetworkActor p_player = (NetworkActor)p_data[0];
				network.SetPlayerState(p_player, GamePlayerType.Spectator);
				base.ui.hud.RefreshStandings(base.game.model.players);
				break;
			}
			case "settings.profile-color.apply":
				base.game.model.GetPlayerDataById(base.game.model.playerData.playerId).color = base.app.model.storage.state.player.profile.color;
				base.ui.hud.RefreshStandings(base.game.model.players);
				break;
			case "game.intro.animation@complete":
				if (base.app.model.network.room != null)
				{
					NetworkActor local3 = base.app.model.network.room.Local;
					if (local3.IsSpectator)
					{
						network.SetPlayerState(local3, GamePlayerType.Spectator);
						base.ui.hud.RefreshStandings(base.game.model.players);
					}
				}
				break;
			case "game.simulation.drone@crash":
			{
				Drone drone = p_data[0] as Drone;
				if (drone == null)
				{
					break;
				}
				NetworkRoom room2 = base.app.model.network.room;
				if (room2 != null)
				{
					NetworkActor local4 = base.app.model.network.room.Local;
					if (local4 != null && room2.State == NetworkRoom.StateCode.GameRunning && !local4.IsSpectator && local4.RaceState == NetworkActor.RacerState.Running)
					{
						room2.SendPlayerCrashed(local4.RaceTime, drone.position, drone.transform.rotation, drone.rigidbody.rb.velocity, drone.crashData);
					}
				}
				break;
			}
			case "game.simulation.drone@recover":
			{
				if (p_data[0] as Drone == null)
				{
					break;
				}
				NetworkRoom room = base.app.model.network.room;
				if (room != null)
				{
					NetworkActor local = base.app.model.network.room.Local;
					if (local != null && room.State == NetworkRoom.StateCode.GameRunning && !local.IsSpectator)
					{
						room.SendPlayerRecovered();
					}
				}
				break;
			}
			case "network.player.crashed":
			{
				if (p_data.Length == 0)
				{
					break;
				}
				if (!(p_data[0] is NetworkRoom.DroneState droneState))
				{
					Debug.LogWarning("NetworkRaceController> Drone crashed but no crash data present!");
					break;
				}
				GamePlayerData playerDataById2 = base.game.model.GetPlayerDataById(droneState.PlayerId);
				if (playerDataById2 != null && playerDataById2.type == GamePlayerType.Network)
				{
					float p_ping = ((base.app.model.network.lobby != null) ? ((float)base.app.model.network.lobby.PingTime / 1000f) : 0.3f);
					Quaternion.Euler(droneState.Rotation);
					if (droneState.CrashEnergy > 0f)
					{
						playerDataById2.drone.CrashRemote(droneState.CrashEnergy, droneState.ContactNormal, droneState.ImpactVelocity, droneState.ContactPoint, p_ping);
					}
					Debug.Log("NetworkFreestyleController> Network user [" + playerDataById2.upperName + "] crashed! ");
				}
				break;
			}
			case "network.player.recovered":
			{
				int p_id = (int)p_data[0];
				GamePlayerData playerDataById = base.game.model.GetPlayerDataById(p_id);
				if (playerDataById != null && playerDataById.type == GamePlayerType.Network)
				{
					playerDataById.drone.Fix();
					Debug.Log("NetworkFreestyleController> Network user [" + playerDataById.upperName + "] recovered! ");
				}
				break;
			}
			}
		}

		protected override void LoadDrones()
		{
		}

		public override bool OnGameCommand(GameCommand p_command)
		{
			switch (p_command.type)
			{
			case GameCommandType.TabScreenEnable:
				base.game.SwitchTabScreen();
				break;
			case GameCommandType.Pause:
				AssertSpectator();
				return true;
			}
			return base.OnGameCommand(p_command);
		}

		private new void OnDestroy()
		{
		}

		protected override void OnPause(bool p_flag, bool p_pause_physics)
		{
			Drone playerDrone = base.game.model.playerDrone;
			if (playerDrone != null && playerDrone.isBroken)
			{
				playerDrone.Fix();
			}
		}

		protected void AssertSpectator()
		{
			if (m_spectator_assert != null)
			{
				m_spectator_assert.Stop();
			}
			if (base.app.model.network.room == null)
			{
				return;
			}
			NetworkActor local = base.app.model.network.room.Local;
			if (local == null || !local.IsSpectator)
			{
				return;
			}
			m_spectator_assert = Activity.RunOnce(delegate
			{
				m_spectator_assert = null;
				if (base.validContext)
				{
					int count = base.app.view.ui.screens.manager.history.Count;
					Debug.Log($"NetworkFreestyleController> Pause / Asserting Spectate UI [{base.app.view.ui.screens.current}] history[{count}]");
					if (count <= 0 && !base.app.view.ui.screens.IsCurrent("game-spectate-screen"))
					{
						base.app.view.ui.screens.Open<UISpectateView>("game-spectate-screen");
					}
				}
			}, 3f);
		}
	}
}
