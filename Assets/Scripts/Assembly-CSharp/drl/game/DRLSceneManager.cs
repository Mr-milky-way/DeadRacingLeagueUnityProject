using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.network;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLSceneManager : Controller<DRLApp>
	{
		public StorageModel storage;

		public SettingsController settings;

		public NetworkModel network;

		public DRLAppArguments arguments;

		public UILoaderView loader;

		public bool isMapEditor;

		public DRLMap map;

		public string customMap;

		public DRLMapTrack track;

		public DRLMission mission;

		public List<long> memorySamples;

		public List<AssetBundle> bundles;

		public long m_load_t0;

		public long m_load_t1;

		public long m_load_dt;

		public LevelManager manager => AssertLocal<LevelManager>("manager");

		public void OnPersistency()
		{
			base.app.scene = this;
		}

		[ContextMenu("Load Main")]
		public void ForceLoadMain()
		{
			LoadMain(p_force: true);
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			UIView ui = null;
			bool flag = (bool)base.app && base.app.arguments.game.isCustomMap;
			switch (p_event)
			{
			case "scene.track.build@start":
			case "scene.track.build@progress":
			case "scene.track.build@complete":
				if ((bool)base.app && (bool)base.app.view)
				{
					ui = base.app.view.ui;
				}
				break;
			}
			switch (p_event)
			{
			case "scene.track.build@start":
				if ((bool)ui)
				{
					ui.loader.progress = (flag ? 0.8f : ui.loader.progress);
				}
				break;
			case "scene.track.build@progress":
			{
				float num = (float)p_data[0];
				if ((bool)ui)
				{
					ui.loader.progress = (flag ? (0.8f + num * 0.2f) : ui.loader.progress);
				}
				break;
			}
			case "scene.track.build@complete":
				m_load_t1 = DateTime.Now.Ticks;
				m_load_dt = m_load_t1 - m_load_t0;
				Debug.Log($"DRLSceneManager> TrackBuildComplete / duration[{(int)new TimeSpan(m_load_dt).TotalSeconds}s]");
				if ((bool)ui)
				{
					ui.loader.progress = 1f;
					ui.fade.transition = 0f;
					ui.loader.fade.FadeOut(0.5f, 2.6f);
					this.TimerRunOnce(delegate
					{
						ui.loader.fade.alpha = 0f;
					}, 3.1f);
				}
				break;
			}
		}

		public GameObject GetMapRoot()
		{
			DRLMap dRLMap = map;
			if (!manager)
			{
				return null;
			}
			if (!dRLMap)
			{
				return null;
			}
			if (!manager.IsLevelLoaded(dRLMap.scene))
			{
				return null;
			}
			return LevelManager.GetRootGameObject(dRLMap.scene, "level");
		}

		public GameObject[] GetTrackSceneObjects()
		{
			if (!track)
			{
				return new GameObject[0];
			}
			if (!manager.IsLevelLoaded(track.scene))
			{
				return new GameObject[0];
			}
			return LevelManager.GetRootGameObjects(track.scene);
		}

		public GameObject GetTrackRoot()
		{
			if (!track)
			{
				return null;
			}
			if (!manager.IsLevelLoaded(track.scene))
			{
				return null;
			}
			GameObject rootGameObject = LevelManager.GetRootGameObject(track.scene, "tracks");
			if (!rootGameObject)
			{
				return null;
			}
			Transform transform = null;
			for (int i = 0; i < rootGameObject.transform.childCount; i++)
			{
				Transform child = rootGameObject.transform.GetChild(i);
				bool flag = child.name == track.id;
				child.gameObject.SetActive(flag);
				if (flag)
				{
					transform = child;
				}
			}
			if (!transform)
			{
				return null;
			}
			return transform.gameObject;
		}

		public T GetMissionRoot<T>() where T : Component
		{
			if (!mission)
			{
				return null;
			}
			if (!manager.IsLevelLoaded(mission.scene))
			{
				return null;
			}
			GameObject[] rootGameObjects = LevelManager.GetRootGameObjects(mission.scene);
			foreach (GameObject gameObject in rootGameObjects)
			{
				if ((bool)gameObject)
				{
					T component = gameObject.GetComponent<T>();
					if ((bool)component)
					{
						return component;
					}
				}
			}
			return null;
		}

		public GameObject GetMissionRoot(string p_name)
		{
			if (!mission)
			{
				return null;
			}
			if (!manager.IsLevelLoaded(mission.scene))
			{
				return null;
			}
			GameObject[] rootGameObjects = LevelManager.GetRootGameObjects(mission.scene);
			foreach (GameObject gameObject in rootGameObjects)
			{
				if ((bool)gameObject && gameObject.name == p_name)
				{
					return gameObject;
				}
			}
			return null;
		}

		public void LoadScene(string p_name)
		{
			Clear();
			manager.LoadLevel(p_name);
		}

		public void LoadMain(bool p_force)
		{
			Debug.Log($"DRLSceneManager> LoadMain / force[{p_force}]");
			Debug.Log("DRLSceneManager> LoadMain / Clear Start");
			base.app?.model?.service?.opponent?.ForceResetLoadedReplays();
			DRLApp.LogMemStats($"DRLSceneManager> LoadMain / Start - force[{p_force}]", p_show_delta: true);
			Clear();
			Debug.Log("DRLSceneManager> LoadMain / Clear Stop");
			GameObject gameObject = GameObject.Find("dr-background");
			if (gameObject != null)
			{
				RawImage component = gameObject.GetComponent<RawImage>();
				if (component != null && component.texture != null && component.texture is RenderTexture renderTexture)
				{
					renderTexture.DiscardContents();
					RenderTexture.ReleaseTemporary(renderTexture);
					component.texture = null;
				}
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
			StorageModel storageModel = storage;
			if ((bool)storageModel)
			{
				DRLApp.LogMemStats("DRLSceneManager> LoadMain / Storage Clean Start", p_show_delta: true);
				storageModel.GetAssetBundleLibrary(StorageAssetBundleLibraryId.Dynamic).UnloadLibrary();
				DRLApp.LogMemStats("DRLSceneManager> LoadMain / Dynamic Library Clean", p_show_delta: true);
				DRLAssetBundleLibrary assetBundleLibrary = storageModel.GetAssetBundleLibrary(StorageAssetBundleLibraryId.DroneSelection);
				assetBundleLibrary.UnloadLibrary();
				DRLApp.LogMemStats("DRLSceneManager> LoadMain / DroneSelection LibraryClean", p_show_delta: true);
				DroneRigData rd = storageModel.state.player.garage.currentRigData;
				rd = ((rd == null) ? storageModel.state.player.garage.defaultRig : rd);
				if ((bool)storageModel.state.player.circuits)
				{
					storageModel.state.player.circuits.ClearInProgress();
				}
				assetBundleLibrary.LoadAssetsAsync(rd.dependencies, p_include_dependency: true, delegate(float p)
				{
					if (!(p < 1f))
					{
						DRLApp.LogMemStats($"DRLSceneManager> LoadMain / Drone Dependencies Load Complete - count[{rd.dependencies}]", p_show_delta: true);
						manager.LoadLevel("main", p_force);
						DRLApp.LogMemStats("DRLSceneManager> LoadMain / LoadLevel Finish", p_show_delta: true);
					}
				});
			}
			else
			{
				manager.LoadLevelAsync("main");
			}
			GCCollect();
		}

		public void LoadMain()
		{
			LoadMain(p_force: false);
		}

		public void LoadSandbox()
		{
			Clear();
			manager.LoadLevel("drone-simulation-sandbox");
		}

		public void ExitGame()
		{
			base.app.view.ui.fade.FadeIn(1.5f);
			base.app.arguments.Clear();
			Activity.RunOnce(delegate
			{
				base.app.scene.LoadMain();
			}, 1.7f);
		}

		public void Load(DRLMap p_target, Action p_callback = null)
		{
			UIView ui = base.app.view.ui;
			float num = 0f;
			if (Mathf.Abs(ui.fade.transition) > 0.1f)
			{
				ui.fade.FadeIn(1.5f, 0f);
				num = 1.5f;
			}
			Texture mapBackground = GetMapBackground(p_target, ui.loader.defaultBackground);
			ui.loader.progress = 0f;
			ui.loader.background = mapBackground;
			ui.loader.tint = (mapBackground ? Color.white : Color.black);
			Debug.Log("DRLSceneManager> Started Loading screen Fade In @" + Time.renderedFrameCount);
			ui.loader.fade.FadeIn(0.5f, num);
			ui.loader.ClearFooter();
			if ((bool)p_target)
			{
				if ((bool)mapBackground)
				{
					GameFlag type = base.app.arguments.game.type;
					string text = p_target.caption.ToUpper();
					bool flag = false;
					bool flag2 = true;
					switch (type)
					{
					case GameFlag.Freestyle:
					case GameFlag.Race:
					case GameFlag.Campaign:
					case GameFlag.Replay:
					case GameFlag.FreeCamera:
					case GameFlag.Sandbox:
					case GameFlag.MapEditor:
					{
						DRLMapTrack dRLMapTrack = base.app.arguments.game.track;
						string text2 = "";
						if ((bool)dRLMapTrack)
						{
							text2 = " <color=red>/</color> " + dRLMapTrack.label;
						}
						if ((bool)p_target && p_target.data != null)
						{
							text2 = " <color=red>/</color> " + p_target.data.mapTitle.ToUpper();
						}
						text += text2;
						flag = base.app.arguments.game.tournamentPromo || base.app.arguments.game.promo;
						if (type == GameFlag.Campaign)
						{
							DRLCampaign campaign = base.app.arguments.game.campaign;
							if ((bool)campaign)
							{
								flag = campaign.tournament;
							}
						}
						break;
					}
					case GameFlag.Mission:
					{
						DRLMission dRLMission = base.app.arguments.game.mission;
						if ((bool)dRLMission)
						{
							text = text + " <color=red>/</color> " + dRLMission.title.ToUpper().Replace("\n", " ");
						}
						break;
					}
					}
					LoaderFooterInfo loaderFooterInfo = LoaderFooterInfo.None;
					if (flag)
					{
						loaderFooterInfo |= LoaderFooterInfo.Promo;
					}
					if (flag2)
					{
						loaderFooterInfo |= LoaderFooterInfo.Hotkeys;
					}
					ui.loader.SetFooter(text, p_target.description, loaderFooterInfo);
					RCI.LockInput(l: false);
				}
				else
				{
					ui.loader.ClearFooter();
				}
			}
			string text3 = (p_target ? p_target.name : "");
			string text4 = (p_target ? ("/" + p_target.guid) : "");
			int num2;
			object obj;
			if (!p_target)
			{
				num2 = 0;
			}
			else
			{
				num2 = (p_target.custom ? 1 : 0);
				if (num2 != 0)
				{
					obj = p_target.data.mapTitle;
					goto IL_0344;
				}
			}
			obj = "";
			goto IL_0344;
			IL_0344:
			string text5 = (string)obj;
			string text6 = ((num2 != 0) ? ("/" + p_target.data.guid) : "");
			Debug.Log("DRLSceneManager> Load / map[" + text3 + text4 + "] custom[" + text5 + text6 + "] previous[" + (map ? map.name : "") + "]");
			num += 0.8f;
			Activity.RunOnce(delegate
			{
				bool flag3 = false;
				DRLMap dRLMap = map;
				bool flag4 = (bool)dRLMap && manager.IsLevelLoaded(dRLMap.scene);
				Debug.Log($"DRLSceneManager> Load Fade Complete / map-prev-loaded[{flag4}] need-force-reload[{flag3}]");
				if (flag3)
				{
					if (!flag4)
					{
						OnMapCleanup(p_target, p_callback);
					}
					else
					{
						manager.UnloadAsync(dRLMap.scene, delegate
						{
							map = null;
							OnMapCleanup(p_target, p_callback);
						});
					}
				}
				else
				{
					OnMapCleanup(p_target, p_callback);
				}
			}, num);
		}

		protected Texture GetMapBackground(DRLMap p_map, Texture p_default = null)
		{
			if (!p_map)
			{
				return p_default;
			}
			if (p_map.data == null)
			{
				return p_map.background;
			}
			DRLMap dRLMap = p_map;
			switch (p_map.data.mapCategoryFlag)
			{
			case GameFlag.MapMultiGP:
				dRLMap = base.app.model.storage.library.FindByGUID<DRLMap>("MP-7ea");
				break;
			case GameFlag.MapSimple:
				dRLMap = base.app.model.storage.library.FindByGUID<DRLMap>("MP-d3f");
				break;
			}
			return dRLMap.background;
		}

		protected void OnMapCleanup(DRLMap p_target, Action p_callback = null)
		{
			DRLMap dRLMap = map;
			map = p_target;
			string text = ((!map) ? "" : (map.custom ? map.data.guid : ""));
			bool flag = base.app.arguments.game != null && base.app.arguments.game.type == GameFlag.MapEditor;
			bool need_dependency = flag;
			if (text != customMap && !string.IsNullOrEmpty(text))
			{
				need_dependency = true;
			}
			need_dependency = true;
			customMap = text;
			string text2 = "";
			bool flag2 = false;
			bool flag3 = (bool)map && manager.IsLevelLoaded(map.scene);
			if (dRLMap != map)
			{
				text2 = (dRLMap ? dRLMap.scene : "");
				flag2 = true;
			}
			if (!dRLMap && flag3)
			{
				text2 = map.scene;
				flag2 = true;
			}
			if (!flag2)
			{
				Debug.Log("DRLSceneManager> map[" + map.scene + "] is cached!");
				OnMapLoadComplete(need_dependency, p_callback);
				return;
			}
			bool flag4 = !string.IsNullOrEmpty(text2);
			Debug.Log("DRLSceneManager> OnMapCleanup / unload[" + text2 + "] need_load[" + flag2 + "] will_unload[" + flag4 + "] need-dependency[" + need_dependency + "]");
			if (flag4 && manager.UnloadAsync(text2, delegate
			{
				LevelManager level = base.app.level;
				for (int i = 0; i < bundles.Count; i++)
				{
					AssetBundle assetBundle = bundles[i];
					if ((bool)assetBundle)
					{
						if ((bool)level)
						{
							break;
						}
						level.AddBundle(assetBundle);
						assetBundle.Unload(unloadAllLoadedObjects: true);
					}
				}
				bundles.Clear();
				OnMapCleanupComplete(need_dependency, p_callback);
			}) == null)
			{
				LoadMain();
			}
			else if (!flag4 && flag2)
			{
				OnMapCleanupComplete(need_dependency, p_callback);
			}
		}

		protected void OnMapCleanupComplete(bool p_load_depencies, Action p_callback = null)
		{
			UIView ui = (base.app ? base.app.view.ui : null);
			AsyncManager loader = AsyncManager.instance;
			string next_scene = (map ? map.scene : "main");
			AsyncOperation resource_unload_op = null;
			int resource_unload_frames = 0;
			Debug.Log($"DRLSceneManager> OnMapCleanupComplete / next-scene[{next_scene}] resource-unload[{resource_unload_op != null}]");
			bool will_load_bundle = false;
			List<string> bpl = new List<string>();
			if ((bool)map && bundles.Count <= 0)
			{
				bpl = DRLPaths.GetBundleFiles("maps/" + next_scene + ".*");
				will_load_bundle = bpl.Count > 0;
			}
			bool is_loading = false;
			float bundle_count = bpl.Count;
			float r = 0f;
			if (resource_unload_op != null)
			{
				GCCollect(0f);
			}
			BundleAsyncRequest req;
			Activity.Run(delegate(float t)
			{
				if (resource_unload_op != null)
				{
					if (!resource_unload_op.isDone)
					{
						return true;
					}
					resource_unload_frames--;
					if (resource_unload_frames > 0)
					{
						return true;
					}
					resource_unload_op = null;
				}
				bool flag = ui != null && ui.loader != null;
				if (is_loading)
				{
					req = (BundleAsyncRequest)loader.Find("scene-bundle");
					if (req.completed)
					{
						is_loading = false;
						AssetBundle assetBundle = req.Get<AssetBundle>();
						base.app.level.AddBundle(assetBundle);
						bundles.Add(assetBundle);
						loader.RemoveAll("scene-bundle");
					}
					return true;
				}
				if (!will_load_bundle)
				{
					r = Mathf.Clamp01(t / 1f);
					if (flag)
					{
						ui.loader.progress = r * 0.3f;
					}
					if (r < 1f)
					{
						return true;
					}
					Activity.RunOnce(delegate
					{
						LoadMapAsync(next_scene, p_load_depencies, p_callback);
					}, 2f);
					return false;
				}
				r = Mathf.Clamp01(1f - (float)bpl.Count / bundle_count);
				if (flag)
				{
					ui.loader.progress = r * 0.3f;
				}
				if (bpl.Count <= 0)
				{
					Activity.RunOnce(delegate
					{
						LoadMapAsync(next_scene, p_load_depencies, p_callback);
					}, 2f);
					return false;
				}
				string text = bpl[0];
				bpl.RemoveAt(0);
				if (File.Exists(text))
				{
					is_loading = true;
					req = (BundleAsyncRequest)loader.ReadBundle("scene-bundle", text, p_active_scenes: true);
					req.persistent = true;
				}
				return true;
			});
		}

		protected void LoadMapAsync(string p_scene, bool p_load_depencies, Action p_callback = null)
		{
			UIView ui = base.app.view.ui;
			manager.LoadLevelAsync(p_scene, delegate(LevelEvent p_event)
			{
				bool flag = ui != null && ui.loader != null;
				float num = (p_load_depencies ? 0.3f : 0.7f);
				switch (p_event.type)
				{
				case LevelEventType.Progress:
					if (flag)
					{
						ui.loader.progress = 0.3f + p_event.progress * num;
					}
					break;
				case LevelEventType.Complete:
					Debug.Log("DRLSceneManager> LoadMapAsync / Complete map[" + map?.ToString() + "] scene[" + p_scene + "] need-dependencies[" + p_load_depencies + "]");
					if ((bool)map)
					{
						OnMapLoadComplete(p_load_depencies, p_callback);
					}
					else
					{
						Clear();
						ui.loader.fade.FadeOut(0.5f, 1f / 30f);
					}
					break;
				}
			});
		}

		protected void OnMapLoadComplete(bool p_load_depencies, Action p_callback)
		{
			Debug.Log("DRLSceneManager> Unloading Previous Game");
			if (manager.IsLevelLoaded("game-bypass"))
			{
				manager.UnloadAsync("game-bypass", delegate
				{
					manager.UnloadAsync("game", delegate
					{
						GCCollect();
						AddGame(p_load_depencies, p_callback);
					});
				});
			}
			else
			{
				manager.UnloadAsync("game", delegate
				{
					GCCollect();
					AddGame(p_load_depencies, p_callback);
				});
			}
		}

		protected void AddGame(bool p_load_depencies, Action p_callback)
		{
			if (base.validContext)
			{
				_ = base.app.view.ui;
			}
			Debug.Log("DRLSceneManager> AddGame / New Scene");
			Action process_game_scene = delegate
			{
				Debug.Log("DRLSceneManager> AddGame.ProcessGameScene / AddLevel Start");
				manager.AddLevel("game");
				Debug.Log("DRLSceneManager> AddGame.ProcessGameScene / AddLevel Complete");
				Debug.Log("DRLSceneManager> AddGame.ProcessGameScene / WaitLevel Start");
				manager.WaitLevel("game", delegate
				{
					Debug.Log("DRLSceneManager> AddGame.ProcessGameScene / WaitLevel Complete");
					if (p_callback != null)
					{
						p_callback();
					}
				});
			};
			Action on_process_game_scene = delegate
			{
				process_game_scene();
			};
			StorageModel storageModel = storage;
			SettingsController stc = settings;
			Debug.Log("DRLSceneManager> AddGame / UnloadLibrary Start - bundle-drone-selection");
			DRLAssetBundleLibrary assetBundleLibrary = storageModel.GetAssetBundleLibrary(StorageAssetBundleLibraryId.DroneSelection);
			assetBundleLibrary.UnloadLibrary();
			Debug.Log("DRLSceneManager> AddGame / UnloadLibrary Complete - bundle-drone-selection");
			DRLApp.IsHighRAMSpec();
			bool num = isMapEditor;
			string text = StorageAssetBundleLibraryId.Static;
			if (0 == 0)
			{
				text = StorageAssetBundleLibraryId.Dynamic;
			}
			if (num)
			{
				text = StorageAssetBundleLibraryId.Dynamic;
			}
			assetBundleLibrary = storageModel.GetAssetBundleLibrary(text);
			Debug.Log("DRLSceneManager> AddGame / Fetched Library Container - id[" + text + "]");
			if (text == StorageAssetBundleLibraryId.Dynamic)
			{
				Debug.Log("DRLSceneManager> AddGame / DynamicLibrary Unload Start");
				assetBundleLibrary.UnloadLibrary();
				Debug.Log("DRLSceneManager> AddGame / DynamicLibrary Unload Complete");
			}
			bool isCustomMap = arguments.game.isCustomMap;
			float load_cap = (isCustomMap ? 0.2f : 0.4f);
			if (p_load_depencies)
			{
				List<string> sceneDependencies = GetSceneDependencies();
				Debug.Log($"DRLSceneManager> AddGame / Library Loading Start - guid-count[{sceneDependencies.Count}]");
				Debug.Log("DRLSceneManager> AddGame / Force lower FPS for stability");
				QualitySettings.vSyncCount = 0;
				Application.targetFrameRate = 50;
				assetBundleLibrary.LoadAssetsAsync(sceneDependencies, p_include_dependency: true, delegate(float p)
				{
					loader.progress = 0.6f + p * load_cap;
					if (p >= 1f)
					{
						Debug.Log("DRLSceneManager> AddGame / Library Loading Complete");
						Debug.Log("DRLSceneManager> AddGame / Restoring FPS after load.");
						if ((bool)stc)
						{
							stc.SetFps();
						}
						on_process_game_scene();
					}
				});
			}
			else
			{
				on_process_game_scene();
			}
		}

		public void Load(DRLMapTrack p_target, Action p_callback = null)
		{
			if (!manager.IsLevelLoaded("game"))
			{
				Debug.LogWarning("DRLSceneManager> Tried to load track outside game scene - track[" + p_target?.ToString() + "]");
				manager.LoadLevelAsync("game");
				return;
			}
			DRLMapTrack dRLMapTrack = track;
			track = p_target;
			string previous_track_scene = (dRLMapTrack ? dRLMapTrack.scene : "");
			Debug.Log("DRLSceneManager> LoadTrack.UnloadAsync Start / track[" + previous_track_scene + "]");
			manager.UnloadAsync(previous_track_scene, delegate
			{
				Debug.Log("DRLSceneManager> LoadTrack.UnloadAsync Complete / track[" + previous_track_scene + "]");
				OnTrackCleanup(p_callback);
			});
		}

		protected void OnTrackCleanup(Action p_callback = null)
		{
			if (!track)
			{
				if (p_callback != null)
				{
					p_callback();
				}
				return;
			}
			Debug.Log("DRLSceneManager> OnTrackCleanup.AddLevelAsync Start / scene[" + track.scene + "]");
			manager.AddLevelAsync(track.scene);
			manager.WaitLevel(track.scene, delegate(bool p_result)
			{
				DRLMapTrack dRLMapTrack = track;
				string text = (dRLMapTrack ? dRLMapTrack.scene : "<null>");
				string text2 = (dRLMapTrack ? dRLMapTrack.title : "<null>");
				string text3 = (dRLMapTrack ? dRLMapTrack.id : "<null>");
				Debug.Log("DRLSceneManager> OnTrackCleanup.WaitLevel Complete / scene[" + text + "]");
				if (!p_result)
				{
					Debug.LogWarning("DRLSceneManager> OnTrackCleanup.WaitLevel / Failed to load track [" + text + "]!");
				}
				else
				{
					GameObject rootGameObject = LevelManager.GetRootGameObject(text, "tracks");
					if (!rootGameObject)
					{
						Debug.LogWarning("DRLSceneManager> OnTrackCleanup.WaitLevel / Failed to find track container!");
					}
					int num = (rootGameObject ? rootGameObject.transform.childCount : 0);
					Debug.Log($"DRLSceneManager> OnTrackCleanup.WaitLevel / track[{text2}] root[{rootGameObject}] scene[{text}] id[{text3}] count[{num}]");
					if (p_callback != null)
					{
						p_callback();
					}
				}
			});
		}

		public void Load(DRLMission p_target, Action p_callback = null)
		{
			if (!manager.IsLevelLoaded("game"))
			{
				Debug.LogWarning("DRLSceneManager> Tried to load mission outside game scene - mission[" + p_target?.ToString() + "]");
				manager.LoadLevelAsync("game");
				return;
			}
			DRLMission dRLMission = mission;
			mission = p_target;
			if ((bool)dRLMission)
			{
				manager.UnloadAsync(dRLMission.scene, delegate
				{
					OnMissionCleanup(p_callback);
				});
			}
			else
			{
				OnMissionCleanup(p_callback);
			}
		}

		protected void OnMissionCleanup(Action p_callback = null)
		{
			if (!mission)
			{
				if (p_callback != null)
				{
					p_callback();
				}
				return;
			}
			manager.AddLevelAsync(mission.scene);
			manager.WaitLevel(mission.scene, delegate(bool p_result)
			{
				if (!p_result)
				{
					Debug.LogWarning("DRLSceneManager> Load - Failed to load mission [" + mission.scene + "]!");
				}
				else if (p_callback != null)
				{
					p_callback();
				}
			});
		}

		public void Load(DRLMap p_map, DRLMapTrack p_track, DRLMission p_mission, Action p_callback = null)
		{
			if (!storage)
			{
				storage = base.app.model.storage;
			}
			if (!settings)
			{
				settings = base.app.controller.settings;
			}
			if (!network)
			{
				network = base.app.model.network;
			}
			if (!loader)
			{
				loader = base.app.view.ui.loader;
			}
			if (!arguments)
			{
				arguments = base.app.arguments;
			}
			isMapEditor = base.app.arguments.game.type == GameFlag.MapEditor;
			m_load_t0 = DateTime.Now.Ticks;
			Notify("scene.game.scenes@start");
			string mp_name = (p_map ? p_map.name : "<null>");
			string mp_guid = (p_map ? ("/" + p_map.guid) : "");
			bool flag = (bool)p_map && p_map.custom;
			string cmp_name = (flag ? p_map.data.mapTitle : "");
			string cmp_guid = (flag ? ("/" + p_map.data.guid) : "");
			string mt_name = (p_track ? p_track.name : "<null>");
			string mt_guid = (p_track ? ("/" + p_track.guid) : "");
			string ms_name = (p_mission ? p_mission.name : "<null>");
			string ms_guid = (p_mission ? p_mission.guid : "");
			Debug.Log("DRLSceneManager> Load Start / map[" + mp_name + mp_guid + "] map-data[" + cmp_name + cmp_guid + "] track[" + mt_name + mt_guid + "] mission[" + ms_name + ms_guid + "]");
			Load(p_map, delegate
			{
				Debug.Log("DRLSceneManager> Load.Map Complete / map[" + mp_name + mp_guid + "] map-data[" + cmp_name + cmp_guid + "] track[" + mt_name + mt_guid + "] mission[" + ms_name + ms_guid + "]");
				Load(p_track, delegate
				{
					Debug.Log("DRLSceneManager> Load.Track Complete / map[" + mp_name + mp_guid + "] map-data[" + cmp_name + cmp_guid + "] track[" + mt_name + mt_guid + "] mission[" + ms_name + ms_guid + "]");
					Load(p_mission, delegate
					{
						Debug.Log("DRLSceneManager> Load.Mission Complete / map[" + mp_name + mp_guid + "] map-data[" + cmp_name + cmp_guid + "] track[" + mt_name + mt_guid + "] mission[" + ms_name + ms_guid + "]");
						if (p_callback != null)
						{
							p_callback();
						}
						Notify(1f / 30f, "scene.game.scenes@complete");
						Activity.RunOnce(LogBundleLibraryStats, 2f);
					});
				});
			});
		}

		public void LogBundleLibraryStats()
		{
			string text = "";
			int num = 0;
			int num2 = 0;
			List<AssetBundle> list = new List<AssetBundle>(AssetBundle.GetAllLoadedAssetBundles());
			list.Sort((AssetBundle a, AssetBundle b) => string.Compare(a.name, b.name));
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				AssetBundle assetBundle = list[num3];
				if (assetBundle.name.Contains("pack-me"))
				{
					num++;
				}
				if (assetBundle.name.Contains("pack-dr"))
				{
					num2++;
				}
			}
			Debug.Log($"DRLSceneManager> All Bundles - Total[{list.Count}] MapEditor[{num}] Drone[{num2}]\n{text}");
			text = "";
			DRLAssetBundleLibrary assetBundleLibrary = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.DroneParts);
			text += $"  {assetBundleLibrary.name.PadRight(23)} - assets[{assetBundleLibrary.assets.Count}] dependencies[{assetBundleLibrary.dependencies.Count}]\n";
			assetBundleLibrary = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.DroneSelection);
			text += $"  {assetBundleLibrary.name.PadRight(23)} - assets[{assetBundleLibrary.assets.Count}] dependencies[{assetBundleLibrary.dependencies.Count}]\n";
			assetBundleLibrary = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.Static);
			text += $"  {assetBundleLibrary.name.PadRight(23)} - assets[{assetBundleLibrary.assets.Count}] dependencies[{assetBundleLibrary.dependencies.Count}]\n";
			assetBundleLibrary = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.Dynamic);
			text += $"  {assetBundleLibrary.name.PadRight(23)} - assets[{assetBundleLibrary.assets.Count}] dependencies[{assetBundleLibrary.dependencies.Count}]\n";
			Debug.Log("DRLSceneManager> Asset Libraries Stats\n" + text);
		}

		public void Load(DRLAppArguments p_args, Action p_callback = null)
		{
			DRLMapTrack dRLMapTrack = p_args.game.track;
			DRLMission dRLMission = p_args.game.mission;
			DRLMap p_map = (dRLMission ? dRLMission.map : (dRLMapTrack ? dRLMapTrack.map : p_args.game.map));
			dRLMapTrack = (dRLMission ? dRLMission.track : dRLMapTrack);
			if (dRLMission == null)
			{
				Notify("analytics.gameplay.loadgame", p_args);
			}
			Load(p_map, dRLMapTrack, dRLMission, p_callback);
		}

		public void Load(Action p_callback)
		{
			Load(base.app.arguments, p_callback);
		}

		public void Load()
		{
			Load(base.app.arguments);
		}

		public void Load(object p_replay, string p_map = "", string p_track = "", string p_custom_map = "")
		{
			string text = "";
			string text2 = "";
			string text3 = "";
			bool flag = true;
			base.app.arguments.game.players = new List<GamePlayerData>();
			if (ReplayFile.EnableVersion2)
			{
				List<ReplayFile> list = ((p_replay is IList<ReplayFile>) ? new List<ReplayFile>((IList<ReplayFile>)p_replay) : new List<ReplayFile> { (ReplayFile)p_replay });
				ReplayFile replayFile = list[0];
				text = replayFile.header.mapGUID;
				text2 = replayFile.header.trackGUID;
				flag = replayFile.header.isCustomMap;
				text3 = replayFile.header.customMapGUID;
				for (int i = 0; i < list.Count; i++)
				{
					base.app.arguments.game.AddReplay(list[i]);
				}
			}
			else
			{
				BlackboxRecord blackboxRecord = (BlackboxRecord)p_replay;
				text = blackboxRecord.GetMapGUID();
				text2 = blackboxRecord.GetTrackGUID();
				flag = blackboxRecord.IsCustomMap();
				text3 = blackboxRecord.GetCustomMapGUID();
				base.app.arguments.game.AddReplay(blackboxRecord);
			}
			string p_guid = (string.IsNullOrEmpty(p_map) ? text : p_map);
			string p_guid2 = (string.IsNullOrEmpty(p_track) ? text2 : p_track);
			int num;
			object obj;
			if (!string.IsNullOrEmpty(p_custom_map))
			{
				num = 1;
			}
			else
			{
				num = (flag ? 1 : 0);
				if (num == 0)
				{
					obj = "";
					goto IL_014d;
				}
			}
			obj = (string.IsNullOrEmpty(p_custom_map) ? text3 : p_custom_map);
			goto IL_014d;
			IL_014d:
			string p_guid3 = (string)obj;
			base.app.arguments.game.type = GameFlag.Replay;
			base.app.arguments.game.mode = GameFlag.SinglePlayer;
			base.app.arguments.game.SetPlayerType(GamePlayerType.Data);
			if (num != 0)
			{
				LoadCommunityMap(p_guid3);
				return;
			}
			DRLMap dRLMap = base.app.model.storage.library.FindByGUID<DRLMap>(p_guid);
			DRLMapTrack dRLMapTrack = base.app.model.storage.library.FindByGUID<DRLMapTrack>(p_guid2);
			if (!dRLMap)
			{
				Debug.LogWarning("DRLSceneManager> Failed to load Map from Replay");
			}
			if (!dRLMapTrack)
			{
				Debug.LogWarning("DRLSceneManager> Failed to load Track from Replay");
			}
			base.app.arguments.game.map = dRLMap;
			base.app.arguments.game.track = dRLMapTrack;
			Load();
		}

		public bool Load(MapData p_data, bool is_offline = false)
		{
			if (p_data == null)
			{
				Debug.LogWarning("DRLSceneManager> Load / Invalid MapData");
				if (!is_offline)
				{
					LoadMain();
				}
				return false;
			}
			string mapId = p_data.mapId;
			DRLMap dRLMap = base.app.model.storage.library.FindByGUID<DRLMap>(mapId);
			DRLMapTrack dRLMapTrack = base.app.model.storage.GetMapTracks(dRLMap, GameFlag.Freestyle)[0];
			if (dRLMap == null || dRLMapTrack == null)
			{
				Debug.LogWarning("DRLSceneManager> Load / No map or track data stored!");
				if (!is_offline)
				{
					LoadMain();
				}
				return false;
			}
			dRLMap.data = p_data;
			base.app.arguments.game.map = dRLMap;
			base.app.arguments.game.track = dRLMapTrack;
			Debug.Log("DRLSceneManager> Load / map[" + dRLMap.scene + "] track[" + dRLMapTrack.scene + "] data[" + p_data.guid + "] mode-type[" + p_data.mode.typeFlag.ToString() + "]");
			base.app.scene.Load(base.app.arguments);
			return true;
		}

		public void LoadCommunityMap(string p_guid, float p_load_delay, Action p_on_complete = null, int p_mapVersion = -1)
		{
			DRLApp.LogMemStats("DRLSceneManager> LoadCommunityMap / Start", p_show_delta: true);
			LoadCommunityMapOffline(p_guid, p_mapVersion, delegate(bool success)
			{
				DRLApp.LogMemStats("DRLSceneManager>   Cache " + (success ? "Success" : "Failure"), p_show_delta: true);
				if (success)
				{
					if (p_on_complete != null)
					{
						p_on_complete();
					}
				}
				else
				{
					base.app.model.service.GetCommunityMap(p_guid, delegate(DRLCommunityMapResult p_result)
					{
						DRLCommunityMapData d = ((p_result.data.Length == 0) ? null : p_result.data[0]);
						if (d == null)
						{
							Debug.LogWarning("DRLSceneManager> LoadCommunityMap / Failed to Load DRLCommunityMapData - guid[" + p_guid + "]");
							LoadMain(p_force: true);
						}
						else
						{
							new Thread((ThreadStart)delegate
							{
								MapData md = d.Convert<MapData>();
								if (md != null)
								{
									md.LoadRoot(d.root);
								}
								Activity.RunOnce(delegate
								{
									if (md == null)
									{
										Debug.LogWarning("DRLSceneManager> LoadCommunityMap / Failed to Parse MapData - guid[" + p_guid + "]");
										LoadMain(p_force: true);
									}
									else
									{
										if (p_on_complete != null)
										{
											Activity.RunOnce(p_on_complete, 0.5f);
										}
										Activity.RunOnce(delegate
										{
											Load(md);
										}, p_load_delay + 0.5f);
									}
								}, 1f / 60f);
							}).Start();
						}
					});
				}
			});
		}

		public void LoadCommunityMap(string p_guid, Action p_on_complete = null)
		{
			LoadCommunityMap(p_guid, 0f, p_on_complete);
		}

		public void LoadCommunityMapOffline(string p_guid, int p_mapVersion, Action<bool> p_on_complete)
		{
			base.app.model.storage.maps.LoadFromCache(p_guid, delegate(MapData md)
			{
				if (md == null)
				{
					Debug.LogWarning("DRLSceneManager> LoadCommunityMap / Failed to Parse MapData - guid[" + p_guid + "]");
					if (p_on_complete != null)
					{
						p_on_complete(obj: false);
						return;
					}
				}
				if ((!DRLApp.offline && p_mapVersion > md.version) || md.root == null)
				{
					p_on_complete?.Invoke(obj: false);
				}
				else if (p_on_complete != null)
				{
					p_on_complete(Load(md, is_offline: true));
				}
			});
		}

		public void UnloadMapTrack(Action p_callback = null)
		{
			bool num = map != null && !string.IsNullOrEmpty(map.scene);
			bool has_track = track != null && !string.IsNullOrEmpty(track.scene);
			if (!num)
			{
				p_callback?.Invoke();
			}
			manager.UnloadAsync(map.scene, delegate
			{
				if (!has_track)
				{
					p_callback?.Invoke();
				}
				else
				{
					manager.UnloadAsync(track.scene, delegate
					{
						p_callback?.Invoke();
					});
				}
			});
		}

		public List<string> GetSceneDependencies()
		{
			bool num = isMapEditor;
			List<string> list = new List<string>();
			StorageModel storageModel = storage;
			DRLAppArguments dRLAppArguments = arguments;
			DRLAssetBundleLibrary assetBundleLibrary = storageModel.GetAssetBundleLibrary(StorageAssetBundleLibraryId.Dynamic);
			if (num)
			{
				list.AddRange(assetBundleLibrary.GetGUIDs());
				list.RemoveAll((string it) => !it.Contains("DMA"));
			}
			else
			{
				MapData mapData = (map ? map.data : null);
				if (mapData != null)
				{
					List<string> dependencies = mapData.root.dependencies;
					list.AddRange(dependencies);
				}
				bool useLibraryLow = DRLPaths.Content.useLibraryLow;
				bool flag = dRLAppArguments.game.mode == GameFlag.NetworkMultiplayer;
				bool flag2 = dRLAppArguments.game.type == GameFlag.FreeCamera;
				if (useLibraryLow)
				{
					DroneRigData droneRigData = null;
					List<string> list2 = new List<string>();
					if (flag && flag2)
					{
						list2.Clear();
						List<string> list3 = new List<string>(assetBundleLibrary.GetGUIDs());
						list3.RemoveAll((string it) => it.Contains("DMA"));
						list2.AddRange(list3);
					}
					else
					{
						List<GamePlayerData> players = dRLAppArguments.game.players;
						Debug.Log($"DRLSceneManager> GetSceneDependencies / Collecting Players Dependencies - count[{players.Count}]");
						for (int num2 = 0; num2 < players.Count; num2++)
						{
							GamePlayerData p_player = players[num2];
							droneRigData = GetRigData(p_player);
							if (droneRigData != null)
							{
								list2.AddRange(droneRigData.dependencies);
							}
						}
						if (dRLAppArguments.game.type == GameFlag.Mission)
						{
							droneRigData = storageModel.state.player.garage.GetTemplateByGUID("DRD-1fd4ef78be03");
							if (droneRigData != null)
							{
								list2.AddRange(droneRigData.dependencies);
							}
						}
						if (flag)
						{
							NetworkRoom networkRoom = (network ? network.room : null);
							if (networkRoom == null)
							{
								Debug.LogWarning("DRLSceneManager> GetSceneDependencies / Multiplayer Mode but room is null");
							}
							if (networkRoom != null)
							{
								List<NetworkActor> racers = networkRoom.Racers;
								Debug.Log($"DRLSceneManager> GetSceneDependencies / Collecting Network Players Dependencies - count[{racers.Count}]");
								for (int num3 = 0; num3 < racers.Count; num3++)
								{
									NetworkActor networkActor = racers[num3];
									if (!networkActor.IsLocal)
									{
										droneRigData = (string.IsNullOrEmpty(networkActor.DroneRigData) ? null : DroneRigData.FromJson(networkActor.DroneRigData));
										if (droneRigData != null)
										{
											list2.AddRange(droneRigData.dependencies);
										}
									}
								}
							}
						}
						list2.AddRange(assetBundleLibrary.GetGUIDs().FindAll((string it) => it.Contains("PD")));
					}
					list.AddRange(list2);
				}
				for (int num4 = 0; num4 < list.Count; num4++)
				{
					if (string.IsNullOrEmpty(list[num4]))
					{
						list.RemoveAt(num4--);
					}
				}
				for (int num5 = 0; num5 < list.Count; num5++)
				{
					for (int num6 = num5 + 1; num6 < list.Count; num6++)
					{
						if (list[num5] == list[num6])
						{
							list.RemoveAt(num6--);
						}
					}
				}
			}
			return list;
		}

		protected DroneRigData GetRigData(GamePlayerData p_player)
		{
			if (p_player == null)
			{
				return null;
			}
			StorageModel storageModel = storage;
			switch (p_player.type)
			{
			case GamePlayerType.Human:
				return storageModel.state.player.garage.currentRigData;
			case GamePlayerType.Ghost:
			case GamePlayerType.Data:
				if (ReplayFile.EnableVersion2)
				{
					return p_player.replayV2.header.GetDroneRig();
				}
				return DroneRigData.FromJson(p_player.replay.header.Get("drone-rig", ""));
			case GamePlayerType.Network:
				return DroneRigData.FromJson(p_player.droneRigData);
			default:
				return null;
			}
		}

		public void Clear()
		{
			ClearBundles();
			map = null;
			mission = null;
			track = null;
		}

		public void ClearBundles()
		{
			if (!base.validContext)
			{
				return;
			}
			if (bundles == null)
			{
				bundles = new List<AssetBundle>();
			}
			for (int i = 0; i < bundles.Count; i++)
			{
				AssetBundle assetBundle = bundles[i];
				if ((bool)assetBundle)
				{
					base.app.level.RemoveBundle(assetBundle);
					assetBundle.Unload(unloadAllLoadedObjects: true);
					UnityEngine.Object.Destroy(assetBundle);
				}
			}
			bundles.Clear();
		}

		private void GCCollect(float delay = 4f)
		{
			Debug.Log("DRLSceneManager> GCCollect");
			Activity.RunOnce(delegate
			{
				long totalMemory = GC.GetTotalMemory(forceFullCollection: true);
				memorySamples.Add(totalMemory);
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				string text = "DRLSceneManager> Running preemptive GC";
				long totalMemory2 = GC.GetTotalMemory(forceFullCollection: true);
				long num = ((memorySamples.Count <= 1) ? 0 : (memorySamples[memorySamples.Count - 1] - memorySamples[0]));
				text = text + "Before: " + totalMemory / 1024 + "kb\nAfter: " + totalMemory2 / 1024 + "kb\nDelta: " + (totalMemory2 - totalMemory) / 1024 + "kb\nOverall: " + num / 1024 + "kb\nSamples:\n" + string.Join("\n", memorySamples.ConvertAll((long a) => a.ToString()).ToArray());
			}, delay);
		}
	}
}
