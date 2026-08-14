using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class PLMController : Controller<DRLApp>
	{
		public NetworkConnectivityController network;

		private bool m_is_quit_active;

		private bool m_is_reset_active;

		private string m_activation_args;

		private Activity m_invite_debug_loop;

		public string debugInviteRoomId;

		public string debugInviteOwnerId;

		public bool isSceneLoading;

		public bool forceResetScheduled;

		private DialogComponent m_dialog;

		public DialogComponent dialog
		{
			get
			{
				if ((bool)m_dialog)
				{
					return m_dialog;
				}
				if (!base.app)
				{
					return null;
				}
				if (!base.app.view)
				{
					return null;
				}
				if (!base.app.view.ui)
				{
					return null;
				}
				return m_dialog = base.app.view.ui.dialog;
			}
		}

		[ContextMenu("Force Reset")]
		public void ForceReset()
		{
			if (m_is_reset_active)
			{
				UnityEngine.Debug.LogWarning("PLMController> ForceReset / Already Resetting...");
				return;
			}
			m_is_reset_active = true;
			base.app.view.ui.fade.FadeIn(2f);
			Notify("scene.force-reset");
			Activity.RunOnce(delegate
			{
				if ((bool)base.app && (bool)base.app.model && (bool)base.app.model.storage)
				{
					List<DRLAssetBundleLibrary> allAssetBundleLibrary = base.app.model.storage.GetAllAssetBundleLibrary();
					for (int i = 0; i < allAssetBundleLibrary.Count; i++)
					{
						allAssetBundleLibrary[i].UnloadLibrary();
					}
				}
				if ((bool)base.app && (bool)base.app.level)
				{
					base.app.level.UnloadBundles();
				}
				bool wait_network_disconnect = false;
				float wait_network_disconnect_timeout = 5f;
				if (PhotonNetwork.connected)
				{
					PhotonNetwork.Disconnect();
					wait_network_disconnect = true;
				}
				if (wait_network_disconnect)
				{
					UnityEngine.Debug.Log("PLMController> ForceReset / Needs Photon Disconnect...");
				}
				Activity.Run((Func<bool>)delegate
				{
					if (wait_network_disconnect)
					{
						wait_network_disconnect_timeout -= 1f;
						if (wait_network_disconnect_timeout > 0f)
						{
							return true;
						}
					}
					ApplyForceReset();
					return false;
				}, 0f, false);
			}, 3f);
		}

		protected void ApplyForceReset()
		{
			if ((bool)base.app.boot && (bool)base.app.boot.slack)
			{
				base.app.boot.slack.skipLogDelete = true;
			}
			GameObject[] dontDestroyRootObjects = LevelManager.GetDontDestroyRootObjects();
			for (int i = 0; i < dontDestroyRootObjects.Length; i++)
			{
				if (!(dontDestroyRootObjects[i].name == "PhotonMono") && !(dontDestroyRootObjects[i].name == "SteamManager") && !(dontDestroyRootObjects[i].name == "RCInputManager"))
				{
					UnityEngine.Debug.Log("DRLBootController> ApplyForceReset / TryDestroy [" + dontDestroyRootObjects[i].name + "]");
					dontDestroyRootObjects[i].GetComponents<Component>();
					UnityEngine.Object.Destroy(dontDestroyRootObjects[i]);
				}
			}
			SetPersistent.Clear();
			SceneManager.LoadScene("boot-bypass");
		}

		protected void Awake()
		{
		}

		protected void StartXboxUserChangePoll(float p_delay)
		{
		}

		protected void StartXboxUserChangePoll()
		{
			StartXboxUserChangePoll(0f);
		}

		protected void ClearXboxEvents()
		{
		}

		public void Init()
		{
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "splash.quit@click":
			case "home.quit@click":
				Application.wantsToQuit += OnApplicationQuitCallback;
				Application.Quit();
				break;
			case "scene.start":
			{
				string text = (string)p_data[0];
				if (text != null)
				{
					_ = text == "main";
				}
				break;
			}
			case "scene.game.scenes@start":
				isSceneLoading = true;
				break;
			case "scene.game.scenes@complete":
				isSceneLoading = false;
				if (forceResetScheduled)
				{
					UnityEngine.Debug.Log("PLMController> GameScenesComplete / Force Reset is scheduled, resetting...");
					forceResetScheduled = false;
					ClearXboxEvents();
					if ((bool)base.app && (bool)base.app.controller.plm)
					{
						base.app.controller.plm.ForceReset();
					}
				}
				break;
			}
		}

		private void ForceQuit()
		{
			if (m_is_quit_active)
			{
				return;
			}
			m_is_quit_active = true;
			if ((bool)base.app && (bool)base.app.view && (bool)base.app.view.ui)
			{
				base.app.view.ui.fade.FadeIn(0.5f);
			}
			UnityEngine.Debug.Log("PLMController> ForceQuit");
			RunOnce(0.5f, delegate
			{
				string appName = DRLPaths.appName;
				Process[] processesByName = Process.GetProcessesByName(appName);
				foreach (Process process in processesByName)
				{
					if (process != null)
					{
						UnityEngine.Debug.Log("PLMController> ForceQuit / Killing " + appName + " process...");
						process.Kill();
					}
				}
			});
		}

		public void OnPersistency()
		{
			base.app.controller.plm = this;
		}

		public bool OnApplicationQuitCallback()
		{
			Application.wantsToQuit -= OnApplicationQuitCallback;
			switch (Application.platform)
			{
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
				return false;
			case RuntimePlatform.WindowsPlayer:
				Application.CancelQuit();
				ForceQuit();
				break;
			}
			return true;
		}
	}
}
