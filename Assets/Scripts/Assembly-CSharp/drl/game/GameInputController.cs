using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using drl.sim.rci;
using thelab.mvc;

namespace drl.game
{
	public class GameInputController : Controller<DRLApp>
	{
		public GameTypeController controller;

		public bool ready;

		public bool listening;

		public List<GameInputMapComponent> maps;

		public List<GameCommand> history;

		private List<GameCommand> ignoredCommands = new List<GameCommand>();

		public bool pausePhysics = true;

		private bool m_history_change;

		private bool m_toggledA;

		private bool m_toggledB;

		public GameController game => base.app.controller.game;

		public GameModel model => base.app.model.game;

		public GameCommand current
		{
			get
			{
				if (history.Count > 0)
				{
					return history[history.Count - 1];
				}
				return null;
			}
		}

		public bool leftAlt => Input.GetKey(KeyCode.LeftAlt);

		public bool rightAlt => Input.GetKey(KeyCode.RightAlt);

		public bool alt
		{
			get
			{
				if (!leftAlt)
				{
					return rightAlt;
				}
				return true;
			}
		}

		public bool leftCtrl
		{
			get
			{
				if (!Input.GetKey(KeyCode.LeftControl))
				{
					return Input.GetKey(KeyCode.LeftCommand);
				}
				return true;
			}
		}

		public bool rightCtrl
		{
			get
			{
				if (!Input.GetKey(KeyCode.RightControl))
				{
					return Input.GetKey(KeyCode.RightCommand);
				}
				return true;
			}
		}

		public bool leftShift => Input.GetKey(KeyCode.LeftShift);

		public bool rightShift => Input.GetKey(KeyCode.RightShift);

		public bool ctrl
		{
			get
			{
				if (!leftCtrl)
				{
					return rightCtrl;
				}
				return true;
			}
		}

		public bool shift
		{
			get
			{
				if (!leftShift)
				{
					return rightShift;
				}
				return true;
			}
		}

		public bool modified
		{
			get
			{
				if (!alt && !ctrl)
				{
					return shift;
				}
				return true;
			}
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "boot@complete":
				ready = true;
				history = new List<GameCommand>();
				m_history_change = false;
				ResetMaps();
				break;
			case "game.pause.return@click":
				Post(GameCommandType.Pause);
				break;
			}
		}

		public void ResetMaps()
		{
			maps = new List<GameInputMapComponent>();
			maps.AddRange(GetComponents<GameInputMapComponent>());
		}

		public void SetController(GameTypeController p_controller)
		{
			ResetMaps();
			controller = p_controller;
			LoadControllerMaps();
		}

		public void LoadControllerMaps()
		{
			if ((bool)controller)
			{
				maps.AddRange(controller.GetComponents<GameInputMapComponent>());
			}
		}

		public GameInputMapComponent FindMap(string p_name)
		{
			return maps.Find((GameInputMapComponent it) => it.name == p_name);
		}

		public void Post(GameCommand p_command)
		{
			if (p_command != null)
			{
				history.Add(p_command);
				if (history.Count > 50)
				{
					history.RemoveAt(0);
				}
				m_history_change = true;
			}
		}

		public void Post(GameCommandType p_type, KeyCode p_key, ConsoleButtons p_button, bool p_down, bool p_left_alt, bool p_right_alt, bool p_left_ctrl, bool p_right_ctrl, bool p_left_shift, bool p_right_shift)
		{
			GameCommand gameCommand = new GameCommand();
			gameCommand.type = p_type;
			gameCommand.key = p_key;
			gameCommand.button = p_button;
			gameCommand.down = p_down;
			gameCommand.leftAlt = p_left_alt;
			gameCommand.rightAlt = p_right_alt;
			gameCommand.leftCtrl = p_left_ctrl;
			gameCommand.rightCtrl = p_right_ctrl;
			gameCommand.leftShift = p_left_shift;
			gameCommand.rightShift = p_right_shift;
			Post(gameCommand);
		}

		public void Post(GameCommandType p_type, KeyCode p_key, bool p_down = true)
		{
			Post(p_type, p_key, (ConsoleButtons)(-1), p_down, leftAlt, rightAlt, leftCtrl, rightCtrl, leftShift, rightShift);
		}

		public void Post(GameCommandType p_type, ConsoleButtons p_button, bool p_down = true)
		{
			Post(p_type, KeyCode.None, p_button, p_down, leftAlt, rightAlt, leftCtrl, rightCtrl, leftShift, rightShift);
		}

		public void Post(GameCommandType p_type, bool p_down = true)
		{
			Post(p_type, KeyCode.None, (ConsoleButtons)(-1), p_down, leftAlt, rightAlt, leftCtrl, rightCtrl, leftShift, rightShift);
		}

		public void PostInput(GameCommandType p_type, KeyCode p_input, bool p_down = true, bool p_modified = false)
		{
			if (GetInput(p_input, p_down) && modified == p_modified)
			{
				Post(p_type, p_input, p_down);
			}
		}

		public void PostInput(GameCommandType p_type, ConsoleButtons p_input, bool p_down = true, bool p_modified = false)
		{
			if (GetInput(p_input, p_down) && modified == p_modified)
			{
				Post(p_type, p_input, p_down);
			}
		}

