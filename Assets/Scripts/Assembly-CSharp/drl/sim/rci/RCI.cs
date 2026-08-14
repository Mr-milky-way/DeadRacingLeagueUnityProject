using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Rewired;
using UnityEngine;
using drl.game;

namespace drl.sim.rci
{
	public static class RCI
	{
		private class TriggerData
		{
			public bool TriggerAThisFrame;

			public bool TriggerBThisFrame;

			public bool TriggerAToReset;

			public bool TriggerBToReset;

			public bool TriggeredPos;

			public bool TriggeredNeg;

			public float previousValue;

			public float previousPos;

			public float previousNeg;
		}

		public class Controller
		{
			public string guid;

			public Joystick joystick;

			public NetworkRewiredReceiver network;

			public string hardwareName;

			public string name;

			public int axisCount;

			public int buttonCount;

			public bool isNavigation;

			private ControllerPluginType pluginType;

			public bool isNetwork => pluginType == ControllerPluginType.Network;

			public Controller(Joystick p_joystick)
			{
				joystick = p_joystick;
				hardwareName = p_joystick.hardwareName;
				name = p_joystick.name;
				axisCount = p_joystick.axisCount;
				buttonCount = p_joystick.buttonCount;
				guid = joystick.hardwareIdentifier;
			}

			public Controller(NetworkRewiredReceiver p_network)
			{
				network = p_network;
				hardwareName = p_network.controllerName + "-APP";
				name = p_network.controllerSimplifiedName + "-APP";
				axisCount = p_network.axisCount;
				buttonCount = p_network.buttonCount;
				pluginType = ControllerPluginType.Network;
				guid = hardwareName;
			}

			public Controller(string p_id, string p_name, int p_axisCount, int p_buttonCount)
			{
				guid = p_id;
				hardwareName = (name = p_name);
				axisCount = p_axisCount;
				buttonCount = p_buttonCount;
				pluginType = ControllerPluginType.USBD;
			}

			public float GetAxisRaw(int p_index)
			{
				return pluginType switch
				{
					ControllerPluginType.Rewired => joystick.GetAxisRaw(p_index), 
					ControllerPluginType.Network => network.axisData[p_index], 
					_ => joystick.GetAxisRaw(p_index), 
				};
			}

			public float GetAxisDeltaRaw(int p_index)
			{
				return pluginType switch
				{
					ControllerPluginType.Rewired => joystick.Axes[p_index].valueDeltaRaw, 
					ControllerPluginType.Network => network.axisDeltaData[p_index], 
					_ => joystick.Axes[p_index].valueDeltaRaw, 
				};
			}

			public bool GetButtonById(int p_index)
			{
				switch (pluginType)
				{
				case ControllerPluginType.Rewired:
					return joystick.GetButtonById(p_index);
				case ControllerPluginType.Network:
				{
					int num = p_index - network.axisCount;
					if (num < 0 || num >= network.buttonChangedData.Length)
					{
						return false;
					}
					return network.buttonChangedData[num] == 1f;
				}
				default:
					return joystick.GetButtonById(p_index);
				}
			}

			public bool GetButtonChangedState(int p_index)
			{
				switch (pluginType)
				{
				case ControllerPluginType.Rewired:
					return joystick.Buttons[p_index].justChangedState;
				case ControllerPluginType.Network:
				{
					int num = p_index - network.axisCount;
					if (num < 0 || num >= network.buttonChangedData.Length)
					{
						return false;
					}
					return network.buttonChangedData[num] == 1f;
				}
				default:
					return joystick.Buttons[p_index].justChangedState;
				}
			}
		}

		public const int RCIVersion = 3;

		private static bool appHasFocus;

		private static Player rewiredPlayer;

		private static Controller currentJoystick;

		private static RCDeviceData joystickSettings;

		private static RCDeviceData navigationJoystickSettings;

		private static DRLApp _app;

		private static List<RCDeviceData> savedJoystickSettings;

		private static List<Controller> controllers;

		private static readonly Dictionary<RawAxis, TriggerData> triggerData;

		private const float TriggerAt = 0.75f;

		private const float ToggleAt = 0.4f;

		private const float TriggerResetAt = 0.01f;

		private const float ToggleResetRange = 0.15f;

		private static int skipFramesCounterToggle;

		private static int skipFramesCounter;

		private const int skipFramesLimitToggle = 10;

		private const int skipFramesLimit = 15;

		private const float toggleResetTime = 0.5f;

		private static float toggleResetTimer;

		private static bool toggleAButtonPrevValue;

		private static bool toggleBButtonPrevValue;

		private static bool toggledA;

		private static bool toggledB;

		private static bool toggleCooldown;

		private static float overrideYawDeadzone;

		private static RCInputManagerHolder m_manager;

		public static float throttleCap;

		private static LibUsbInput m_plugin;

		private static bool lockInput;

		public static bool Initialized { get; set; }

		public static bool IsCalibrated { get; private set; }

		public static bool UsingKeyboardAsController { get; private set; }

		public static bool HasAssignedController => currentJoystick != null;

		public static bool HasNavigationController => navigationController != null;

		public static Controller navigationController { get; private set; }

		private static DRLApp app
		{
			get
			{
				if ((bool)_app)
				{
					return _app;
				}
				GameObject gameObject = GameObject.Find("app");
				if (!gameObject)
				{
					return null;
				}
				_app = gameObject.GetComponent<DRLApp>();
				return _app;
			}
		}

		private static ControllerProfileStateModel cpsm
		{
			get
			{
				if (!app)
				{
					return null;
				}
				return app.model.storage.state.player.settings.controller;
			}
		}

		private static RCDeviceData defaultXbox => new RCDeviceData(DefaultControllerType.XBox, custom: false, "default", currentJoystick?.guid.ToString());

		private static RCDeviceData defaultPS => new RCDeviceData(DefaultControllerType.PS, custom: false, "default", currentJoystick?.guid.ToString());

		private static RCDeviceData defaultTaranis => new RCDeviceData(DefaultControllerType.Taranis, custom: false, "default", currentJoystick?.guid.ToString());

		private static RCDeviceData defaultNikko => new RCDeviceData(DefaultControllerType.Nikko, custom: false, "default", currentJoystick?.guid.ToString());

		public static RCInputManagerHolder manager
		{
			get
			{
				if (m_manager == null)
				{
					m_manager = GameObject.Find("RCInputManager").GetComponent<RCInputManagerHolder>();
				}
				return m_manager;
			}
		}

		static RCI()
		{
			triggerData = new Dictionary<RawAxis, TriggerData>
			{
				{
					RawAxis.LeftStickX,
					new TriggerData()
				},
				{
					RawAxis.LeftStickY,
					new TriggerData()
				},
				{
					RawAxis.RightStickX,
					new TriggerData()
				},
				{
					RawAxis.RightStickY,
					new TriggerData()
				},
				{
					RawAxis.ToggleA,
					new TriggerData()
				},
				{
					RawAxis.ToggleB,
					new TriggerData()
				}
			};
			skipFramesCounterToggle = 10;
			skipFramesCounter = 15;
			toggleResetTimer = 0f;
			overrideYawDeadzone = -1f;
			throttleCap = -1f;
			Initialize();
		}

