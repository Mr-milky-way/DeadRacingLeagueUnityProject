using System.Collections;
using UnityEngine;
using drl.backend;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class SplashController : Controller<DRLApp>
	{
		public bool videoComplete;

		public UISplash view;

		public UITryoutsLoginView tryoutsLogin;

		public string loginTryoutsSteamId;

		private Activity m_login_tryouts_timer;

		private Coroutine _retryInitializePlatformWithDelay;

		protected void Awake()
		{
			Cursor.visible = false;
			view.content.alpha = 0f;
			base.app.view.audio.volume = 0.25f;
			float p_delay = 0f;
			Debug.Log("SplashController> Awake / PlayVideo");
			if (base.app.boot.forceIntroSkip)
			{
				Activity.RunOnce(delegate
				{
					ApplyIntroComplete();
				}, 1f);
				base.app.boot.forceIntroSkip = false;
			}
			else if (SystemInfo.operatingSystem.ToLower().Replace(" ", "").Contains("windows7"))
			{
				Debug.LogWarning("SplashController> Windows7 doesnt support video playback. Skipping...");
				Activity.RunOnce(ApplyIntroComplete, 3f);
			}
			else
			{
				Activity.RunOnce(view.video.Play, p_delay);
				Timer.Set(view.video, "allowSkip", 1f, true);
			}
		}

		protected override void Start()
		{
			base.Start();
			base.app.view.audio.SetSplashSceneAudio();
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "scene.load":
				Debug.Log("SplashController> SceneLoad");
				base.app.view.ui.splash.version = DRLApp.GetVersionString();
				Activity.RunOnce(delegate
				{
					_ = Debug.unityLogger.logEnabled;
				}, 2f);
				base.app.view.ui.splash.screen.alpha = 1f;
				base.app.view.audio.PlayMusicIntro();
				break;
			case "splash.intro@complete":
				Debug.Log("SplashController> Intro Complete");
				base.app.view.audio.volumeMusic = 0f;
				base.app.view.audio.MuteFadeIn(1f, 1f);
				view.content.alpha = 1f;
				base.app.view.ui.splash.videoFade.FadeOut(0.5f);
				base.app.view.ui.loader.background = null;
				base.app.view.ui.loader.fade.alpha = 1f;
				Activity.RunOnce(delegate
				{
					base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.initializing", "INITIALIZING...");
					base.app.view.ui.splash.spinnerFade.gameObject.SetActive(value: true);
					base.app.view.ui.splash.spinnerFade.FadeIn(0.5f);
					if (0 == 0)
					{
						Activity.RunOnce(base.app.view.audio.PlayUILoadingLoop, 0.5f);
					}
					if (DRLApp.forceOffline)
					{
						RunOnce(base.app.boot.Run, 0.2f);
					}
					else
					{
						base.app.controller.plm.network.CheckInternetConnectivity(delegate(bool isConnected)
						{
							DRLApp.offline = !isConnected;
							Debug.Log($"SplashController> CheckInternetConnectivity - connection[{isConnected}] - is offline[{DRLApp.offline}] - force offline[{DRLApp.forceOffline}]");
							if (DRLApp.offline)
							{
								Debug.Log("SplashController> No internet connection on boot.");
								if (DRLApp.forceOffline)
								{
									RunOnce(base.app.boot.Run, 0.2f);
								}
								else
								{
									Notify("boot.drl.offline-layout");
								}
							}
							else
							{
								RunOnce(base.app.boot.Run, 0.2f);
							}
						}, 2, 0.1f);
					}
				}, 0.7f);
				break;
			case "boot.timeout@start":
				base.app.view.ui.splash.spinnerTimeoutProgress = 0f;
				break;
			case "boot.timeout@update":
			{
				float spinnerTimeoutProgress = (float)p_data[0];
				base.app.view.ui.splash.spinnerTimeoutProgress = spinnerTimeoutProgress;
				break;
			}
			case "boot.drl.login@start":
				base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.connecting-server", "CONNECTING TO SERVER...");
				break;
			case "boot.drl.license.check":
				base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.license-check", "CHECKING LICENSE...");
				break;
			case "boot.drl.state@start":
				base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.state-load", "LOADING GAME STATE...");
				break;
			case "boot.drl.content.manifest":
				base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.content.manifest.load", "LOADING CONTENT MANIFEST...");
				break;
			case "boot.drl.content.download@start":
				base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.content.manifest.load", "DOWNLOADING CONTENT...");
				break;
			case "boot.drl.bundle.load@start":
				base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.bundle.load.start", "LOADING GAME DATA...");
				break;
			case "boot.drl.offline-maps.download@start":
				base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.state-download-maps", "DOWNLOADING MAPS FOR OFFLINE USE...");
				break;
			case "boot.drl.offline-maps.store@start":
				base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.state-store-maps", "STORING MAPS...");
				break;
			case "boot.timeout":
				if (!base.app.model.storage.HasOfflineData())
				{
					base.app.view.audio.StopUILoadingLoop();
					Activity.RunOnce(base.app.view.audio.PlayUILoadingError, 0.5f);
					base.app.view.ui.splash.StopClip();
					base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.connection-timeout", "CONNECTION TIMEOUT!");
					view.SetOfflineLayout(p_flag: true);
				}
				break;
			case "splash.connection-check@click":
				Application.OpenURL(DRLService.baseStatusPageUri);
				break;
			case "splash.connection-retry@click":
				base.app.view.audio.PlayUILoadingLoop();
				base.app.view.ui.splash.PlayClip();
				base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.connecting-server", "CONNECTING TO SERVER...");
				this.TimerRunOnce(delegate
				{
					base.app.boot.Run();
				}, 0.2f);
				view.SetErrorLayout(p_flag: false);
				break;
			case "boot.drl.login@retry":
			{
				int num = (int)p_data[0];
				string text2 = base.app.model.storage.locale.Get("splash.connection-retry", "CONNECTION FAILED... RETRY");
				base.app.view.ui.splash.spinnerText = text2 + " (" + num + ")";
				break;
			}
			case "boot.drl.state@fail":
			case "boot.drl.login@fail":
				if (!base.app.model.storage.HasOfflineData())
				{
					base.app.view.audio.StopUILoadingLoop();
					Activity.RunOnce(base.app.view.audio.PlayUILoadingError, 0.5f);
					base.app.view.ui.splash.StopClip();
					base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.connection-fail", "CONNECTION FAILED!");
					view.SetOfflineLayout(p_flag: true);
					if (p_event != null && p_event == "boot.drl.state@fail")
					{
						view.connectionRetryButton.gameObject.SetActive(value: false);
					}
				}
				break;
			case "boot.drl.offline-maps.download@error":
				view.SetErrorLayout(p_flag: true);
				base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.state-download-maps-fail", "FAILED TO DOWNLOAD MAPS!");
				break;
			case "boot.drl.offline-layout":
				RCI.manager.Initialize();
				view.SetOfflineLayout(p_flag: true);
				break;
			case "splash.offline-mode@click":
				view.SetOfflineLayout(p_flag: false);
				break;
			case "boot.drl.platform@fail":
			{
				RCI.manager.Initialize();
				base.app.view.audio.StopUILoadingLoop();
				Activity.RunOnce(base.app.view.audio.PlayUILoadingError, 0.5f);
				base.app.view.ui.splash.StopClip();
				string text = "PLATFORM ERROR!";
				text = base.app.model.storage.locale.Get("splash.connection-fail", "CONNECTION FAILED!");
				base.app.view.ui.splash.spinnerText = text;
				view.SetOfflineLayout(p_flag: true);
				view.connectionCheckButton.gameObject.SetActive(value: true);
				view.connectionRetryButton.gameObject.SetActive(value: true);
				UINavigation.Focus(view.connectionCheckButton);
				break;
			}
			case "boot.missing.dll":
				base.app.view.audio.StopUILoadingLoop();
				Activity.RunOnce(base.app.view.audio.PlayUILoadingError, 0.5f);
				base.app.view.ui.splash.StopClip();
				base.app.view.ui.splash.spinnerText = base.app.model.storage.locale.Get("splash.missing-dll", "INSTALL ERROR: MISSING VC REDIST 2015.");
				view.SetOfflineLayout(p_flag: true);
				view.connectionCheckButton.gameObject.SetActive(value: false);
				Debug.LogError("SplashController> Missing DLL detected, aborting boot");
				break;
			case "boot@complete":
				base.app.view.ui.dialog?.Close();
				base.app.view.audio.StopUILoadingLoop();
				base.app.view.ui.splash.spinnerFade.FadeOut(0.5f);
				Activity.RunOnce(base.app.view.audio.PlayUILoadingSuccess, 0.5f);
				base.app.view.ui.fade.FadeIn(0.5f, 0.5f);
				Activity.RunOnce(delegate
				{
					base.app.scene.LoadMain();
				}, 1.5f);
				break;
			}
		}

		private IEnumerator RetryInitializePlatformWithDelay()
		{
			base.app.view.ui.dialog.Open(DialogTemplateType.ConnectionDisconnectNoneButtons, "connection-disconnected");
			yield return new WaitForSeconds(2f);
			Notify("splash.connection-retry@click");
		}

		public void OnVideoEvent(VideoEventType p_event)
		{
			if (!videoComplete)
			{
				switch (p_event)
				{
				case VideoEventType.Start:
					Debug.Log("SplashController> OnVideoEvent / Start");
					break;
				case VideoEventType.Complete:
				case VideoEventType.Skip:
					Debug.Log("SplashController> OnVideoEvent / event[" + p_event.ToString() + "]");
					ApplyIntroComplete();
					break;
				}
			}
		}

		protected void ApplyIntroComplete()
		{
			videoComplete = true;
			base.app.view.audio.StopMusicIntro();
			Debug.Log("SplashController> ApplyIntroComplete");
			Notify("splash.intro@complete");
		}

		protected void Update()
		{
			bool buttonDown = RCI.GetButtonDown(ConsoleButtons.ActionBottomRow1);
			bool buttonDown2 = RCI.GetButtonDown(ConsoleButtons.ActionTopRow2);
			if (buttonDown && view.connectionCheckButton.gameObject.activeInHierarchy)
			{
				Notify("splash.connection-check@click");
			}
			if (buttonDown2 && view.connectionRetryButton.gameObject.activeInHierarchy)
			{
				Notify("splash.connection-retry@click");
			}
		}
	}
}
