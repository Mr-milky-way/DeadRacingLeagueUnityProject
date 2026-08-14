using System;
using Rewired;
using UnityEngine;
using drl.game;
using thelab.mvc;

namespace drl.sim.rci
{
	[RequireComponent(typeof(InputManager))]
	public class RCInputManagerHolder : Controller<DRLApp>
	{
		private bool initializedLocal;

		private Action onStart;

		private Action<bool> onAppFocusChanged;

		private Action onLateUpdate;

		private Action onUpdate;

		private InputManager inputManager;

		[SerializeField]
		private NetworkRewiredReceiver m_network;

		public NetworkRewiredReceiver network
		{
			get
			{
				if (!m_network)
				{
					return m_network = GetComponent<NetworkRewiredReceiver>();
				}
				return m_network;
			}
		}

		public void OnInstantiating(Action onStart, Action<bool> onAppFocusChanged, Action onLateUpdate, Action onUpdate)
		{
			if (!RCI.Initialized && !initializedLocal)
			{
				this.onStart = onStart;
				this.onAppFocusChanged = onAppFocusChanged;
				this.onLateUpdate = onLateUpdate;
				this.onUpdate = onUpdate;
				initializedLocal = true;
			}
		}

		private void LateUpdate()
		{
			if (RCI.Initialized)
			{
				onLateUpdate?.Invoke();
			}
		}

		private void Update()
		{
			if (RCI.Initialized)
			{
				onUpdate?.Invoke();
			}
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			Debug.Log($"RCInputManagerHolder> OnApplicationFocus / has-focus[{hasFocus}] rci-init[{RCI.Initialized}]");
			if (RCI.Initialized)
			{
				onAppFocusChanged?.Invoke(hasFocus);
			}
		}

		public void Initialize()
		{
			if (onStart != null)
			{
				onStart();
			}
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (!RCI.Initialized && !(p_event != "boot@complete"))
			{
				Debug.Log("RCInputManagerHolder> OnNotification / BootComplete - Initializing RCI...");
				onStart();
			}
		}

		private void OnEnable()
		{
			inputManager = GetComponent<InputManager>();
		}

		private void OnDisable()
		{
			RCI.Unsubscribe();
		}

		public bool IsSupported(string p_name)
		{
			if (p_name.ToLower().Contains("xbox"))
			{
				Debug.Log("RCInputManagerHolder> Found xbox controller - is supported true!");
				return true;
			}
			if (p_name.ToUpper() == "UNKNOWN CONTROLLER")
			{
				Debug.LogWarning("RCInputManagerHolder> IsSupported / [" + p_name + "] is unknown controller");
				return false;
			}
			if (!inputManager)
			{
				Debug.LogWarning("RCInputManagerHolder> IsSupported / [" + p_name + "] input manager is <null>");
				return false;
			}
			if (inputManager.dataFiles == null)
			{
				Debug.LogWarning("RCInputManagerHolder> IsSupported / [" + p_name + "] data files are missing");
				return false;
			}
			string[] joystickNames = inputManager.dataFiles.GetJoystickNames();
			for (int i = 0; i < joystickNames.Length; i++)
			{
				if (string.Equals(joystickNames[i], p_name))
				{
					return true;
				}
			}
			Debug.LogWarning("RCInputManagerHolder> IsSupported / [" + p_name + "] joystick name not found!");
			return false;
		}
	}
}