		public static void Initialize()
		{
			GameObject gameObject = Resources.Load<GameObject>("RCInputManager");
			GameObject gameObject2 = (gameObject ? UnityEngine.Object.Instantiate(gameObject) : null);
			if (!gameObject2)
			{
				Debug.LogError("RCInput> Failed to find RCInputManager on Resources!");
				return;
			}
			gameObject2.name = "RCInputManager";
			m_manager = gameObject2.GetComponent<RCInputManagerHolder>();
			m_plugin = gameObject2.GetComponent<LibUsbInput>();
			m_manager.OnInstantiating(delegate
			{
				rewiredPlayer = ((ReInput.players == null) ? null : ReInput.players.GetPlayer(0));
				ReInput.ControllerConnectedEvent += OnConnectedEvent;
				ReInput.ControllerPreDisconnectEvent += OnPreDisconnectedEvent;
				ReInput.ControllerDisconnectedEvent += OnDisconnectedEvent;
				savedJoystickSettings = new List<RCDeviceData>();
				controllers = new List<Controller>();
				if ((bool)cpsm)
				{
					foreach (RCDeviceData profile in cpsm.profiles)
					{
						savedJoystickSettings.Add(profile);
					}
				}
				Initialized = true;
				appHasFocus = true;
				Controller controller = null;
				if (ReInput.controllers != null)
				{
					foreach (Joystick joystick in ReInput.controllers.Joysticks)
					{
						Controller controller2 = new Controller(joystick);
						OnConnectedEvent(controller2, appNotify: false);
						if ((bool)app)
						{
							app.Notify("settings.controller.connect", controller2.hardwareName);
						}
						if (HasSavedProfile(controller2.guid))
						{
							controller = controller2;
						}
					}
				}
				if (controller != null)
				{
					OnConnectedEvent(controller);
				}
				UsingKeyboardAsController = ControllersConnectedCount() == 0 || (ControllersConnectedCount() > 0 && !IsCalibrated);
				ResetButtonToggleData();
				if ((bool)app && app.model.storage.state.player.profile.usingTransmitterAdapter)
				{
					SetupTransmitterSettings();
				}
				if ((bool)app)
				{
					bool num = app.model.storage.state.player.garage.IsOfficial();
					bool flag = app.model.storage.state.player.garage.CanUseDamage();
					SetThrottleCap((num || flag) ? 80f : (-1f));
				}
			}, delegate(bool hasFocus)
			{
				appHasFocus = hasFocus;
			}, delegate
			{
				UpdateAxisTriggerData();
				UpdateButtonToggleData();
			}, delegate
			{
			});
		}

		private static void UpdateAxisTriggerData()
		{
			foreach (KeyValuePair<RawAxis, TriggerData> triggerDatum in triggerData)
			{
				if (!HasNavigationController || !appHasFocus)
				{
					break;
				}
				triggerDatum.Value.TriggerAThisFrame = false;
				triggerDatum.Value.TriggerBThisFrame = false;
				if (triggerDatum.Value.TriggerAToReset)
				{
					if (triggerDatum.Key == RawAxis.ToggleA || triggerDatum.Key == RawAxis.ToggleB)
					{
						float assignedAxis = GetAssignedAxis(triggerDatum.Key, currentJoystick, excludeZeroThrottle: true);
						if (assignedAxis > -0.15f && assignedAxis < 0.15f)
						{
							triggerDatum.Value.TriggerAToReset = false;
						}
					}
					else if (GetAssignedAxis(triggerDatum.Key, navigationController, excludeZeroThrottle: true) < 0.01f)
					{
						triggerDatum.Value.TriggerAToReset = false;
					}
				}
				if (triggerDatum.Value.TriggerBToReset)
				{
					if (triggerDatum.Key == RawAxis.ToggleA || triggerDatum.Key == RawAxis.ToggleB)
					{
						float assignedAxis2 = GetAssignedAxis(triggerDatum.Key, currentJoystick, excludeZeroThrottle: true);
						if (assignedAxis2 > -0.15f && assignedAxis2 < 0.15f)
						{
							triggerDatum.Value.TriggerBToReset = false;
						}
					}
					else if (GetAssignedAxis(triggerDatum.Key, navigationController, excludeZeroThrottle: true) < 0.01f)
					{
						triggerDatum.Value.TriggerBToReset = false;
					}
				}
				if (triggerDatum.Key == RawAxis.ToggleA || triggerDatum.Key == RawAxis.ToggleB)
				{
					if (skipFramesCounterToggle > 0)
					{
						skipFramesCounterToggle--;
						continue;
					}
					skipFramesCounterToggle = 10;
					triggerDatum.Value.previousValue = GetAssignedAxis(triggerDatum.Key, currentJoystick, excludeZeroThrottle: true);
					continue;
				}
				if (skipFramesCounter > 0)
				{
					skipFramesCounter--;
					continue;
				}
				skipFramesCounter = 15;
				float assignedAxis3 = GetAssignedAxis(triggerDatum.Key, navigationController, excludeZeroThrottle: true);
				if (assignedAxis3 > 0f)
				{
					triggerDatum.Value.previousPos = assignedAxis3;
				}
				else
				{
					triggerDatum.Value.previousNeg = assignedAxis3;
				}
			}
		}

		private static void ResetButtonToggleData()
		{
			toggledA = false;
			toggledB = false;
			toggleCooldown = false;
			if (currentJoystick != null && joystickSettings != null)
			{
				int toggleAsButtonA = joystickSettings.toggleAsButtonA;
				int toggleAsButtonB = joystickSettings.toggleAsButtonB;
				toggleAsButtonA += currentJoystick.axisCount;
				toggleAsButtonB += currentJoystick.axisCount;
				if (toggleAsButtonA > -1)
				{
					toggleAButtonPrevValue = GetButtonRawIndex(toggleAsButtonA, currentJoystick);
				}
				if (toggleAsButtonB > -1)
				{
					toggleBButtonPrevValue = GetButtonRawIndex(toggleAsButtonB, currentJoystick);
				}
			}
		}

		private static void UpdateButtonToggleData()
		{
			toggleResetTimer += Time.deltaTime;
			if (toggleResetTimer > 0.5f)
			{
				toggleResetTimer = 0f;
				if (toggledA || toggledB || toggleCooldown)
				{
					ResetButtonToggleData();
				}
			}
			if (toggleCooldown || currentJoystick == null || joystickSettings == null)
			{
				return;
			}
			int toggleAsButtonA = joystickSettings.toggleAsButtonA;
			int toggleAsButtonB = joystickSettings.toggleAsButtonB;
			if (toggleAsButtonA > -1)
			{
				toggleAsButtonA += currentJoystick.axisCount;
				if (GetButtonRawIndex(toggleAsButtonA, currentJoystick) != toggleAButtonPrevValue)
				{
					toggledA = true;
				}
				toggleAButtonPrevValue = GetButtonRawIndex(toggleAsButtonA, currentJoystick);
			}
			if (toggleAsButtonB > -1)
			{
				toggleAsButtonB += currentJoystick.axisCount;
				if (GetButtonRawIndex(toggleAsButtonB, currentJoystick) != toggleBButtonPrevValue)
				{
					toggledB = true;
				}
				toggleBButtonPrevValue = GetButtonRawIndex(toggleAsButtonB, currentJoystick);
			}
		}