		protected void Update()
		{
			if (!ProcessPriorityInputCommand() || !ready || !listening)
			{
				return;
			}
			ProcessInputCommands();
			if (m_history_change)
			{
				GameCommand p_from = ((history.Count <= 1) ? null : history[history.Count - 2]);
				GameCommand gameCommand = ((history.Count <= 0) ? null : history[history.Count - 1]);
				bool flag = true;
				if ((bool)controller && controller.OnGameCommandChange(p_from, gameCommand))
				{
					flag = controller.OnGameCommand(gameCommand);
				}
				if (OnGameCommandChange(p_from, gameCommand) && flag)
				{
					OnGameCommand(gameCommand);
				}
				m_history_change = false;
			}
		}

		protected void ProcessInputCommands()
		{
			for (int i = 0; i < maps.Count; i++)
			{
				GameInputMapComponent gameInputMapComponent = maps[i];
				if ((bool)gameInputMapComponent && gameInputMapComponent.enabled)
				{
					GameCommand command = gameInputMapComponent.GetCommand();
					if (command != null && (ignoredCommands.Count <= 0 || !ignoredCommands.Contains(command)))
					{
						Post(command);
					}
				}
			}
		}

		protected bool OnGameCommandChange(GameCommand p_from, GameCommand p_to)
		{
			return true;
		}

		protected void OnGameCommand(GameCommand p_command)
		{
			if (p_command == null)
			{
				return;
			}
			switch (p_command.type)
			{
			case GameCommandType.Pause:
				Debug.Log("GameInputController> " + p_command.type);
				if (base.app.view.ui.screens.current != null && base.app.view.ui.screens.current.name == "garage-rig-edit-screen")
				{
					break;
				}
				if (!model.paused)
				{
					if (base.app.arguments.game.garage)
					{
						controller.Pause(p_flag: true, pausePhysics);
						base.app.arguments.game.garage = false;
					}
					else
					{
						model.paused = true;
						controller.Pause(p_flag: true, pausePhysics);
					}
					Debug.Log("GameInputController> Pause Notified");
					Notify("game.pause");
				}
				else if (base.app.view.ui.screens.manager.IsOpen("game-pause-screen"))
				{
					RunOnce(0.2f, delegate
					{
						Notify("game.unpause", "pause-menu");
						controller.Pause(p_flag: false, p_pause_physics: false);
					});
				}
				break;
			case GameCommandType.ResetDrone:
			{
				Debug.Log("GameInputController> " + p_command.type);
				if (model.paused)
				{
					break;
				}
				Drone drone = game.model.playerDrone;
				if (!drone && base.app.model.game.type == GameFlag.Mission)
				{
					drone = base.app.model.game.simulation.drones.Any;
				}
				if ((bool)drone && !drone.isBroken)
				{
					if (!base.app.model.storage.state.player.settings.game.armAndTurtle)
					{
						game.DroneReset(drone);
					}
					else
					{
						game.DroneTurtle(drone);
					}
					Notify("game.simulation.drone@flip");
				}
				break;
			}
			case GameCommandType.ResetDronePodium:
				_ = model.paused;
				break;
			case GameCommandType.SwitchDebugDashboard:
				Notify("game.ui.debug.dashboard@toggle");
				break;
			case GameCommandType.EditDrone:
				Notify("garage.open");
				break;
			case GameCommandType.SwitchCameraMode:
			{
				if (model.paused)
				{
					break;
				}
				DroneCamera camera2 = game.model.camera;
				if ((bool)camera2)
				{
					Drone playerDrone = game.model.playerDrone;
					if ((bool)playerDrone)
					{
						GameCameraMode p_mode = (GameCameraMode)((int)(game.GetCameraMode(camera2) + 1) % 3);
						game.SetCameraMode(playerDrone, camera2, p_mode);
					}
				}
				break;
			}
			case GameCommandType.Debug00:
				if (!base.app.inVirtualSeason && !model.paused)
				{
					DroneCamera camera = game.model.camera;
					if ((bool)camera)
					{
						camera.npsnap.enabled = !camera.npsnap.enabled;
						Debug.Log("GameInputController> Nearplane Snap Switch [" + camera.npsnap.enabled + "]");
						camera.main.nearClipPlane = 0.015f;
					}
				}
				break;
			}
		}

		protected bool ProcessPriorityInputCommand()
		{
			return true;
		}

		public void SetIgnoredCommands(List<GameCommand> p_cmd)
		{
			ignoredCommands.Clear();
			foreach (GameCommand item in p_cmd)
			{
				ignoredCommands.Add(item);
			}
		}

		public void SetIgnoredCommands()
		{
			foreach (GameInputMapComponent map in maps)
			{
				foreach (GameCommand command in map.commands)
				{
					ignoredCommands.Add(command);
				}
			}
		}

		public void ClearIgnoredCommands()
		{
			ignoredCommands.Clear();
		}

		protected bool GetInput(KeyCode k, bool d)
		{
			if (!d)
			{
				return Input.GetKeyUp(k);
			}
			return Input.GetKeyDown(k);
		}

		protected bool GetInput(ConsoleButtons k, bool d)
		{
			if (!d)
			{
				return RCI.GetButtonUp(k);
			}
			return RCI.GetButtonDown(k);
		}

		protected bool GetRCPause()
		{
			return false;
		}
	}
}
