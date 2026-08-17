using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using drl.backend;
using drl.sim.rci;
using drl.sim.thread;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLBootController : Controller<DRLApp>
	{
		public static bool ready = false;

		public static bool writeUserPlatformData = true;

		private static bool m_reset;

		private static bool m_force_intro_skip;

		private SlackDebugTool m_slack;

		public bool run;

		public int retries = 7;

		public bool forceTimeout;

		public bool forcePlatformFail;

		public bool ignoreBundles;

		public float hardwareScore = 0.5f;

		public float connectionTimeoutMax = 30f;

		public UILoaderView uiBootLoader;

		private static int m_verifyCount = 0;

		public string beginnerReplayId = "https://drl-game-api.s3.amazonaws.com/onboarding-bots-replays-v2/onboarding-bot-beginner.race";

		public string intermediateReplayId = "https://drl-game-api.s3.amazonaws.com/onboarding-bots-replays-v2/onboarding-bot-intermediate.race";

		public string proReplayId0 = "https://drl-game-api.s3.amazonaws.com/onboarding-bots-replays-v2/onboarding-bot-pro-1.race";

		public string proReplayId1 = "https://drl-game-api.s3.amazonaws.com/onboarding-bots-replays-v2/onboarding-bot-pro-2.race";

		public string proReplayId2 = "https://drl-game-api.s3.amazonaws.com/onboarding-bots-replays-v2/onboarding-bot-pro-3.race";

		private Activity m_timeout_loop;

		private bool m_run_lock;

		private static bool m_locale_loaded;

		private int m_mapsPass;

		private bool m_hasMapUpdates;

		private List<MapData> m_maps = new List<MapData>();

		private WebAsyncRequest custom_maps_request;

		private WebAsyncRequest maps_request;

		public bool reset
		{
			get
			{
				return m_reset;
			}
			set
			{
				m_reset = value;
			}
		}

		public bool forceIntroSkip
		{
			get
			{
				return m_force_intro_skip;
			}
			set
			{
				m_force_intro_skip = value;
			}
		}

		public SlackDebugTool slack
		{
			get
			{
				if (!m_slack)
				{
					return m_slack = UnityEngine.Object.FindObjectOfType<SlackDebugTool>();
				}
				return m_slack;
			}
		}

		public bool complete => ready;

		protected void ConnectionTimeoutStart()
		{
			if (m_timeout_loop != null)
			{
				m_timeout_loop.Stop();
			}
			m_timeout_loop = null;
			float t = -40f;
			if (forceTimeout)
			{
				t = 10f;
			}
			Notify("boot.timeout@start");
			m_timeout_loop = Activity.Run((Func<bool>)delegate
			{
				t += Time.deltaTime;
				if (t < 0f)
				{
					return true;
				}
				if (t >= connectionTimeoutMax)
				{
					Notify("boot.timeout");
					return false;
				}
				float num = Mathf.Clamp01(t / connectionTimeoutMax);
				Notify("boot.timeout@update", num);
				return true;
			}, 0f, false);
		}

		protected void ConnectionTimeoutStop()
		{
			if (m_timeout_loop != null)
			{
				m_timeout_loop.Stop();
			}
			m_timeout_loop = null;
			Notify("boot.timeout@stop");
		}

		protected void Awake()
		{
			Scene activeScene = SceneManager.GetActiveScene();
			Cursor.visible = false;
			if (Input.mousePresent && (bool)base.app)
			{
				Cursor.SetCursor(base.app.cursor, Vector2.zero, CursorMode.Auto);
			}
			if ((bool)base.app && (bool)base.app.controller)
			{
				base.app.controller.SetMouseVisible(p_flag: false);
			}
			switch (activeScene.name)
			{
			case "boot-bypass":
				reset = true;
				SceneManager.LoadSceneAsync("boot");
				break;
			case "boot":
				if (reset)
				{
					Debug.Log("DRLBootController> Awake / context[" + OS.context + "] Force Reset.");
					ready = false;
					reset = false;
					forceIntroSkip = true;
					DRLUINavigationSystem.IsLoading = false;
					RCI.Initialized = false;
					RCI.Initialize();
					SceneManager.LoadSceneAsync("boot");
				}
				else
				{
					bool flag = PlayerPrefs.HasKey("graphics-tier");
					int num = (flag ? PlayerPrefs.GetInt("graphics-tier") : (-1));
					Debug.Log($"DRLBootController> Awake / Checking GraphicsTier Prefs - exist[{flag}] value[{num}]");
					if (num < 0)
					{
						num = 0;
					}
					Graphics.activeTier = (GraphicsTier)num;
					SceneManager.LoadSceneAsync("splash");
				}
				break;
			}
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "scene.start":
				hardwareScore = GraphicsStateModel.GetHardwareScore();
				if (!run)
				{
					break;
				}
				if (DRLApp.forceOffline)
				{
					RunOnce(Run, 0.2f);
				}
				else
				{
					Debug.Log("DRLBootController> Scene.Start / CheckInternetConnectivity");
					base.app.controller.plm.network.CheckInternetConnectivity(delegate(bool isConnected)
					{
						Debug.Log($"DRLBootController> Scene.Start / CheckInternetConnectivity Completed - connected[{isConnected}]");
						DRLApp.offline = !isConnected;
						if (DRLApp.offline)
						{
							Notify("boot.drl.offline-layout");
							Debug.Log("DRLBootController> No internet connection on boot.");
						}
						RunOnce(Run, 0.2f);
					}, 2, 0.1f);
				}
				if (!base.app.inGame && !HasCache())
				{
					DownloadOnboardingCache();
				}
				break;
			case "service.login@error":
				retries--;
				Notify((retries > 0) ? "boot.drl.login@retry" : "boot.drl.login@fail", retries);
				if (retries > 0)
				{
					this.TimerRunOnce(DRLAuth, 5f);
				}
				break;
			case "service.state.game@error":
				retries--;
				if (retries <= 0)
				{
					Notify("boot.drl.state@fail");
				}
				else
				{
					Activity.RunOnce(LoadState, 10f);
				}
				break;
			case "settings.language.apply":
				LoadPreferedLanguage(p_force: true);
				break;
			case "boot@complete":
				Debug.Log("DRLBootController> Boot Sequence Complete");
				LoadPreferedLanguage(p_force: false);
				break;
			case "splash.offline-mode@click":
				DRLApp.offline = true;
				DRLApp.forceOffline = true;
				RunOnce(Run, 0.2f);
				Debug.Log("DRLBootController> Starting game in offline mode.");
				break;
			}
		}

		protected void CheckBundleQuality()
		{
			DRLApp.GetSystemInfo();
			ProfileStateModel profile = base.app.model.storage.state.player.profile;
			int cpuRAM = DRLApp.GetCpuRAM();
			int gpuRAM = DRLApp.GetGpuRAM();
			int num = cpuRAM + gpuRAM;
			bool flag = DRLApp.IsLowRAMSpec();
			if (Application.platform == RuntimePlatform.XboxOne)
			{
				num = Mathf.Max(cpuRAM, gpuRAM);
				Debug.Log($"DRLBootController> XBox  model[{OS.context}] graphics[{SystemInfo.graphicsDeviceType}]");
				flag = OS.context == "xbs" || OS.context == "xb";
			}
			if (profile.isDeveloper && Input.GetKey(KeyCode.B))
			{
				flag = true;
			}
			DRLPaths.Content.useLibraryLow = flag;
			Debug.Log($"DRLBootController> CheckBundleQuality / ram[{cpuRAM}] vram[{gpuRAM}] total[{num}] is-low[{flag}]");
		}

		public void Run()
		{
			if (!m_run_lock)
			{
				m_run_lock = true;
				Debug.Log("DRLBootController> Run / version[" + DRLApp.GetVersionString() + "]\n" + StackTraceUtility.ExtractStackTrace());
				CheckBundleQuality();
				LocalizationInit();
			}
		}

		protected void LocalizationInit()
		{
			Debug.Log("DRLBootController> LocalizationInit");
			if (base.validContext)
			{
				Localization locale = base.app.model.storage.locale;
				if (locale.sources.Count > 0)
				{
					OnLocalizationComplete();
					return;
				}
				locale.OnLoad.RemoveAllListeners();
				locale.OnLoad.AddListener(OnLocalizationComplete);
				locale.sources.Add(DRLPaths.Content.localeRoot);
				locale.sources.Add(DRLPaths.Storage.localeRoot);
				LoadPreferedLanguage(p_force: true);
			}
		}

		public void LoadPreferedLanguage(bool p_force)
		{
			if (p_force || !m_locale_loaded)
			{
				m_locale_loaded = true;
				PreferedLanguage preferedLanguage = base.app.model.storage.state.player.preferedLanguage;
				string text = "en-us";
				switch (preferedLanguage)
				{
				case PreferedLanguage.Auto:
					text = base.app.model.storage.systemLocale;
					break;
				case PreferedLanguage.English:
					text = "en-us";
					break;
				case PreferedLanguage.SimplifiedChinese:
					text = "zh";
					break;
				}
				Debug.Log($"DRLBootController> LoadPreferedLanguage /  lang[{text}] system-lang[{Application.systemLanguage}]");
				Localization.instance.Load(text);
				Localization.instance.Refresh();
			}
		}

		protected void OnLocalizationComplete()
		{
			Localization locale = base.app.model.storage.locale;
			Debug.Log("DRLBootController> OnLocalizationComplete / Loaded " + locale.language);
			locale.OnLoad.RemoveAllListeners();
			locale.OnLoad.AddListener(OnLocalizationRefresh);
			Notify(0.5f, "boot.drl.localization@complete");
			AuthInit();
		}

		protected void OnLocalizationRefresh()
		{
			Notify(1f / 60f, "storage.localization@refresh");
		}

		public void RefreshChecksum(Action p_callback)
		{
			OS.RefreshChecksum(p_async: true, delegate
			{
				p_callback?.Invoke();
			});
		}

		protected void ValidateLicense(Action<bool> p_callback)
		{
			Debug.Log("DRLBootController> ValidateLicense / Checking License...");
			if (!base.validContext)
			{
				return;
			}
			if (DRLApp.offline)
			{
				LicenseStateModel.m_exists = true;
				Debug.Log("DRLBootController> License [" + true + "]");
				base.app.model.storage.SetLibraryByLicense(p_full: true);
				if (p_callback != null)
				{
					p_callback(obj: true);
				}
				return;
			}
			m_verifyCount %= 10;
			if (m_verifyCount != 0)
			{
				base.app.model.storage.SetLibraryByLicense(p_full: true);
				if (p_callback != null)
				{
					p_callback(obj: true);
				}
				m_verifyCount++;
				return;
			}
			Notify(1f / 60f, "boot.drl.license.check");
			ConnectionTimeoutStart();
			base.app.model.service.License(delegate(DRLLicenseResult p_result)
			{
				ConnectionTimeoutStop();
				m_verifyCount++;
				if (p_result == null)
				{
					Debug.LogWarning("DRLBootController> License Service Failed!");
					ValidateLicense(p_callback);
				}
				else
				{
					bool flag = (LicenseStateModel.m_exists = p_result.exists);
					Debug.Log("DRLBootController> License [" + flag + "]");
					if (base.validContext)
					{
						base.app.model.storage.SetLibraryByLicense(flag);
						if (p_callback != null)
						{
							p_callback(flag);
						}
					}
				}
			});
		}

		protected void AuthInit()
		{
			if (true)
			{
				try
				{
					_ = FlightController.Version;
					Debug.Log("DRLBootController> BetaFlight DLL Initialized!");
				}
				catch (DllNotFoundException)
				{
					Debug.LogError("DRLBootController> Missing DLL detected, aborting boot");
					Notify(0.5f, "boot.missing.dll");
					return;
				}
			}
			if (DRLApp.offline)
			{
				ValidateLicense(delegate
				{
					PlatformAuth();
					LoadState();
					if (ready)
					{
						OnComplete();
					}
				});
			}
			else if (ready)
			{
				ValidateLicense(delegate
				{
					OnComplete();
				});
			}
			else
			{
				PlatformAuth();
			}
		}

		public void RetryAuth()
		{
			Debug.Log("DRLBootController> RetryAuth");
			retries = 7;
			ConnectionTimeoutStop();
			PlatformAuth();
		}

		protected void PlatformAuth()
		{
			Debug.Log("DRLBootController> PlatformAuth");
			PlatformService platform = base.app.model.service.platform;
			if (!platform.ready)
			{
				platform.offline = DRLApp.offline;
				platform.Initialize();
				PlatformPolling();
			}
		}

		[ContextMenu("Start Platform Polling")]
		protected void PlatformPolling()
		{
			Debug.Log("DRLBootController> PlatformPolling Start");
			float ready_check_elapsed = 0.5f;
			float t = 0f;
			float platform_timeout = 15f;
			PlatformService ps = base.app.model.service.platform;
			Activity.Run((Func<bool>)delegate
			{
				t += (ps.active ? 0f : Time.unscaledDeltaTime);
				if (t >= platform_timeout)
				{
					Notify("boot.drl.platform@fail");
					return false;
				}
				ready_check_elapsed += Time.unscaledDeltaTime;
				if (ready_check_elapsed < 0.5f)
				{
					return true;
				}
				ready_check_elapsed = 0f;
				if (!ps.ready)
				{
					Debug.Log($"DRLBootController> PlatformAuth / Services not ready - [{ps.GetType().Name}.{ps.name}.{ps.GetInstanceID()}]");
					return true;
				}
				if (forcePlatformFail)
				{
					Notify(4f, "boot.drl.platform@fail");
					return false;
				}
				Notify("boot.platform.login");
				if (!DRLApp.offline)
				{
					DRLAuth();
				}
				else
				{
					RunOnce(Run, 0.2f);
				}
				return false;
			}, 0f, false).name = "boot-platform-loop";
		}

		protected void DRLAuth()
		{
			Debug.Log("DRLBootController> DRLAuth / retries[" + retries + "]");
			if (false)
			{
				Notify("boot.drl.tryouts.login");
			}
			else
			{
				DRLLogin();
			}
		}

		protected void DRLLogin()
		{
			ConnectionTimeoutStart();
			Notify("boot.drl.login@start");
			RefreshChecksum(delegate
			{
				base.app.model.service.Login(delegate
				{
					if (!forceTimeout)
					{
						ConnectionTimeoutStop();
						OnAuthSuccess();
					}
				});
			});
		}

		protected void OnAuthSuccess()
		{
			Notify("boot.drl.login@success");
			ConnectionTimeoutStart();
			ValidateLicense(delegate
			{
				ConnectionTimeoutStop();
				LoadState();
			});
		}

		protected void LoadState()
		{
			Debug.Log("DRLBootController> LoadState [" + retries + "]");
			if (!base.validContext)
			{
				return;
			}
			Notify("boot.drl.state@start");
			int state_pass = 0;
			int state_steps = (DRLApp.offline ? 1 : 5);
			m_mapsPass = 0;
			int maps_steps = 2;
			_ = DRLPaths.Storage.offlineMapsHashFilename;
			ServiceModel service = base.app.model.service;
			StorageModel stm = base.app.model.storage;
			CircuitStateModel cs = base.app.model.storage.state.player.circuits;
			ProgressionStateModel progression = base.app.model.storage.state.player.progression;
			GamePlayerData.service = service.backend;
			Action OnValidateStates = delegate
			{
				if (state_pass >= state_steps && m_mapsPass >= maps_steps)
				{
					base.app.model.storage.state.ready = true;
					stm.maps.SyncLocalMapVersions(delegate
					{
						OnMapsDownloadComplete();
					});
					UpdateState();
				}
			};
			LoadPlayerState(delegate
			{
				state_pass++;
				OnValidateStates();
				RCI.manager.Initialize();
				cs.RefreshCircuitData(delegate
				{
					Debug.Log("DRLBootController> LoadState / Circuits State Load");
					state_pass++;
					OnValidateStates();
				});
				UpdateLocalMaps(delegate
				{
					if (maps_request != null)
					{
						maps_request.loader.Dispose();
						maps_request = null;
					}
					if (custom_maps_request != null)
					{
						custom_maps_request.loader.Dispose();
						custom_maps_request = null;
					}
					OnValidateStates();
				}, maps_steps);
				LoadPreferedLanguage(p_force: true);
			});
			if (DRLApp.offline)
			{
				return;
			}
			service.StateGame(delegate
			{
				if (!base.validContext)
				{
					Debug.Log("DRLBootController> service.StateGame / Invalid Context");
				}
				else if (!base.app.model.storage.state.ready)
				{
					Debug.Log("DRLBootController> LoadState / Game State Load");
					state_pass++;
					OnValidateStates();
				}
			});
			progression.Refresh(delegate
			{
				Debug.Log("DRLBootController> LoadState / Progression State Load");
				state_pass++;
				OnValidateStates();
			});
			progression.LoadTracks(delegate
			{
				Debug.Log("DRLBootController> LoadState / Progression Tracks Load");
				state_pass++;
				OnValidateStates();
			});
			service.SyncOfflineMapEditorMaps();
		}

		private void UpdateLocalMaps(Action p_callback, int p_maps_steps)
		{
			StorageModel stm = base.app.model.storage;
			MapsStorageModel ms = base.app.model.storage.maps;
			ServiceModel sm = base.app.model.service;
			string offlineMapsHashFilename = DRLPaths.Storage.offlineMapsHashFilename;
			if (base.app.model.storage.state.player.profile.clearMapsCache && !DRLApp.offline)
			{
				ms.ClearCache();
			}
			bool has_cache = stm.HasOfflineData();
			bool has_community_cache = stm.HasCommunityCache();
			if (DRLApp.offline && !has_cache)
			{
				m_mapsPass = p_maps_steps;
				p_callback?.Invoke();
				return;
			}
			Debug.Log("DRLBootController> LoadState / Loading Maps from Cache");
			base.app.model.storage.maps.LoadCache(offlineMapsHashFilename, delegate(bool success)
			{
				if (success)
				{
					Debug.Log("DRLBootController> LoadState / All Maps in Cache count[" + stm.maps.maps.Count + "]");
				}
				else
				{
					has_cache = false;
					if (File.Exists(DRLPaths.Storage.offlineMapsHash))
					{
						File.Delete(DRLPaths.Storage.offlineMapsHash);
					}
				}
				maps_request = sm.UpdateLocalMaps(delegate(MapData[] p_result)
				{
					base.app.model.storage.state.player.profile.clearMapsCache = false;
					if (p_result == null || p_result.Length == 0 || DRLApp.offline)
					{
						Debug.Log("DRLBootController> LoadState / No new map updates.");
						m_hasMapUpdates = false;
						Notify("boot.drl.offline-maps.download@progress", 1f);
						m_mapsPass++;
						p_callback?.Invoke();
					}
					else
					{
						Debug.Log("DRLBootController> LoadState / New map updates - updated[" + p_result.Length + "] maps.");
						m_hasMapUpdates = true;
						m_maps.Clear();
						for (int i = 0; i < p_result.Length; i++)
						{
							m_maps.Add(p_result[i]);
						}
						for (int j = 0; j < ms.maps.Count; j++)
						{
							bool flag = false;
							for (int k = 0; k < m_maps.Count; k++)
							{
								if (ms.maps[j].guid == m_maps[k].guid)
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								m_maps.Add(ms.maps[j]);
							}
						}
						ms.maps.Clear();
						m_mapsPass++;
						p_callback?.Invoke();
					}
					custom_maps_request = UpdateLocalCommunityMaps(!has_community_cache, m_maps, p_result);
				}, !has_cache);
			});
			WebAsyncRequest UpdateLocalCommunityMaps(bool p_full, List<MapData> p_drlMaps, MapData[] p_newMaps)
			{
				return base.app.model.service.UpdateLocalCommunityMaps(delegate(MapData[] p_communityMaps)
				{
					Notify("boot.drl.offline-maps.download@start");
					if (p_communityMaps == null || p_communityMaps.Length == 0 || DRLApp.offline)
					{
						Debug.Log("DRLBootController> LoadState / No new community map updates.");
						if (!DRLApp.offline && m_hasMapUpdates)
						{
							base.app.model.storage.maps.SaveCache(p_drlMaps, p_newMaps, delegate
							{
								if (p_drlMaps != null)
								{
									p_drlMaps.Clear();
									p_drlMaps = null;
									p_newMaps = null;
								}
								Notify("boot.drl.offline-maps.download@progress", 1f);
								GCCollect();
								m_mapsPass++;
								p_callback?.Invoke();
							});
						}
						else
						{
							GCCollect();
							m_mapsPass++;
							p_callback?.Invoke();
						}
					}
					else
					{
						ms.StoreCommunityMaps(p_communityMaps, delegate
						{
							Debug.Log("DRLBootController> LoadState / New community map updates - updated[" + p_communityMaps.Length + "] maps.");
							p_communityMaps = null;
							if (!DRLApp.offline && m_hasMapUpdates)
							{
								base.app.model.storage.maps.SaveCache(p_drlMaps, p_newMaps, delegate
								{
									if (p_drlMaps != null)
									{
										p_drlMaps.Clear();
										p_drlMaps = null;
										p_newMaps = null;
									}
									Notify("boot.drl.offline-maps.download@progress", 1f);
									GCCollect();
									m_mapsPass++;
									p_callback?.Invoke();
								});
							}
							else
							{
								GCCollect();
								m_mapsPass++;
								p_callback?.Invoke();
							}
						});
					}
				}, p_full);
			}
		}

		protected void OnMapsDownloadComplete()
		{
			List<string> list = new List<MapData>(base.app.model.storage.maps.maps).ConvertAll((MapData it) => string.Format("[{0}][{1}][{2}] ", it.mapCategoryFlag, it.mapId, it.mode.race.allowed ? "R" : "F") + it.mapTitle);
			list.Sort();
			string.Join("\n", list);
			GCCollect();
		}

		protected void LoadPlayerState(Action p_on_complete)
		{
			ServiceModel service = base.app.model.service;
			StorageModel stm = base.app.model.storage;
			if (DRLApp.offline)
			{
				stm.LoadPlayerState(delegate
				{
					if (!base.validContext)
					{
						Debug.Log("DRLBootController> service.State / Invalid Context");
					}
					else if (!base.app.model.storage.state.ready)
					{
						Debug.Log("DRLBootController> LoadState / Player State Load");
						if (p_on_complete != null)
						{
							p_on_complete();
						}
					}
				});
			}
			else if (stm.HasOfflineData())
			{
				service.State(delegate
				{
					if (!base.validContext)
					{
						Debug.Log("DRLBootController> service.State / Invalid Context");
					}
					else
					{
						if (!stm.ValidateLocalPlayerId(stm.state.player.profile.playerId))
						{
							Debug.Log("DRLBootController> service.State / Cache Invalidated");
							stm.state.player.profile.invalidateCache = true;
						}
						if (stm.state.player.profile.invalidateCache)
						{
							stm.state.player.profile.invalidateCache = false;
							base.app.model.service.State(delegate
							{
								StoreLocalPlayerState();
								if (p_on_complete != null)
								{
									p_on_complete();
								}
							});
						}
						else
						{
							stm.LoadPlayerState(delegate(bool p_success)
							{
								if (!base.validContext)
								{
									Debug.Log("DRLBootController> service.State / Invalid Context");
								}
								else if (!base.app.model.storage.state.ready)
								{
									Debug.Log("DRLBootController> service.State / Fetching local player state data - " + p_success);
									if (p_success)
									{
										base.app.model.service.State(delegate
										{
											DownloadPlayerState(p_on_complete);
										}, stm.state.player.data.hash);
									}
									else
									{
										DownloadPlayerState(p_on_complete);
									}
								}
							});
						}
					}
				});
			}
			else
			{
				DownloadPlayerState(p_on_complete);
			}
		}

		protected void DownloadPlayerState(Action p_on_complete)
		{
			base.app.model.service.State(delegate(DRLServiceResult p_result)
			{
				if (!base.validContext)
				{
					Debug.Log("DRLBootController> service.State / Invalid Context");
				}
				else if (!base.app.model.storage.state.ready)
				{
					Debug.Log("DRLBootController> LoadState / Player State Load");
					Dictionary<string, string> data = p_result.GetData<Dictionary<string, string>>();
					StoreLocalPlayerState(data);
					if (p_on_complete != null)
					{
						p_on_complete();
					}
				}
			});
		}

		private void StoreLocalPlayerState(Dictionary<string, string> p_data = null)
		{
			StorageModel storage = base.app.model.storage;
			string contents = Serialize.ToJson((p_data == null) ? ((object)storage.state.player.data.data) : ((object)p_data));
			File.WriteAllText(DRLPaths.Storage.offlinePlayerStateFile, contents);
			Texture2D texture2D = storage.state.player.profile.photo as Texture2D;
			if (texture2D != null)
			{
				File.WriteAllBytes(DRLPaths.Storage.offlinePlayerStatePicture, texture2D.EncodeToPNG());
			}
		}

		protected void UpdateState()
		{
			Debug.Log($"DRLBootController> UpdateState / write-platform-data[{writeUserPlatformData}]");
			ProfileStateModel psm = base.app.model.storage.state.player.profile;
			PlatformService platform = base.app.model.service.platform;
			if (writeUserPlatformData)
			{
				psm.username = platform.playerName;
				psm.languageISO = platform.languageISO;
				psm.countryISO = platform.countryISO;
				psm.platformId = platform.id.ToString();
				psm.playerId = base.app.model.service.backend.playerId;
				psm.colorHex = psm.colorHex;
				psm.branchId = DRLApp.branchName;
				List<string> range = platform.blockedUserIds.GetRange(0, Mathf.Min(platform.blockedUserIds.Count, 100));
				psm.blockList = JsonConvert.SerializeObject(range);
				psm.photo = platform.playerThumbBig;
				long p_size = 0L;
				int replayFilesInfo = base.app.model.storage.GetReplayFilesInfo(out p_size);
				psm.storageReplayFileCount = replayFilesInfo;
				psm.storageReplayMemoryUsage = p_size.ToString();
				SteamService steamService = platform as SteamService;
				psm.steamInstallPath = steamService.appDirectoryPath;
				psm.steamUnixSecondsFromPurchase = steamService.unixSecondsSincePurchase;
			}
			else
			{
				Debug.Log("DRLBootController> UpdateState / Local Platform Data - player-name[" + psm.username + "] platform-id[" + psm.platformId + "]");
				platform.playerName = psm.username;
				platform.id = psm.platformId;
				base.app.model.service.GetPlayerAvatar(base.app.model.storage.state.player.profile.playerId, delegate(Texture2D p_data)
				{
					psm.photo = p_data;
				});
			}
			base.app.model.storage.state.player.garage.PreloadDrones();
			AppSystemInfo systemInfo = DRLApp.GetSystemInfo();
			float num = GraphicsStateModel.GetHardwareScore();
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			Debug.Log("DRLBootController> UpdateState / graphics-state[" + graphics.name + "]");
			graphics.InitializeQualityByScore(num);
			int num2 = (int)graphics.resolution[0];
			int num3 = (int)graphics.resolution[1];
			Debug.Log("DRLBootController> UpdateState / Welcome User[" + psm.username + "]");
			_ = new string[9]
			{
				"  platform-id:    ",
				psm.platformId,
				"  is-developer:   ",
				psm.isDeveloper.ToString(),
				"  flags:          [",
				string.Join(",", platform.flags) + "]",
				"  hardware-score: " + num.ToString("0.00"),
				"  quality:        " + graphics.quality,
				"  resolution:     " + num2 + "x" + num3
			};
			systemInfo.hardwareScore = num.ToString("0.00");
			string systemInfo2 = Serialize.ToJson(systemInfo);
			if (writeUserPlatformData)
			{
				base.app.model.storage.state.player.systemInfo = systemInfo2;
			}
			base.app.model.service.stateAutoRefresh = true;
			LoadContentManifest();
		}

		protected void LoadContentManifest()
		{
			Debug.Log("DRLBootController> LoadContentManifest");
			Notify("boot.drl.content.manifest");
			if (Application.isEditor)
			{
				InitializeBundleFilesData();
				return;
			}
			string text = "release";
			string p_platform = "temp";
			text = "release";
			switch (OS.prefix)
			{
			case "win":
				p_platform = "win";
				break;
			case "osx":
				p_platform = "osx";
				break;
			case "xbox":
				p_platform = "xbox";
				break;
			case "ps4":
				p_platform = "ps4";
				break;
			}
			base.app.model.service.GetContentManifest(text, p_platform, delegate(DRLServiceResult p_result)
			{
				List<DRLContentManifestData> list = null;
				if (!p_result.success)
				{
					Debug.LogWarning("DRLBootController> LoadContentManifest / Manifest Load Failed - Skipping...\n  " + p_result.message);
					InitializeBundleFilesData();
				}
				else
				{
					list = ((list == null) ? p_result.GetData<List<DRLContentManifestData>>() : list);
					int num = list?.Count ?? 0;
					Debug.Log(string.Format("DRLBootController> LoadContentManifest / Found {0} Manifests{1}", num, (list == null) ? "- Invalid Manifest Data" : ""));
					string manifestRoot = DRLPaths.Storage.manifestRoot;
					if (list != null)
					{
						for (int i = 0; i < list.Count; i++)
						{
							string id = list[i].id;
							if (new FileInfo(manifestRoot + id).Exists)
							{
								Debug.Log("DRLBootController> ProcessManifests / Manifest " + id + " - Already Processed!");
								list.RemoveAt(i--);
							}
						}
					}
					num = list?.Count ?? 0;
					if (num <= 0)
					{
						InitializeBundleFilesData();
					}
					else
					{
						ProcessManifests(list);
					}
				}
			});
		}

		protected void ProcessManifests(List<DRLContentManifestData> p_list)
		{
			Notify("boot.drl.content.download@start");
			List<DRLContentManifestData> ml = new List<DRLContentManifestData>(p_list);
			List<DRLManifestOperation> opl = new List<DRLManifestOperation>();
			string state = "fetch-manifest";
			string content_root = DRLPaths.Storage.root;
			string manifests_root = DRLPaths.Storage.manifestRoot;
			DRLContentManifestData c_manifest = null;
			DRLManifestOperation c_manifest_operation = null;
			WebAsyncRequest c_operation_loader = null;
			int operations_t = 0;
			int operations_idx = 0;
			float action_p = 0f;
			for (int i = 0; i < ml.Count; i++)
			{
				List<DRLManifestOperation> operations = ml[i].GetOperations();
				operations_t += operations.Count;
			}
			string c_manifest_id = "";
			string c_manifest_fp = "";
			string c_operation_fp = "";
			string c_operation_lp = "";
			int c_operation_retry = 0;
			bool c_manifest_error = false;
			Activity.Run((Predicate<float>)delegate
			{
				if (state != null)
				{
					switch (state)
					{
					case "fetch-manifest":
						if (ml.Count <= 0)
						{
							Debug.Log("DRLBootController> ProcessManifests / All Manifests Processed!");
							Notify("boot.drl.content.download@complete");
							InitializeBundleFilesData();
							return false;
						}
						c_manifest = ml[0];
						ml.RemoveAt(0);
						if (c_manifest == null)
						{
							return true;
						}
						state = "init-manifest";
						break;
					case "init-manifest":
						c_manifest_id = c_manifest.id;
						c_manifest_fp = manifests_root + c_manifest_id;
						Debug.Log("DRLBootController> ProcessManifests / Manifest " + c_manifest_id + " Init");
						opl.Clear();
						opl.AddRange(c_manifest.GetOperations());
						state = "fetch-operation";
						break;
					case "fetch-operation":
						if (opl.Count <= 0)
						{
							string contents = c_manifest.ToJson(p_indented: true);
							try
							{
								if (!c_manifest_error)
								{
									Debug.Log("DRLBootController> ProcessManifests / Manifest " + c_manifest_id + " Finished!\n  " + c_manifest_fp);
									File.WriteAllText(c_manifest_fp, contents);
								}
								else
								{
									Debug.LogWarning("DRLBootController> ProcessManifests / Manifest " + c_manifest_id + " Not Saved due Errors!");
								}
							}
							catch (Exception ex3)
							{
								Debug.LogWarning("DRLBootController> ProcessManifests / Manifest " + c_manifest_id + " - Manifest Write ERROR!\n" + ex3.Message);
							}
							state = "fetch-manifest";
							return true;
						}
						c_manifest_operation = opl[0];
						opl.RemoveAt(0);
						if (c_manifest_operation == null)
						{
							return true;
						}
						c_operation_retry = 0;
						state = "execute-operation";
						break;
					case "execute-operation":
					{
						switch (c_manifest_operation.type)
						{
						case ManifestActionType.Invalid:
							Debug.Log($"DRLBootController> ProcessManifests / Manifest {c_manifest_id} - {c_manifest_operation.type} Operation");
							operations_idx++;
							action_p = operations_idx;
							state = "fetch-operation";
							break;
						case ManifestActionType.Remove:
						{
							c_operation_lp = c_manifest_operation.file.localPath.Replace("content/", "");
							c_operation_fp = content_root + c_operation_lp;
							FileInfo fileInfo3 = new FileInfo(c_operation_fp);
							Debug.Log(string.Format("DRLBootController> ProcessManifests / Manifest {0} - {1} Action | File {2}\n {3}", c_manifest_id, c_manifest_operation.type, fileInfo3.Exists ? "DELETE" : "NOT FOUND", c_operation_fp));
							if (fileInfo3.Exists)
							{
								try
								{
									fileInfo3.Delete();
								}
								catch (Exception ex2)
								{
									Debug.LogWarning($"DRLBootController> ProcessManifests / Manifest {c_manifest_id} - {c_manifest_operation.type} Action | ERROR!\n{ex2.Message}");
								}
							}
							operations_idx++;
							action_p = operations_idx;
							state = "fetch-operation";
							break;
						}
						case ManifestActionType.Add:
						case ManifestActionType.Update:
							if (c_operation_loader != null)
							{
								action_p = (float)operations_idx + c_operation_loader.progress;
								if (c_operation_loader.completed)
								{
									bool flag = c_operation_loader.code / 100 != 2;
									if (c_operation_loader.hasError)
									{
										if (c_operation_retry >= 10)
										{
											Debug.LogWarning($"DRLBootController> ProcessManifests / Manifest {c_manifest_id} - {c_manifest_operation.type} Operation | Retry Failure, Skipping...");
											operations_idx++;
											action_p = operations_idx;
											c_manifest_error = true;
											state = "fetch-action";
										}
										else
										{
											Debug.LogWarning($"DRLBootController> ProcessManifests / Manifest {c_manifest_id} - {c_manifest_operation.type} Operation | Download Error\n {c_operation_loader.error}");
											c_operation_loader = null;
											c_operation_retry++;
										}
									}
									else
									{
										c_operation_lp = c_manifest_operation.file.localPath.Replace("content/", "");
										c_operation_fp = content_root + c_operation_lp;
										Directory.CreateDirectory(System.IO.Path.GetDirectoryName(c_operation_fp));
										FileInfo fileInfo = new FileInfo(c_operation_fp);
										byte[] array = c_operation_loader.Get<byte[]>();
										Debug.Log(string.Format("DRLBootController> ProcessManifests / Manifest {0} - {1} Action | Download Complete | Code {2} - {3} kb | {4}\n{5}", c_manifest_id, c_manifest_operation.type, c_operation_loader.code, array.Length / 1024, fileInfo.Exists ? "UPDATE" : "ADD", c_operation_fp));
										try
										{
											if (!flag)
											{
												File.WriteAllBytes(c_operation_fp, array);
											}
											else
											{
												c_manifest_error = true;
											}
										}
										catch (Exception ex)
										{
											Debug.LogWarning($"DRLBootController> ProcessManifests / Manifest {c_manifest_id} - {c_manifest_operation.type} Action | ERROR!\n{ex.Message}");
											FileInfo fileInfo2 = new FileInfo(c_operation_fp);
											if (fileInfo2.Exists)
											{
												fileInfo2.Delete();
											}
											c_manifest_error = true;
										}
										WebAsyncRequest req = c_operation_loader;
										Activity.RunOnce(delegate
										{
											req.loader.Dispose();
										}, 1f / 12f);
										c_operation_loader = null;
										operations_idx++;
										action_p = operations_idx;
										state = "fetch-operation";
									}
								}
							}
							else
							{
								string url = c_manifest_operation.file.url;
								c_operation_loader = Web.Get<byte[]>("manifest.operation." + c_manifest.id, url, delegate
								{
								});
							}
							break;
						}
						float num = Mathf.Clamp01(action_p / (float)operations_t);
						Notify("boot.drl.content.download@progress", num);
						break;
					}
					}
				}
				return true;
			}, 0f, false);
		}

		protected void InitializeBundleFilesData()
		{
			Debug.Log("DRLBootController> InitializeBundleFilesData");
			bool is_low = DRLPaths.Content.useLibraryLow;
			new Thread((ThreadStart)delegate
			{
				DRLPaths.CollectBundleFiles();
				string remove_path = (is_low ? "/high" : "/low");
				DRLPaths.bundleFiles.RemoveAll((string v) => v.Contains(remove_path));
				List<string> bundleFiles = DRLPaths.Content.bundleFiles;
				List<string> bundleFiles2 = DRLPaths.Storage.bundleFiles;
				Debug.Log($"DRLBootController> CollectBundleFiles / {DRLPaths.bundleFiles.Count} Files\n  Content: {bundleFiles.Count} files\n  Storage: {bundleFiles2.Count} files");
				Activity.RunOnce(BundlesLoadFromDisk, 1f / 60f);
			}).Start();
		}

		protected void BundlesLoadFromDisk()
		{
			Debug.Log("DRLBootController> BundlesLoadFromDisk / path[" + DRLPaths.Content.root + "]");
			Notify("boot.drl.bundle.load@start");
			Debug.Log("DRLBootController> BundlesLoadFromDisk / Locking FPS/VSync to safeguard async operations");
			int p_value = 60;
			base.app.controller.settings.SetFps(p_value, p_vsync: false);
			float bundle_p = 0f;
			float bundle_np = 0f;
			int bundle_complete = 0;
			float[] bundle_progress_l = new float[6];
			int bundle_complete_count = bundle_progress_l.Length;
			float bundle_p_frac = 1f / (float)bundle_complete_count;
			Activity.Run((Func<bool>)delegate
			{
				if (bundle_complete >= bundle_complete_count)
				{
					Notify("boot.drl.bundle.load@progress", 1f);
					Notify("boot.drl.bundle.load@complete");
					BundleLoadFromDiskComplete();
					return false;
				}
				float num = 0f;
				for (int i = 0; i < bundle_progress_l.Length; i++)
				{
					num += bundle_progress_l[i];
				}
				bundle_np = Mathf.Clamp01(num * bundle_p_frac);
				if (Mathf.Abs(bundle_np - bundle_p) <= 0f)
				{
					return true;
				}
				bundle_p = Mathf.Lerp(bundle_p, bundle_np, Time.deltaTime * 1f);
				if (Mathf.Abs(bundle_np - bundle_p) <= 0.01f)
				{
					bundle_p = bundle_np;
				}
				Notify("boot.drl.bundle.load@progress", bundle_p * 0.99f);
				return true;
			}, 0f, false);
			bool is_low = DRLPaths.Content.useLibraryLow;
			string[] pack_manifests = DRLPaths.GetBundleFiles("manifest-pack-dr|manifest-pack-me").ToArray();
			string pack_dr_manifest = pack_manifests[0];
			Action load_dynamic_abl = null;
			Action load_drone_selection_abl = null;
			Action load_drone_parts_abl = null;
			Action load_static_abl = null;
			DRLAssetBundleLibrary slb = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.Static);
			DRLAssetBundleLibrary dlb = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.Dynamic);
			DRLAssetBundleLibrary dplb = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.DroneParts);
			DRLAssetBundleLibrary dslb = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.DroneSelection);
			slb.batchLoadSize = 10u;
			dlb.batchLoadSize = 10u;
			dplb.batchLoadSize = 10u;
			dslb.batchLoadSize = 10u;
			slb.batchLoadAsync = false;
			dlb.batchLoadAsync = false;
			dplb.batchLoadAsync = false;
			dslb.batchLoadAsync = false;
			bool manifest_parse_active = true;
			DRLAssetBundleLibrary lb;
			new Thread((ThreadStart)delegate
			{
				lb = slb;
				lb.bundleFiles = new List<string>(DRLPaths.bundleFiles);
				lb.LoadManifests(pack_manifests, p_cache: true);
				lb = dlb;
				lb.bundleFiles = new List<string>(DRLPaths.bundleFiles);
				lb.LoadManifests(pack_manifests);
				lb.LoadCache(slb.dependencyCache);
				lb = dplb;
				lb.bundleFiles = new List<string>(DRLPaths.bundleFiles);
				lb.LoadManifests(new string[1] { pack_dr_manifest });
				lb.LoadCache(slb.dependencyCache);
				lb = dslb;
				lb.bundleFiles = new List<string>(DRLPaths.bundleFiles);
				lb.LoadManifests(new string[1] { pack_dr_manifest });
				lb.LoadCache(slb.dependencyCache);
				manifest_parse_active = false;
			}).Start();
			load_static_abl = LoadAssetBundleLibrary(StorageAssetBundleLibraryId.Static, pack_manifests, p_load_dependencies: true, delegate(int p_state, float p_progress)
			{
				switch (p_state)
				{
				case 0:
				{
					if (is_low)
					{
						return (List<string>)null;
					}
					lb = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.DroneParts);
					List<string> gUIDs = lb.GetGUIDs();
					Debug.Log($"DRLBootController> BundlesLoadFromDisk / Found {gUIDs.Count} drone parts GUIDs!");
					return gUIDs;
				}
				case 1:
					bundle_progress_l[4] = p_progress;
					if (!(p_progress < 1f))
					{
						bundle_complete++;
						load_dynamic_abl();
					}
					break;
				}
				return (List<string>)null;
			});
			load_dynamic_abl = LoadAssetBundleLibrary(StorageAssetBundleLibraryId.Dynamic, pack_manifests, p_load_dependencies: true, delegate(int p_state, float p_progress)
			{
				switch (p_state)
				{
				case 0:
					return new List<string>();
				case 1:
					if (!(p_progress < 1f))
					{
						bundle_complete++;
						bundle_progress_l[1] = p_progress;
						load_drone_parts_abl();
					}
					break;
				}
				return (List<string>)null;
			});
			load_drone_parts_abl = LoadAssetBundleLibrary(StorageAssetBundleLibraryId.DroneParts, new string[1] { pack_dr_manifest }, p_load_dependencies: false, delegate(int p_state, float p_progress)
			{
				switch (p_state)
				{
				case 0:
					if (!is_low)
					{
						return (List<string>)null;
					}
					lb = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.DroneParts);
					return lb.GetGUIDs();
				case 1:
					bundle_progress_l[3] = p_progress;
					if (!(p_progress < 1f))
					{
						bundle_complete++;
						lb = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.DroneParts);
						lb.UnloadDependencies(p_all: false);
						load_drone_selection_abl();
					}
					break;
				}
				return (List<string>)null;
			});
			load_drone_selection_abl = LoadAssetBundleLibrary(StorageAssetBundleLibraryId.DroneSelection, new string[1] { pack_dr_manifest }, p_load_dependencies: true, delegate(int p_state, float p_progress)
			{
				switch (p_state)
				{
				case 0:
				{
					if (!is_low)
					{
						return (List<string>)null;
					}
					PlayerStateModel player = base.app.model.storage.state.player;
					return ((player.garage.currentRigData == null) ? player.garage.defaultRig : player.garage.currentRigData).dependencies;
				}
				case 1:
					bundle_progress_l[2] = p_progress;
					if (!(p_progress < 1f))
					{
						bundle_complete++;
					}
					break;
				}
				return (List<string>)null;
			});
			AsyncManager loader = AsyncManager.instance;
			BundleAsyncRequest bundle_req = null;
			string bundle_query_id = "boot.load.mission.bundle";
			bool slow_loading = DRLPaths.Content.useLibraryLow;
			List<string> bundle_missions = DRLPaths.GetBundleFilesByFilter(false, "/missions/");
			float bundle_count = bundle_missions.Count;
			float bundle_load_interval = 0f;
			int bundle_idx = 0;
			Activity.Run((Func<bool>)delegate
			{
				if (bundle_load_interval > 0f)
				{
					bundle_load_interval -= Time.deltaTime;
					return true;
				}
				string text = ((bundle_count <= 0f) ? "" : bundle_missions[bundle_idx]);
				if (!string.IsNullOrEmpty(text) && bundle_req == null)
				{
					Debug.Log("DRLBootController> BundlesLoadFromDisk / ReadBundle path[" + text + "]");
					bundle_req = (BundleAsyncRequest)loader.ReadBundle(bundle_query_id, text, p_active_scenes: true);
					bundle_req.persistent = true;
					return true;
				}
				bundle_progress_l[0] = ((bundle_count <= 0f) ? 1f : Mathf.Clamp01(((float)bundle_idx + bundle_req.progress) / (bundle_count - 1f)));
				if (bundle_req != null)
				{
					_ = bundle_req.loader;
				}
				if (bundle_req != null && !bundle_req.hasError && !bundle_req.completed)
				{
					return true;
				}
				AssetBundle assetBundle = ((bundle_req == null) ? null : bundle_req.Get<AssetBundle>());
				string[] array = ((assetBundle == null) ? new string[0] : assetBundle.GetAllScenePaths());
				if (array.Length != 0)
				{
					Debug.Log(string.Format("DRLBootController> BundlesLoadFromDisk / Scene Bundle - count[{0}]\n{1}", array.Length, string.Join("\n", array)));
					base.app.level.AddBundle(assetBundle);
				}
				bundle_req = null;
				bundle_idx++;
				if ((float)bundle_idx >= bundle_count)
				{
					Debug.Log("DRLBootController> BundlesLoadFromDisk / Missions Pack Bundles Complete!");
					Activity.Run((Func<bool>)delegate
					{
						bundle_progress_l[5] = Mathf.Clamp01(bundle_progress_l[5] + Time.deltaTime / 10f);
						if (manifest_parse_active)
						{
							return true;
						}
						bundle_progress_l[5] = 1f;
						bundle_complete++;
						load_static_abl();
						return false;
					}, 0f, false);
					bundle_progress_l[0] = 1f;
					bundle_complete++;
					return false;
				}
				bundle_progress_l[0] = Mathf.Clamp01((float)bundle_idx / (bundle_count - 1f));
				bundle_load_interval = (slow_loading ? (1f / 30f) : 0f);
				return true;
			}, 0f, false);
		}

		protected Action LoadAssetBundleLibrary(string p_id, string[] p_manifests, bool p_load_dependencies, Func<int, float, List<string>> p_on_status)
		{
			return delegate
			{
				DRLAssetBundleLibrary lb = base.app.model.storage.GetAssetBundleLibrary(p_id);
				List<string> list = new List<string>();
				if (p_on_status != null)
				{
					list = p_on_status(0, 0f);
					if (list == null)
					{
						list = new List<string>();
					}
				}
				if (list.Count <= 0)
				{
					Debug.Log("DRLBootController> LoadAssetBundleLibrary / [" + lb.name + "] Complete / NO GUIDS PROVIDED");
					if (p_on_status != null)
					{
						p_on_status(1, 1f);
					}
				}
				else
				{
					lb.LoadAssetsAsync(list, p_load_dependencies, delegate(float p)
					{
						if (p >= 1f)
						{
							Debug.Log("DRLBootController> LoadAssetBundleLibrary / [" + lb.name + "] Complete!");
						}
						if (p_on_status != null)
						{
							p_on_status(1, p);
						}
					});
				}
			};
		}

		protected void BundleLoadFromDiskComplete()
		{
			Debug.Log("DRLBootController> Bundles Loading Complete!");
			OnComplete();
		}

		public void SaveReplayCache(byte[] p_data, string replayPath)
		{
			base.app.model.storage.replays.SaveReplayCache(replayPath, p_data);
		}

		public void DeleteReplayCache()
		{
			base.app.model.storage.replays.DeleteReplayCache("onboarding-replay*");
		}

		public void DownloadOnboardingCache()
		{
			if (!DRLApp.offline && !DRLApp.forceOffline)
			{
				Debug.Log("DRLBootController> DownloadOnboardingCache");
				OpponentModel opponent = base.app.model.service.opponent;
				GetReplay(opponent, OnboardingCampaignMode.Beginner, beginnerReplayId, 0);
			}
		}

		public bool HasCache()
		{
			new ReplayFile();
			string p_key = "onboarding-replay-" + OnboardingCampaignMode.Beginner.ToString() + 0;
			if (base.app.model.storage.replays.GetReplayCache(p_key) == null)
			{
				return false;
			}
			p_key = "onboarding-replay-" + OnboardingCampaignMode.Intermediate.ToString() + 0;
			if (base.app.model.storage.replays.GetReplayCache(p_key) == null)
			{
				return false;
			}
			p_key = "onboarding-replay-" + OnboardingCampaignMode.Pro.ToString() + 0;
			if (base.app.model.storage.replays.GetReplayCache(p_key) == null)
			{
				return false;
			}
			p_key = "onboarding-replay-" + OnboardingCampaignMode.Pro.ToString() + 1;
			if (base.app.model.storage.replays.GetReplayCache(p_key) == null)
			{
				return false;
			}
			p_key = "onboarding-replay-" + OnboardingCampaignMode.Pro.ToString() + 2;
			if (base.app.model.storage.replays.GetReplayCache(p_key) == null)
			{
				return false;
			}
			return true;
		}

		private void GetReplay(OpponentModel om, OnboardingCampaignMode campaignMode, string replayId, int step)
		{
			om.Cancel();
			string replayPath = "onboarding-replay-" + campaignMode.ToString() + step;
			base.app.controller.onboarding.selectedDifficulty = campaignMode;
			om.Load(replayId, campaignMode, delegate
			{
				if (om.status == OpponentModel.Status.Complete)
				{
					switch (campaignMode)
					{
					case OnboardingCampaignMode.Beginner:
						SaveReplayCache(om.replayV2OpponentsBytes, replayPath);
						GetReplay(om, OnboardingCampaignMode.Intermediate, intermediateReplayId, 0);
						break;
					case OnboardingCampaignMode.Intermediate:
						SaveReplayCache(om.replayV2OpponentsBytes, "onboarding-replay-" + OnboardingCampaignMode.Intermediate.ToString() + 0);
						GetReplay(om, OnboardingCampaignMode.Pro, proReplayId0, 0);
						break;
					case OnboardingCampaignMode.Pro:
						if (replayId.Equals(proReplayId0))
						{
							SaveReplayCache(om.replayV2OpponentsBytes, "onboarding-replay-" + OnboardingCampaignMode.Pro.ToString() + 0);
							GetReplay(om, OnboardingCampaignMode.Pro, proReplayId1, 1);
						}
						if (replayId.Equals(proReplayId1))
						{
							SaveReplayCache(om.replayV2OpponentsBytes, "onboarding-replay-" + OnboardingCampaignMode.Pro.ToString() + 1);
							GetReplay(om, OnboardingCampaignMode.Pro, proReplayId2, 2);
						}
						if (replayId.Equals(proReplayId2))
						{
							SaveReplayCache(om.replayV2OpponentsBytes, "onboarding-replay-" + OnboardingCampaignMode.Pro.ToString() + 2);
						}
						break;
					}
				}
			});
		}

		private void GCCollect()
		{
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.Default;
		}

		protected void OnComplete()
		{
			ready = true;
			m_run_lock = false;
			Notify(0.25f, "boot@complete");
		}
	}
}