		private static void OnConnectedEvent(ControllerStatusChangedEventArgs args)
		{
			if (Initialized && args.controllerType == ControllerType.Joystick && ReInput.controllers != null)
			{
				OnConnectedEvent(new Controller(ReInput.controllers.GetJoystick(args.controllerId)));
			}
		}

		private static void OnConnectedEvent(Controller joystick, bool appNotify = true)
		{
			if (Initialized)
			{
				string text = joystick?.hardwareName ?? "keyboard";
				string text2 = joystick?.name ?? "keyboard";
				if (joystick != null)
				{
					Debug.Log($"RCInput> OnConnectedEvent / hardware[{text}] name[{text2}] axis count[{joystick.axisCount}] buttons count[{joystick.buttonCount}] guid[{joystick.guid}]");
				}
				currentJoystick = joystick;
				SetupControllerSettings(joystick);
				if (appNotify && (bool)app)
				{
					app.Notify("settings.controller.connect", joystick?.hardwareName ?? "keyboard");
				}
				UsingKeyboardAsController = joystick == null || ControllersConnectedCount() == 0 || (ControllersConnectedCount() > 0 && !IsCalibrated);
				ResetTriggerData();
			}
		}

		private static void OnPreDisconnectedEvent(ControllerStatusChangedEventArgs args)
		{
			if (!Initialized || args.controllerType != ControllerType.Joystick || ReInput.controllers == null)
			{
				return;
			}
			Joystick joystick = ReInput.controllers.GetJoystick(args.controllerId);
			if (joystick == null)
			{
				return;
			}
			Controller controller = new Controller(joystick);
			for (int i = 0; i < controllers.Count; i++)
			{
				if (controller.guid == controllers[i].guid)
				{
					controllers.RemoveAt(i);
				}
			}
			string text = controller?.hardwareName ?? "keyboard";
			string text2 = controller?.name ?? "keyboard";
			Debug.Log("RCInput> OnPreDisconnectedEvent / hardware[" + text + "] name[" + text2 + "]");
			string hardwareName = controller.hardwareName;
			string hardwareIdentifier = joystick.hardwareIdentifier;
			if (navigationController != null && navigationController.guid == controller.guid)
			{
				navigationController = null;
				navigationJoystickSettings = null;
			}
			if (controller.guid == currentJoystick.guid)
			{
				currentJoystick = null;
				IsCalibrated = false;
				joystickSettings = null;
				joystick = null;
				controller = null;
			}
			ResetTriggerData();
			if ((bool)app)
			{
				app.Notify("settings.controller.predisconnect", hardwareName);
			}
			if (ControllersConnectedCount() <= 0)
			{
				return;
			}
			foreach (Controller controller2 in controllers)
			{
				if (!(hardwareIdentifier == controller2.guid))
				{
					controller = controller2;
					if (HasSavedProfile(controller.guid))
					{
						break;
					}
				}
			}
			if (controller != null)
			{
				IsCalibrated = HasSavedProfile();
				if (IsCalibrated)
				{
					joystickSettings = GetSavedProfile();
				}
				OnConnectedEvent(controller);
			}
		}

		private static void OnDisconnectedEvent(ControllerStatusChangedEventArgs args)
		{
			if (Initialized && args.controllerType == ControllerType.Joystick)
			{
				if ((bool)app)
				{
					app.Notify("settings.controller.disconnect");
				}
				UsingKeyboardAsController = ControllersConnectedCount() == 0 || (ControllersConnectedCount() > 0 && !IsCalibrated);
				ResetTriggerData();
			}
		}

		private static void OnDisconnectedEvent(Controller p_controller)
		{
			if (!Initialized)
			{
				return;
			}
			Controller controller = null;
			string text = "";
			controller = p_controller;
			string text2 = controller?.hardwareName ?? "keyboard";
			string text3 = controller?.name ?? "keyboard";
			Debug.Log("RCInput> OnDisconnectedEvent / hardware[" + text2 + "] name[" + text3 + "]");
			text = controller.hardwareName;
			string guid = controller.guid;
			controllers.Remove(controller);
			Debug.Log(controller.guid);
			Debug.Log(currentJoystick.guid);
			if (controller.guid == currentJoystick.guid)
			{
				currentJoystick = null;
				IsCalibrated = false;
				joystickSettings = null;
				controller = null;
			}
			if ((bool)app)
			{
				app.Notify("settings.controller.predisconnect", text);
			}
			if ((bool)app)
			{
				app.Notify("settings.controller.disconnect");
			}
			UsingKeyboardAsController = ControllersConnectedCount() == 0 || (ControllersConnectedCount() > 0 && !IsCalibrated);
			if (ControllersConnectedCount() > 0)
			{
				foreach (Controller controller2 in controllers)
				{
					if (!(guid == controller2.guid))
					{
						controller = controller2;
						if (HasSavedProfile(controller.guid))
						{
							break;
						}
					}
				}
				if (controller != null)
				{
					IsCalibrated = HasSavedProfile();
					if (IsCalibrated)
					{
						joystickSettings = GetSavedProfile();
					}
					OnConnectedEvent(controller);
				}
			}
			ResetTriggerData();
		}

		public static void SetupTransmitterSettings()
		{
			if ((Initialized || currentJoystick != null) && currentJoystick != null)
			{
				SetupControllerSettings(currentJoystick, using_adapter: true);
				if ((bool)app)
				{
					app.Notify("settings.controller.connect", currentJoystick.hardwareName);
				}
			}
		}

		public static void SetupGamepadSettings()
		{
			if (!Initialized && navigationController == null)
			{
				return;
			}
			DefaultControllerType defaultControllerType = DefaultControllerType.None;
			if (defaultControllerType == DefaultControllerType.None)
			{
				return;
			}
			RCDeviceData rCDeviceData = null;
			foreach (RCDeviceData savedJoystickSetting in savedJoystickSettings)
			{
				if (savedJoystickSetting.defaultControllerType == defaultControllerType && savedJoystickSetting.hardwareName != "TRANSMITTER")
				{
					rCDeviceData = savedJoystickSetting;
					break;
				}
			}
			if (rCDeviceData != null)
			{
				currentJoystick.hardwareName = rCDeviceData.hardwareName;
				SetupControllerSettings(currentJoystick);
				if ((bool)app)
				{
					app.Notify("settings.controller.connect", currentJoystick.hardwareName);
				}
			}
		}

