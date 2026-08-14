using System;
using System.Collections.Generic;
using UnityEngine;
using drl.network;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class NetworkGameModule : Controller<DRLApp>
	{
		public GameTypeController controller => AssertLocal<GameTypeController>("controller");

		public GameController game => base.app.controller.game;

		public virtual DroneRigData defaultRig => null;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (!controller || !controller.isActiveAndEnabled || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "game.pause.exit@click":
				base.enabled = false;
				base.app.model.network.LeaveRoom();
				break;
			case "game.race-complete.exit@click":
				base.enabled = false;
				base.app.model.network.LeaveRoom();
				break;
			case "network.player@update":
				if (p_data != null && p_data.Length != 0)
				{
					NetworkActor networkActor2 = (NetworkActor)p_data[0];
					if (networkActor2 != null)
					{
						game.model.GetPlayerDataById(networkActor2.ID)?.SetNetwork(networkActor2);
					}
				}
				break;
			case "network.player-kicked":
				Debug.Log("NetworkGameModule> Player kicked out from game");
				game.Exit();
				break;
			case "network.room@exit":
				Debug.Log("NetworkGameModule> RoomExit - Exiting Room");
				if ((PhotonService.DisconnectionCode)p_data[0] != PhotonService.DisconnectionCode.ByUser)
				{
					game.Exit();
				}
				break;
			case "network.disconnect":
				Debug.LogWarning("NetworkGameModule> Disconnect");
				if ((PhotonService.DisconnectionCode)p_data[0] != PhotonService.DisconnectionCode.ByUser)
				{
					game.Exit();
				}
				break;
			case "game.simulation.load@complete":
				Debug.Log("NetworkGameModule> Level Boot complete.");
				base.app.model.network.SendLevelLoaded();
				break;
			case "network.player.racer":
			{
				NetworkActor networkActor = (NetworkActor)p_data[0];
				if (!networkActor.IsLocal)
				{
					break;
				}
				DroneSimulation simulation = game.model.simulation;
				if (simulation == null)
				{
					break;
				}
				List<Drone> list = simulation.drones.list.FindAll((Drone d) => d.renderer.shadowsOnly);
				if (list == null || list.Count == 0)
				{
					break;
				}
				{
					foreach (Drone item in list)
					{
						if (item.receiver.channel != networkActor.ID)
						{
							item.renderer.shadowsOnly = false;
						}
					}
					break;
				}
			}
			}
		}

		public void SetPlayerState(NetworkActor p_player, GamePlayerType p_type)
		{
			if (p_player == null)
			{
				return;
			}
			Debug.Log(string.Format("NetworkGameModule> SetPlayerState / {0} - [{1}][{2}] {3}", p_type, p_player.ViewId, p_player.IsLocal ? "local" : "remote", p_player.ProfileName));
			GamePlayerData playerDataById = game.model.GetPlayerDataById(p_player.ID);
			DroneSimulation sim = game.model.simulation;
			DroneCamera c = game.model.camera;
			Drone drone = null;
			DroneInputTransmitter droneInputTransmitter = null;
			if (!sim || playerDataById == null)
			{
				return;
			}
			switch (p_type)
			{
			case GamePlayerType.Human:
			case GamePlayerType.Network:
			{
				Debug.Log($"NetworkGameModule> SetPlayerState / {p_type} - Player Create Start!");
				playerDataById.type = GamePlayerType.Network;
				if (!p_player.IsLocal)
				{
					break;
				}
				playerDataById.type = GamePlayerType.Human;
				float drone_wait_timer = 1f;
				controller.CreatePlayer(playerDataById, delegate(Drone p_new_drone)
				{
					Debug.Log($"NetworkGameModule> SetPlayerState / {p_type} - Drone Created!");
					((Component)this).ActivityRun((Predicate<float>)delegate
					{
						if (drone_wait_timer <= 0f)
						{
							Debug.Log($"NetworkGameModule> SetPlayerState / {p_type} - failed to set player state: " + (p_new_drone == null));
							return false;
						}
						drone_wait_timer -= Time.deltaTime;
						if (!base.validContext || p_new_drone == null || !p_new_drone.ready || p_new_drone.fc == null || p_new_drone.receiver == null)
						{
							return true;
						}
						if (!game.model.IsPlayer(p_new_drone))
						{
							return false;
						}
						Debug.Log($"NetworkGameModule> SetPlayerState / {p_type} - paused[{game.model.paused}]");
						p_new_drone.fc.armed = true;
						p_new_drone.receiver.enabled = true;
						p_new_drone.SetEnabled(p_flag: true);
						if (game.model.paused)
						{
							p_new_drone.SetPaused(p_flag: true);
							sim.drones.SaveArmed(p_new_drone, p_flag: true);
						}
						controller.SetDroneFCMode(p_new_drone, base.app.model.storage.state.player.activeFCMode);
						if (c != null)
						{
							c.SetFPV(p_new_drone);
						}
						Debug.Log($"NetworkGameModule> SetPlayerState / {p_type} - Player Created!");
						return false;
					}, 0f);
				});
				AddTransmitter(p_player);
				break;
			}
			case GamePlayerType.Spectator:
				if (p_player.IsLocal)
				{
					base.app.view.ui.screens.Open<UISpectateView>("game-spectate-screen").GetComponent<UISpectateController>().Initialize();
				}
				Debug.Log($"NetworkGameModule> SetPlayerState / {p_type} - Spectate UI Init");
				playerDataById.type = GamePlayerType.Spectator;
				drone = playerDataById.drone;
				if (!(drone == null))
				{
					if ((bool)drone && (bool)drone.receiver)
					{
						drone.receiver.enabled = true;
					}
					Debug.Log($"NetworkGameModule> SetPlayerState / {p_type} - Spectator Drone Step [{drone.rig.rigName}]");
					droneInputTransmitter = sim.transmitters.GetByDrone<DroneInputTransmitter>(drone);
					sim.RemoveDrone(drone);
					sim.transmitters.Remove(droneInputTransmitter);
					Debug.Log($"NetworkGameModule> SetPlayerState / {p_type} - Spectator Transmitter Step");
				}
				break;
			case GamePlayerType.Ghost:
				break;
			}
		}

		public void AddTransmitter(NetworkActor p_player)
		{
			DroneInputTransmitter byChannel = game.model.simulation.transmitters.GetByChannel<DroneInputTransmitter>(p_player.ID);
			if (!(byChannel is INetworkObservable) || p_player.IsSpectator)
			{
				return;
			}
			INetworkObservable observedObject = byChannel as INetworkObservable;
			bool flag = true;
			bool isLocal = p_player.IsLocal;
			if (isLocal && !(byChannel is DroneRCTransmitter))
			{
				flag = false;
			}
			if (!isLocal && !(byChannel is DroneNetworkTransmitter))
			{
				flag = false;
			}
			if (!flag)
			{
				Debug.LogWarning("NetworkGameModule > AddTransmitter - Invalid Parameters - local[" + isLocal + "] type[" + byChannel.GetType().Name + "]");
			}
			else
			{
				if (isLocal)
				{
					base.app.model.network.room.CreateLocalRacer(observedObject);
				}
				else
				{
					base.app.model.network.room.CreateRemoteRacer(p_player, observedObject);
				}
				Debug.Log(string.Format("NetworkGameModule > AddTransmitter - Added {0} Transmitter with Id: {1} ", p_player.IsLocal ? "Local" : "Remote", p_player.ID));
			}
		}

		public void AddDrone(NetworkActor p_player)
		{
			if (p_player == null)
			{
				return;
			}
			GamePlayerData pdata = game.model.GetPlayerDataById(p_player.ID);
			DroneSimulation sim = game.model.simulation;
			if (pdata == null)
			{
				pdata = new GamePlayerData();
				game.model.players.Add(pdata);
			}
			pdata.SetNetwork(p_player);
			if (p_player.IsSpectator)
			{
				return;
			}
			DroneRigData droneRigData = DroneRigData.FromJson(p_player.DroneRigData);
			controller.CreatePlayer(pdata, droneRigData ?? defaultRig, delegate(Drone p_new_drone)
			{
				p_new_drone.fc.armed = true;
				p_new_drone.SetEnabled(p_flag: true);
				sim.drones.SaveArmed(p_new_drone, p_flag: true);
				if (!p_player.IsLocal)
				{
					p_new_drone.rigidbody.isKinematic = true;
					p_new_drone.rigidbody.SetCollisionEnabled(p_flag: false);
					p_new_drone.body.frame.camera.tilt = p_player.CameraTilt;
					p_new_drone.body.frame.camera.fov = p_player.CameraFOV;
					Notify(Time.deltaTime, "network.remote.transmitter.added", pdata);
				}
				else
				{
					p_new_drone.SetMotorSpinSpeed(0f);
					Notify(Time.deltaTime, "network.local.transmitter.added");
				}
				int order = p_player.Order;
				DronePodium dronePodium = sim.podiums.Get(order);
				Debug.Log("NetworkGameModule> Podium Position - player[" + p_player.ProfileName + "] order[" + order + "] podium[" + dronePodium?.ToString() + "]");
				if ((bool)dronePodium)
				{
					p_new_drone.position = dronePodium.spawn.position;
					p_new_drone.transform.rotation = dronePodium.spawn.rotation;
				}
			});
			AddTransmitter(p_player);
		}

		public void AddPlayer(NetworkActor p_player)
		{
			_ = p_player.ID;
			string playerId = p_player.PlayerId;
			GamePlayerData gamePlayerData = game.model.GetPlayerDataById(playerId);
			if (gamePlayerData == null)
			{
				gamePlayerData = new GamePlayerData();
				game.model.players.Add(gamePlayerData);
			}
			gamePlayerData.SetNetwork(p_player);
			Debug.Log("NetworkGameModule> AddPlayer - id[" + gamePlayerData.id + "] order[" + gamePlayerData.order + "] name[" + gamePlayerData.name + "] steam-id[" + playerId + "] spectator[" + p_player.IsSpectator + "]");
		}

		public void RemovePlayer(int p_player_id)
		{
			int p_id = p_player_id;
			GamePlayerData playerDataById = game.model.GetPlayerDataById(p_id);
			if (playerDataById == null)
			{
				Debug.LogWarning("NetworkGameModule> RemovePlayer - Failed to remove player [" + p_id + "]");
				return;
			}
			DroneSimulation simulation = game.model.simulation;
			playerDataById.type = GamePlayerType.Data;
			playerDataById.id = -1;
			playerDataById.order = -1;
			if ((bool)playerDataById.drone)
			{
				simulation.drones.Remove(playerDataById.drone);
				playerDataById.drone.Destroy(p_async: true);
			}
			Debug.LogWarning("NetworkGameModule> RemovePlayer - id[" + p_id + "] order[" + playerDataById.order + "] name[" + playerDataById.name + "]");
		}
	}
}