		private static void SetupControllerSettings(Controller joystick, bool using_adapter = false)
		{
			if (!Initialized || joystick == null)
			{
				return;
			}
			if (using_adapter)
			{
				joystick.hardwareName = "TRANSMITTER";
			}
			bool flag = false;
			foreach (Controller controller in controllers)
			{
				if (controller.guid == joystick.guid)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				controllers.Add(joystick);
			}
			RCDeviceData rCDeviceData = null;
			foreach (RCDeviceData savedJoystickSetting in savedJoystickSettings)
			{
				if (savedJoystickSetting != null && !(savedJoystickSetting.guid != joystick.guid.ToString()))
				{
					rCDeviceData = savedJoystickSetting;
					break;
				}
			}
			if (rCDeviceData == null)
			{
				foreach (RCDeviceData savedJoystickSetting2 in savedJoystickSettings)
				{
					if (savedJoystickSetting2 != null && !(savedJoystickSetting2.hardwareName.ToLower() != joystick.hardwareName.ToLower()))
					{
						rCDeviceData = savedJoystickSetting2;
						break;
					}
				}
			}
			if (rCDeviceData != null)
			{
				joystickSettings = rCDeviceData;
				IsCalibrated = true;
				bool flag2 = joystickSettings.defaultControllerType == DefaultControllerType.XBox;
				bool flag3 = joystickSettings.defaultControllerType == DefaultControllerType.PS;
				if (flag3 || flag2)
				{
					navigationController = new Controller(joystick.joystick);
					navigationController.isNavigation = true;
					navigationJoystickSettings = ((navigationJoystickSettings != null) ? navigationJoystickSettings : (flag3 ? defaultPS : defaultXbox));
					rewiredPlayer?.controllers?.ClearAllControllers();
					rewiredPlayer?.controllers?.AddController<Joystick>(joystick.joystick.id, removeFromOtherPlayers: false);
				}
				return;
			}
			string text = joystick.hardwareName.ToLower();
			if (text.Contains("taranis") || text.Contains("sky"))
			{
				Debug.Log("RCI Found taranis and loaded default, needs calibration.");
				joystickSettings = defaultTaranis;
				joystickSettings.hardwareName = text;
				IsCalibrated = false;
				return;
			}
			if (text.Contains("nikko"))
			{
				Debug.Log("RCI Found nikko and loaded default, needs calibration.");
				joystickSettings = defaultNikko;
				joystickSettings.hardwareName = text;
				IsCalibrated = false;
				return;
			}
			if (!joystick.isNetwork)
			{
				if (m_manager.IsSupported(joystick.name))
				{
					Debug.Log("RCI Found supported gamepad " + joystick.name + " and loaded default settings.");
					if (text.Contains("sony") || text.Contains("dual") || text.Contains("shock") || text.Contains("play") || text.Contains("station") || text.Contains("wireless"))
					{
						joystickSettings = (using_adapter ? new RCDeviceData(DefaultControllerType.PS, custom: false, "TRANSMITTER", joystick.joystick.id.ToString(), uses_adapter: true) : defaultPS);
						navigationJoystickSettings = defaultPS;
					}
					else
					{
						joystickSettings = (using_adapter ? new RCDeviceData(DefaultControllerType.XBox, custom: false, "TRANSMITTER", joystick.joystick.id.ToString(), uses_adapter: true) : defaultXbox);
						navigationJoystickSettings = defaultXbox;
					}
					joystickSettings.hardwareName = text;
					IsCalibrated = true;
					savedJoystickSettings.Add(joystickSettings);
					if ((bool)cpsm)
					{
						cpsm.profiles = savedJoystickSettings;
					}
					navigationController = new Controller(joystick.joystick);
					navigationController.isNavigation = true;
					return;
				}
			}
			else
			{
				string p_name = joystick.name.Replace("-APP", "");
				if (m_manager.IsSupported(p_name))
				{
					DefaultControllerType dct = DefaultControllerType.XBox;
					if (text.Contains("sony") || text.Contains("dual") || text.Contains("shock") || text.Contains("play") || text.Contains("station"))
					{
						dct = DefaultControllerType.PS;
					}
					joystickSettings = new RCDeviceData(dct, custom: true, "default", joystick.guid);
					joystickSettings.hardwareName = text;
					IsCalibrated = false;
					cpsm.profiles = savedJoystickSettings;
					return;
				}
			}
			Debug.Log("RCI Unknown " + joystick.hardwareName + ", needs calibration");
			IsCalibrated = false;
			joystickSettings = defaultTaranis;
			joystickSettings.hardwareName = text;
		}

		public static bool SetActiveControllerFromIndex(CalibrationData calibrationData)
		{
			if (!Initialized || currentJoystick == null || joystickSettings == null || calibrationData == null)
			{
				return false;
			}
			savedJoystickSettings.Remove(joystickSettings);
			RCDeviceData rCDeviceData = new RCDeviceData(joystickSettings.defaultControllerType, custom: true, joystickSettings.hardwareName, currentJoystick.guid);
			foreach (RawAxis key in calibrationData.ElementIDs.Keys)
			{
				int num = calibrationData.ElementIDs[key];
				rCDeviceData.GetAAD(key).ElementID = num;
				if (num >= currentJoystick.axisCount)
				{
					if (key == RawAxis.ToggleA)
					{
						rCDeviceData.toggleAsButtonA = num - currentJoystick.axisCount;
					}
					if (key == RawAxis.ToggleB)
					{
						rCDeviceData.toggleAsButtonB = num - currentJoystick.axisCount;
					}
				}
				else if (num >= 0)
				{
					switch (key)
					{
					case RawAxis.ToggleA:
						rCDeviceData.toggleAsButtonA = -1;
						break;
					case RawAxis.ToggleB:
						rCDeviceData.toggleAsButtonB = -1;
						break;
					}
				}
				if ((key != RawAxis.ToggleA && key != RawAxis.ToggleB) || (key == RawAxis.ToggleA && rCDeviceData.toggleAsButtonA == -1) || (key == RawAxis.ToggleB && rCDeviceData.toggleAsButtonB == -1))
				{
					if (num >= 0 && num < calibrationData.Centers.Length)
					{
						rCDeviceData.GetAAD(key).center = calibrationData.Centers[num];
					}
					rCDeviceData.GetAAD(key).min = calibrationData.RangeMin[key];
					rCDeviceData.GetAAD(key).max = calibrationData.RangeMax[key];
					rCDeviceData.GetAAD(key).inverted = calibrationData.Invert[key];
					rCDeviceData.GetAAD(key).zeroThrottle = ((key == RawAxis.LeftStickY) ? calibrationData.ZeroThrottle : (-2f));
					if (calibrationData.Deadzone.ContainsKey(key))
					{
						rCDeviceData.GetAAD(key).deadzone = calibrationData.Deadzone[key];
					}
				}
			}
			rCDeviceData.usingCustomXMLmap = true;
			savedJoystickSettings.Add(rCDeviceData);
			cpsm.profiles = savedJoystickSettings;
			joystickSettings = rCDeviceData;
			SetupControllerSettings(currentJoystick);
			UsingKeyboardAsController = false;
			if (currentJoystick != null && currentJoystick.isNetwork)
			{
				currentJoystick.network.SendCalibrationUpdate();
			}
			return true;
		}

		public static void SetActiveControllerFromIndex(int index)
		{
			if (Initialized && index >= 0 && index < controllers.Count)
			{
				currentJoystick = null;
				IsCalibrated = false;
				joystickSettings = null;
				OnConnectedEvent(controllers[index], appNotify: false);
			}
		}

		public static void SetActiveController(Controller p_active)
		{
			if (Initialized)
			{
				currentJoystick = null;
				IsCalibrated = false;
				joystickSettings = null;
				OnConnectedEvent(p_active, appNotify: false);
				if ((bool)app)
				{
					app.Notify("input.active-controller.changed");
				}
			}
		}

		public static void SetActiveControllerMobile(NetworkRewiredReceiver p_nrr)
		{
			if (Initialized)
			{
				OnConnectedEvent(new Controller(p_nrr));
				if ((bool)app)
				{
					app.Notify("input.active-controller.changed");
				}
			}
		}

		public static void DisconnectControllerMobile(NetworkRewiredReceiver p_nrr)
		{
			if (!Initialized)
			{
				return;
			}
			Controller controller = new Controller(p_nrr);
			bool flag = false;
			foreach (Controller controller2 in controllers)
			{
				if (controller2.hardwareName == controller.hardwareName)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				OnDisconnectedEvent(controller);
			}
		}

		public static bool IsRCController()
		{
			if (!Initialized || currentJoystick == null)
			{
				return false;
			}
			ControllerStateType controllerStateType = GetControllerStateType(ControllerStateType.XBox);
			if (controllerStateType != ControllerStateType.XBox && controllerStateType != ControllerStateType.PS4)
			{
				return !currentJoystick.isNetwork;
			}
			return false;
		}

		public static bool NavigationIsGamepad()
		{
			if (!Initialized || navigationController == null || navigationJoystickSettings == null)
			{
				return false;
			}
			if (navigationJoystickSettings.defaultControllerType != DefaultControllerType.XBox)
			{
				return navigationJoystickSettings.defaultControllerType == DefaultControllerType.PS;
			}
			return true;
		}

		public static ControllerStateType GetControllerStateType(ControllerStateType defaultOnNone, RCDeviceData customData = null)
		{
			if (!Initialized)
			{
				return defaultOnNone;
			}
			DefaultControllerType defaultControllerType = GetDefaultControllerType((DefaultControllerType)(defaultOnNone + 1), customData);
			if (defaultControllerType == DefaultControllerType.Nikko)
			{
				return ControllerStateType.Nikko;
			}
			return (ControllerStateType)(defaultControllerType - 1);
		}

		public static DefaultControllerType GetDefaultControllerType(DefaultControllerType defaultOnNone, RCDeviceData customData = null)
		{
			if (!Initialized)
			{
				return defaultOnNone;
			}
			if (customData != null)
			{
				if (customData.defaultControllerType != DefaultControllerType.None)
				{
					return customData.defaultControllerType;
				}
				return defaultOnNone;
			}
			if (joystickSettings == null || joystickSettings.defaultControllerType == DefaultControllerType.None)
			{
				return defaultOnNone;
			}
			return joystickSettings.defaultControllerType;
		}

		public static string GetSimplifiedControllerName(int idx = -1)
		{
			if (!Initialized)
			{
				return "NO CONTROLLER";
			}
			string text = "";
			try
			{
				text = ((idx < 0) ? GetControllerHardwareName().ToLower() : GetHardwareName(idx).ToLower());
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"RCInput> GetSimplifiedControllerName / idx[{idx}] failed to get name!\n  {ex.Message}");
				text = "transmitter";
			}
			bool flag = false;
			Controller controller = ((idx < 0) ? currentJoystick : controllers[idx]);
			if (controller != null)
			{
				flag = controller.isNetwork;
			}
			string text2 = "";
			if (Regex.IsMatch(text, " pad|gamepad"))
			{
				text2 = "gamepad";
			}
			if (Regex.IsMatch(text, "nikko"))
			{
				text2 = "nikko";
			}
			if (Regex.IsMatch(text, "xbox|microsoft|360|one"))
			{
				text2 = "xbox";
			}
			if (Regex.IsMatch(text, "sony|dual|shock|play|station|wireless"))
			{
				text2 = "playstation";
			}
			if (Regex.IsMatch(text, "taranis|sky"))
			{
				text2 = "taranis";
			}
			switch (text2)
			{
			case "xbox":
				text = (flag ? "xbox app" : "xbox");
				break;
			case "nikko":
				text = text2;
				break;
			case "playstation":
				text = text2;
				break;
			case "taranis":
				text = text2;
				break;
			case "gamepad":
				text = text2;
				break;
			}
			Debug.Log($"RCInput> GetSimplifiedControllerName / idx[{idx}] hardware-name[{text}] is-network[{flag}]");
			return text.ToUpper();
		}

		public static string GetControllerHardwareName()
		{
			if (!Initialized)
			{
				return "no controller";
			}
			return currentJoystick?.hardwareName ?? "no controller";
		}

		public static IList<Controller> GetControllers()
		{
			if (!Initialized)
			{
				return null;
			}
			return controllers;
		}

		public static Controller GetController(int p_idx)
		{
			if (p_idx >= 0 || p_idx < controllers.Count)
			{
				return controllers[p_idx];
			}
			return null;
		}

		public static string GetControllerGUID(int p_idx)
		{
			if (p_idx >= 0 || p_idx < controllers.Count)
			{
				return controllers[p_idx].guid;
			}
			return "";
		}

		public static string GetControllerGUID()
		{
			if (Initialized && currentJoystick != null)
			{
				return currentJoystick.guid;
			}
			return "";
		}

		public static bool HasControllersConnected()
		{
			if (Initialized)
			{
				return controllers.Count > 0;
			}
			return false;
		}

		public static int ControllersConnectedCount()
		{
			if (!Initialized)
			{
				return 0;
			}
			return controllers.Count;
		}

		public static string GetHardwareName(int index)
		{
			if (!Initialized || index >= controllers.Count)
			{
				return "";
			}
			return controllers[index].hardwareName;
		}

		public static string GetJoystickName(int index)
		{
			if (!Initialized || index >= controllers.Count)
			{
				return "";
			}
			return controllers[index].name;
		}

		public static bool HasSavedProfile(string p_guid = "")
		{
			if (!Initialized)
			{
				return false;
			}
			if (string.IsNullOrEmpty(p_guid))
			{
				p_guid = GetControllerGUID();
			}
			return savedJoystickSettings.Any((RCDeviceData o) => o.guid == p_guid);
		}

		public static RCDeviceData GetSavedProfile(string p_guid = "")
		{
			if (!Initialized && currentJoystick == null)
			{
				return null;
			}
			RCDeviceData result = null;
			if (string.IsNullOrEmpty(p_guid))
			{
				p_guid = GetControllerGUID();
			}
			foreach (RCDeviceData savedJoystickSetting in savedJoystickSettings)
			{
				if (savedJoystickSetting.guid == p_guid)
				{
					result = savedJoystickSetting;
				}
			}
			return result;
		}

		public static void Unsubscribe()
		{
			ReInput.ControllerConnectedEvent -= OnConnectedEvent;
			ReInput.ControllerDisconnectedEvent -= OnDisconnectedEvent;
			ReInput.ControllerPreDisconnectEvent -= OnPreDisconnectedEvent;
		}

		public static ControllerMap GetControllerMap()
		{
			if (!Initialized || currentJoystick == null || currentJoystick.isNetwork || rewiredPlayer == null)
			{
				return null;
			}
			return rewiredPlayer.controllers.maps.GetMap(currentJoystick.joystick, 0, 0);
		}

		public static bool IsSupported(string p_hardwareName)
		{
			return m_manager.IsSupported(p_hardwareName);
		}

		public static bool IsUsingTransmitterSettings()
		{
			if (!Initialized || currentJoystick == null)
			{
				return false;
			}
			if (!app)
			{
				return false;
			}
			if (!app.model)
			{
				return false;
			}
			if (!app.model.storage)
			{
				return false;
			}
			return app.model.storage.state.player.profile.usingTransmitterAdapter;
		}

		public static void OverrideYawDeadzone(float p_deadzone = 0.1f)
		{
			overrideYawDeadzone = p_deadzone;
		}

		public static void ClearYawDeadzoneOverride()
		{
			overrideYawDeadzone = -1f;
		}

		public static float GetDJIModeThrottle(Controller p_controller = null)
		{
			if (Initialized)
			{
				return GetAssignedAxis(AssignedAxis.Throttle, p_controller, excludeZeroThrottle: true);
			}
			return 0f;
		}

		public static SignalVector GetAssignedSignalVector(Controller p_controller)
		{
			if (!Initialized || p_controller == null)
			{
				return default(SignalVector);
			}
			return new SignalVector
			{
				altitude = GetDJIModeThrottle(p_controller),
				throttle = GetAssignedAxis(AssignedAxis.Throttle, p_controller),
				yaw = GetAssignedAxis(AssignedAxis.Yaw, p_controller),
				pitch = GetAssignedAxis(AssignedAxis.Pitch, p_controller),
				roll = GetAssignedAxis(AssignedAxis.Roll, p_controller)
			};
		}

		public static Vector4 GetRawAxis(Controller p_controller = null)
		{
			if (!Initialized)
			{
				return default(Vector4);
			}
			if (p_controller == null)
			{
				p_controller = currentJoystick;
			}
			if (p_controller == null)
			{
				return default(Vector4);
			}
			return new Vector4(GetAssignedAxis(RawAxis.LeftStickX, p_controller), GetAssignedAxis(RawAxis.LeftStickY, p_controller), GetAssignedAxis(RawAxis.RightStickX, p_controller), GetAssignedAxis(RawAxis.RightStickY, p_controller));
		}

		public static bool GetAxisTrigger(RawAxis rawAxis, bool isPositiveSign)
		{
			if (!Initialized || navigationController == null || !appHasFocus)
			{
				return false;
			}
			float assignedAxis = GetAssignedAxis(rawAxis, navigationController, excludeZeroThrottle: true);
			if (isPositiveSign)
			{
				if (assignedAxis < 0.15f)
				{
					triggerData[rawAxis].previousPos = 0f;
					triggerData[rawAxis].TriggeredPos = false;
					return false;
				}
				if (assignedAxis - triggerData[rawAxis].previousPos > 0.75f || triggerData[rawAxis].TriggeredPos)
				{
					triggerData[rawAxis].previousPos = assignedAxis;
					skipFramesCounter = 15;
					triggerData[rawAxis].TriggeredPos = true;
					return true;
				}
				return false;
			}
			if (assignedAxis > -0.15f)
			{
				triggerData[rawAxis].previousNeg = 0f;
				triggerData[rawAxis].TriggeredNeg = false;
				return false;
			}
			if (triggerData[rawAxis].previousNeg - assignedAxis > 0.75f || triggerData[rawAxis].TriggeredNeg)
			{
				triggerData[rawAxis].previousNeg = assignedAxis;
				skipFramesCounter = 15;
				triggerData[rawAxis].TriggeredNeg = true;
				return true;
			}
			return false;
		}

		public static bool GetAxisToggle(RawAxis toggle, bool p_positiveDirection = false)
		{
			if (!Initialized || (toggle != RawAxis.ToggleA && toggle != RawAxis.ToggleB) || !HasAssignedController || joystickSettings == null || !appHasFocus)
			{
				return false;
			}
			AssignedAxisData aAD = joystickSettings.GetAAD(toggle);
			if (aAD == null)
			{
				return false;
			}
			if (!IsRCController() && ((toggle == RawAxis.ToggleA && joystickSettings.toggleAsButtonA > -1) || (toggle == RawAxis.ToggleB && joystickSettings.toggleAsButtonB > -1)))
			{
				return GetToggle(toggle);
			}
			if (((toggle == RawAxis.ToggleA && joystickSettings.toggleAsButtonA == -1) || (toggle == RawAxis.ToggleB && joystickSettings.toggleAsButtonB == -1)) && (p_positiveDirection ? (GetAssignedAxis(toggle, currentJoystick, excludeZeroThrottle: true) - triggerData[toggle].previousValue) : Mathf.Abs(GetAssignedAxis(toggle, currentJoystick, excludeZeroThrottle: true) - triggerData[toggle].previousValue)) > 0.4f * Mathf.Abs(aAD.max - aAD.min))
			{
				skipFramesCounterToggle = 10;
				triggerData[toggle].previousValue = GetAssignedAxis(toggle, currentJoystick, excludeZeroThrottle: true);
				return true;
			}
			return GetToggle(toggle);
		}

		private static void ResetTriggerData()
		{
			if (!HasNavigationController)
			{
				return;
			}
			skipFramesCounterToggle = 10;
			skipFramesCounter = 15;
			ResetButtonToggleData();
			foreach (KeyValuePair<RawAxis, TriggerData> triggerDatum in triggerData)
			{
				triggerDatum.Value.previousValue = GetAssignedAxis(triggerDatum.Key, navigationController, excludeZeroThrottle: true);
				float assignedAxis = GetAssignedAxis(triggerDatum.Key, navigationController, excludeZeroThrottle: true);
				if (assignedAxis > 0f)
				{
					triggerDatum.Value.previousPos = assignedAxis;
				}
				else
				{
					triggerDatum.Value.previousNeg = assignedAxis;
				}
			}
		}

		public static float GetAssignedAxis(RawAxis rawAxis, Controller p_controller = null, bool excludeZeroThrottle = false)
		{
			if (p_controller == null)
			{
				p_controller = currentJoystick;
			}
			if (lockInput)
			{
				return 0f;
			}
			if (p_controller == null)
			{
				return 0f;
			}
			return rawAxis switch
			{
				RawAxis.LeftStickX => GetAssignedAxis(AssignedAxis.Yaw, p_controller), 
				RawAxis.LeftStickY => GetAssignedAxis(AssignedAxis.Throttle, p_controller, excludeZeroThrottle), 
				RawAxis.RightStickX => GetAssignedAxis(AssignedAxis.Roll, p_controller), 
				RawAxis.RightStickY => GetAssignedAxis(AssignedAxis.Pitch, p_controller), 
				RawAxis.ToggleA => GetRawAxis(RawAxis.ToggleA, p_controller), 
				RawAxis.ToggleB => GetRawAxis(RawAxis.ToggleB, p_controller), 
				_ => throw new ArgumentOutOfRangeException("rawAxis"), 
			};
		}

		public static float GetAssignedAxis(AssignedAxis assignedAxis, Controller p_controller = null, bool excludeZeroThrottle = false, bool useThrottleCap = false)
		{
			if (!Initialized)
			{
				return 0f;
			}
			if (p_controller == null)
			{
				p_controller = currentJoystick;
			}
			if (!appHasFocus)
			{
				if (assignedAxis != AssignedAxis.Throttle)
				{
					return 0f;
				}
				return -1f;
			}
			RCDeviceData rCDeviceData = ((p_controller != null && p_controller.isNavigation) ? navigationJoystickSettings : joystickSettings);
			if (UsingKeyboardAsController || rCDeviceData == null)
			{
				return GetRawAxis(AssignedToRaw(assignedAxis), p_controller);
			}
			if (p_controller == null)
			{
				return 0f;
			}
			AssignedAxisData aAD = rCDeviceData.GetAAD(assignedAxis);
			float assignedAxisValueFromIndex = GetAssignedAxisValueFromIndex(p_controller, aAD, excludeZeroThrottle);
			if (useThrottleCap && assignedAxis == AssignedAxis.Throttle && throttleCap > 0f)
			{
				assignedAxisValueFromIndex = (assignedAxisValueFromIndex + 1f) / 2f * throttleCap;
				return assignedAxisValueFromIndex * 2f - 1f;
			}
			return assignedAxisValueFromIndex;
		}

		public static float GetRawAxis(RawAxis rawAxis, Controller p_controller = null, bool p_useThrottleCap = false)
		{
			if (!Initialized)
			{
				return 0f;
			}
			if (p_controller == null)
			{
				p_controller = currentJoystick;
			}
			if (UsingKeyboardAsController)
			{
				if (DRLUINavigationSystem.IsTyping)
				{
					return 0f;
				}
				switch (rawAxis)
				{
				case RawAxis.LeftStickX:
					return Input.GetKey(KeyCode.A) ? (-1) : (Input.GetKey(KeyCode.D) ? 1 : 0);
				case RawAxis.LeftStickY:
				{
					float num = (Input.GetKey(KeyCode.S) ? (-1) : (Input.GetKey(KeyCode.W) ? 1 : 0));
					float num2 = Mathf.Abs(throttleCap);
					if (!(throttleCap > 0f))
					{
						return num;
					}
					return Mathf.Clamp(num, 0f - num2, num2);
				}
				case RawAxis.RightStickX:
					return Input.GetKey(KeyCode.LeftArrow) ? (-1) : (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
				case RawAxis.RightStickY:
					return Input.GetKey(KeyCode.DownArrow) ? (-1) : (Input.GetKey(KeyCode.UpArrow) ? 1 : 0);
				default:
					return 0f;
				}
			}
			if (p_controller == null)
			{
				return 0f;
			}
			RCDeviceData rCDeviceData = (p_controller.isNavigation ? navigationJoystickSettings : joystickSettings);
			if (rCDeviceData == null || p_controller == null || (rawAxis == RawAxis.ToggleA && rCDeviceData.toggleAsButtonA < -1) || (rawAxis == RawAxis.ToggleB && rCDeviceData.toggleAsButtonB < -1) || (p_controller != navigationController && !IsCalibrated))
			{
				return 0f;
			}
			if ((rawAxis == RawAxis.ToggleA && rCDeviceData.toggleAsButtonA > -1) || (rawAxis == RawAxis.ToggleB && rCDeviceData.toggleAsButtonB > -1))
			{
				int num3 = ((rawAxis == RawAxis.ToggleA) ? rCDeviceData.toggleAsButtonA : rCDeviceData.toggleAsButtonB);
				if (num3 >= 0 && num3 < p_controller.buttonCount)
				{
					if (!p_controller.GetButtonChangedState(num3))
					{
						return 0f;
					}
					return 1f;
				}
				return 0f;
			}
			return GetAxisRaw(p_controller, rawAxis, throttleCap > 0f);
		}

		public static int GetAxisCount(Controller p_controller = null)
		{
			if (!Initialized)
			{
				return 0;
			}
			if (p_controller == null)
			{
				p_controller = currentJoystick;
			}
			return p_controller?.axisCount ?? 0;
		}

		public static float GetDeltaFromIndex(int index)
		{
			if (!Initialized)
			{
				return 0f;
			}
			if (currentJoystick == null || index < 0)
			{
				return 0f;
			}
			return currentJoystick.GetAxisDeltaRaw(index);
		}

		public static float GetRawFromIndex(int index, Controller p_controller = null)
		{
			if (!Initialized)
			{
				return 0f;
			}
			if (p_controller == null)
			{
				p_controller = currentJoystick;
			}
			if (p_controller == null || index < 0)
			{
				return 0f;
			}
			return p_controller.GetAxisRaw(index);
		}

		public static float GetAssignedAxisValueFromIndex(int index, float min, float max, float center, float deadzone, float centerPoint, bool inverted, Controller p_controller = null)
		{
			if (!Initialized)
			{
				return 0f;
			}
			if (p_controller == null)
			{
				p_controller = currentJoystick;
			}
			if (p_controller == null)
			{
				return 0f;
			}
			float num = AdjustValueForAAD(GetRawFromIndex(index, p_controller), min, max, center, deadzone);
			if (inverted)
			{
				num = 0f - num;
			}
			if (centerPoint > -2f)
			{
				float num2 = 1f - centerPoint;
				if (Mathf.Abs(num2) > 0.001f)
				{
					num = ((num < centerPoint) ? (-1f) : ((num - centerPoint) * (2f / num2) - 1f));
				}
			}
			return num;
		}

		private static float AdjustValueForAAD(float v, float min, float max, float center, float deadzone)
		{
			float num = Mathf.Clamp(v, min, max);
			float num2 = ((v > center) ? 1f : (-1f));
			num = (num - center) * (1f / (((num2 > 0f) ? max : min) - center));
			if (Mathf.Abs(num) <= deadzone)
			{
				return 0f;
			}
			num = (num - deadzone) * (1f / (1f - deadzone));
			num *= num2;
			if (!float.IsNaN(num))
			{
				return num;
			}
			return 0f;
		}

		public static float GetAssignedAxisValueFromIndex(Controller p_controller, AssignedAxisData p_aad, bool p_excludeZeroThrottle = false)
		{
			if (!Initialized || p_aad == null)
			{
				return 0f;
			}
			int elementID = p_aad.ElementID;
			float min = p_aad.min;
			float max = p_aad.max;
			float center = p_aad.center;
			bool inverted = p_aad.inverted;
			float num = p_aad.deadzone;
			float centerPoint = p_aad.zeroThrottle;
			if (p_excludeZeroThrottle)
			{
				centerPoint = -2f;
			}
			if (p_aad.assignedAxis == AssignedAxis.Yaw)
			{
				num = Mathf.Max(num, overrideYawDeadzone);
			}
			return GetAssignedAxisValueFromIndex(elementID, min, max, center, num, centerPoint, inverted, p_controller);
		}

		private static float GetAxisRaw(Controller p_controller, RawAxis axis, bool p_useThrottleCap)
		{
			if (!Initialized || (p_controller != navigationController && !IsCalibrated))
			{
				return 0f;
			}
			RCDeviceData rCDeviceData = ((p_controller != null && p_controller.isNavigation) ? navigationJoystickSettings : joystickSettings);
			if (rCDeviceData == null)
			{
				return 0f;
			}
			AssignedAxisData aAD = rCDeviceData.GetAAD(axis);
			if (aAD == null)
			{
				return 0f;
			}
			float assignedAxisValueFromIndex = GetAssignedAxisValueFromIndex(p_controller, aAD, p_excludeZeroThrottle: true);
			if (aAD.assignedAxis == AssignedAxis.Throttle && throttleCap > 0f && p_useThrottleCap)
			{
				assignedAxisValueFromIndex = (assignedAxisValueFromIndex + 1f) / 2f * throttleCap;
				return assignedAxisValueFromIndex * 2f - 1f;
			}
			return assignedAxisValueFromIndex;
		}

		private static RawAxis AssignedToRaw(AssignedAxis p_assignedAxis)
		{
			return p_assignedAxis switch
			{
				AssignedAxis.Throttle => RawAxis.LeftStickY, 
				AssignedAxis.Yaw => RawAxis.LeftStickX, 
				AssignedAxis.Pitch => RawAxis.RightStickY, 
				AssignedAxis.Roll => RawAxis.RightStickX, 
				_ => RawAxis.LeftStickY, 
			};
		}

		public static void SetThrottleCap(float p_value)
		{
			if (p_value < 0f)
			{
				throttleCap = -1f;
			}
			else
			{
				throttleCap = p_value / 100f;
			}
		}

		public static bool GetButton(ConsoleButtons cb)
		{
			if (Initialized && rewiredPlayer != null && NavigationIsGamepad())
			{
				return rewiredPlayer.GetButton((int)RevertButton(cb));
			}
			return false;
		}

		public static bool GetButtonDown(ConsoleButtons cb)
		{
			if (Initialized && rewiredPlayer != null && NavigationIsGamepad())
			{
				return rewiredPlayer.GetButtonDown((int)RevertButton(cb));
			}
			return false;
		}

		public static bool GetButtonUp(ConsoleButtons cb)
		{
			if (Initialized && rewiredPlayer != null && NavigationIsGamepad())
			{
				return rewiredPlayer.GetButtonUp((int)RevertButton(cb));
			}
			return false;
		}

		public static bool GetAnyButton()
		{
			if (Initialized && rewiredPlayer != null && NavigationIsGamepad())
			{
				return rewiredPlayer.GetAnyButton();
			}
			return false;
		}

		public static bool GetAnyButtonDown()
		{
			if (Initialized && rewiredPlayer != null && NavigationIsGamepad())
			{
				return rewiredPlayer.GetAnyButtonDown();
			}
			return false;
		}

		public static bool GetAnyButtonUp()
		{
			if (Initialized && rewiredPlayer != null && NavigationIsGamepad())
			{
				return rewiredPlayer.GetAnyButtonUp();
			}
			return false;
		}

		public static bool GetToggle(RawAxis p_axis)
		{
			if (!Initialized)
			{
				return false;
			}
			if (p_axis == RawAxis.ToggleA && toggledA)
			{
				toggledA = false;
				toggleCooldown = true;
				return true;
			}
			if (p_axis == RawAxis.ToggleB && toggledB)
			{
				toggledB = false;
				toggleCooldown = true;
				return true;
			}
			return false;
		}

		public static int GetButtonCount(Controller p_controller = null)
		{
			if (!Initialized)
			{
				return 0;
			}
			if (p_controller == null)
			{
				p_controller = currentJoystick;
			}
			return p_controller?.buttonCount ?? 0;
		}

		public static bool GetButtonRawIndex(int index, Controller p_controller = null)
		{
			if (!Initialized)
			{
				return false;
			}
			if (p_controller == null)
			{
				p_controller = currentJoystick;
			}
			return p_controller?.GetButtonById(index) ?? false;
		}

		public static bool GetButtonChanged(int index, Controller p_controller = null)
		{
			if (!Initialized)
			{
				return false;
			}
			if (p_controller == null)
			{
				p_controller = currentJoystick;
			}
			return p_controller?.GetButtonChangedState(index) ?? false;
		}

		public static Controller GetActiveJoystick()
		{
			if (!Initialized)
			{
				return null;
			}
			return currentJoystick;
		}

		public static bool UsingToggles()
		{
			if (Initialized && HasNavigationController && navigationJoystickSettings.toggleAsButtonA != -2)
			{
				return navigationJoystickSettings.toggleAsButtonA != -2;
			}
			return false;
		}

		public static bool IsToggleButton(RawAxis p_toggle)
		{
			if (!Initialized || (p_toggle != RawAxis.ToggleA && p_toggle != RawAxis.ToggleB))
			{
				return false;
			}
			if (!HasNavigationController)
			{
				return false;
			}
			if (p_toggle == RawAxis.ToggleA)
			{
				return navigationJoystickSettings.toggleAsButtonA > -1;
			}
			return navigationJoystickSettings.toggleAsButtonB > -1;
		}

		public static float GetToggleDown(RawAxis p_toggle)
		{
			if (!Initialized || (p_toggle != RawAxis.ToggleA && p_toggle != RawAxis.ToggleB))
			{
				return 0f;
			}
			if (!HasNavigationController)
			{
				return 0f;
			}
			if (p_toggle == RawAxis.ToggleA)
			{
				if (navigationJoystickSettings.toggleAsButtonA > -1)
				{
					return GetButtonRawIndex(navigationJoystickSettings.toggleAsButtonA, navigationController) ? 1 : 0;
				}
				return GetRawAxis(RawAxis.ToggleA, navigationController);
			}
			if (navigationJoystickSettings.toggleAsButtonB > -1)
			{
				return GetButtonRawIndex(navigationJoystickSettings.toggleAsButtonB, navigationController) ? 1 : 0;
			}
			return GetRawAxis(RawAxis.ToggleB, navigationController);
		}

		private static ConsoleButtons RevertButton(ConsoleButtons input)
		{
			return input;
		}

		public static bool isEnterButtonInverted()
		{
			return false;
		}

		public static void LockInput(bool l)
		{
			lockInput = l;
		}
	}
}
