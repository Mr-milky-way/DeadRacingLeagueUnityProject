using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class StorageModel : Model<DRLApp>
	{
		public AsyncManager loader;

		public AssetLibrary library;

		public AssetLibrary libraryMain;

		public AssetLibrary libraryDependencies;

		public List<AssetBundle> dependencyBundles;

		private Dictionary<string, string> m_dependencies_lut;

		public AssetLibrary demoLibrary;

		public AssetLibrary fullLibrary;

		public bool saveComplete;

		private bool m_has_init;

		public Dictionary<string, string> dependenciesLUT
		{
			get
			{
				if (m_dependencies_lut != null)
				{
					return m_dependencies_lut;
				}
				return m_dependencies_lut = new Dictionary<string, string>();
			}
			set
			{
				m_dependencies_lut = value;
			}
		}

		public int dependenciesLUTCount => dependenciesLUT.Count;

		public DroneFactory factory => AssertLocal<DroneFactory>("factory");

		public StateModel state => AssertFind<StateModel>("state");

		public Localization locale => AssertFind<Localization>("locale");

		public string systemLocale => DRLApp.systemLocale;

		public MapsStorageModel maps => AssertFind<MapsStorageModel>("maps");

		public ReplaysStorageModel replays => AssertFind<ReplaysStorageModel>("replays");

		public LeaderboardsStorageModel leaderboards => AssertFind<LeaderboardsStorageModel>("leaderboards");

		public DRLAssetBundleLibrary GetAssetBundleLibrary(string p_id)
		{
			int childCount = library.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				GameObject gameObject = library.transform.GetChild(i).gameObject;
				if (!(gameObject.name != p_id))
				{
					DRLAssetBundleLibrary component = gameObject.GetComponent<DRLAssetBundleLibrary>();
					if ((bool)component)
					{
						return component;
					}
				}
			}
			return null;
		}

		public List<DRLAssetBundleLibrary> GetAllAssetBundleLibrary()
		{
			List<DRLAssetBundleLibrary> list = new List<DRLAssetBundleLibrary>();
			int childCount = library.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				DRLAssetBundleLibrary component = library.transform.GetChild(i).gameObject.GetComponent<DRLAssetBundleLibrary>();
				if ((bool)component)
				{
					list.Add(component);
				}
			}
			return list;
		}

		protected void Awake()
		{
			if (!m_has_init)
			{
				m_has_init = true;
				if (!loader)
				{
					loader = AsyncManager.instance;
				}
				dependencyBundles = new List<AssetBundle>();
			}
		}

		public bool HasOfflineData()
		{
			bool num = Directory.Exists(DRLPaths.Storage.offlinePlayerStateRoot);
			bool flag = File.Exists(DRLPaths.Storage.offlinePlayerStateFile);
			bool flag2 = Directory.Exists(DRLPaths.Storage.offlineMapsRoot);
			bool flag3 = File.Exists(DRLPaths.Storage.offlineMapsHash);
			return num && flag && flag2 && flag3;
		}

		public bool HasCommunityCache()
		{
			return File.Exists(DRLPaths.Storage.offlineMapsCustomHash);
		}

		public void SetLibraryByLicense(bool p_full)
		{
			AssetLibrary assetLibrary = (p_full ? demoLibrary : fullLibrary);
			AssetLibrary assetLibrary2 = (p_full ? fullLibrary : demoLibrary);
			if (!libraryMain.assets.Contains(assetLibrary2.gameObject))
			{
				library.Add(assetLibrary2);
			}
			if (libraryMain.assets.Contains(assetLibrary.gameObject))
			{
				library.Remove(assetLibrary);
			}
		}

		public void OnPersistency()
		{
			base.app.model.storage = this;
		}

		public void Add(GameObject p_asset, bool p_dependency)
		{
			(p_dependency ? libraryDependencies : libraryMain).Add(p_asset);
		}

		public void Add(IList p_assets, bool p_dependency)
		{
			AssetLibrary assetLibrary = (p_dependency ? libraryDependencies : libraryMain);
			for (int i = 0; i < p_assets.Count; i++)
			{
				object obj = p_assets[i];
				GameObject gameObject = ((obj is GameObject) ? ((GameObject)obj) : ((obj is Component) ? ((Component)obj).gameObject : null));
				if ((bool)gameObject)
				{
					assetLibrary.Add(gameObject);
				}
			}
		}

		public List<string> GetDependencies(List<string> p_keys)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < p_keys.Count; i++)
			{
				string key = p_keys[i];
				if (dependenciesLUT.ContainsKey(key))
				{
					string item = dependenciesLUT[key];
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			return list;
		}

		protected void PruneDependencies(bool p_all)
		{
			libraryDependencies.assets.Clear();
			for (int i = 0; i < dependencyBundles.Count; i++)
			{
				AssetBundle assetBundle = dependencyBundles[i];
				if ((bool)assetBundle)
				{
					assetBundle.Unload(p_all);
					if (p_all)
					{
						UnityEngine.Object.Destroy(assetBundle);
					}
				}
			}
		}

		public void ClearDependencies()
		{
			PruneDependencies(p_all: true);
			dependencyBundles.Clear();
		}

		public void PruneDependencies()
		{
			PruneDependencies(p_all: false);
		}

		public void PreloadBundleData(string p_library_id, Func<AssetBundlePreloadState, float, object> p_on_status)
		{
			List<string> guids = new List<string>();
			if (p_on_status != null)
			{
				object obj = p_on_status(AssetBundlePreloadState.Init, 0f);
				if (obj != null && obj is IList<string>)
				{
					guids.AddRange((IList<string>)obj);
				}
			}
			DRLAssetBundleLibrary lib = base.app.model.storage.GetAssetBundleLibrary(p_library_id);
			Activity.RunOnce(delegate
			{
				if (p_on_status != null)
				{
					p_on_status(AssetBundlePreloadState.Start, 0f);
				}
				lib.LoadAssetsAsync(guids, p_include_dependency: true, delegate(float p)
				{
					if (p_on_status != null)
					{
						p_on_status(AssetBundlePreloadState.Progress, p);
					}
					if (!(p < 1f))
					{
						if (p_on_status != null)
						{
							p_on_status(AssetBundlePreloadState.Progress, 1f);
						}
						if (p_on_status != null)
						{
							p_on_status(AssetBundlePreloadState.Complete, 1f);
						}
					}
				});
			}, 1.5f);
		}

		public void PreloadDroneBundleData(UICardButtonDroneRig p_button, DroneRigData p_data, bool p_ingame, Action p_on_complete)
		{
			if (!DRLPaths.Content.useLibraryLow)
			{
				if (p_on_complete != null)
				{
					p_on_complete();
				}
				return;
			}
			StorageModel storage = base.app.model.storage;
			bool is_dynamic = p_data == null || p_ingame;
			bool is_all = p_data == null;
			string p_library_id = (is_dynamic ? StorageAssetBundleLibraryId.Dynamic : StorageAssetBundleLibraryId.DroneSelection);
			DRLAssetBundleLibrary dsl = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.DroneSelection);
			List<string> parts_guids = (is_all ? dsl.GetGUIDs() : p_data.dependencies);
			if (p_data == null)
			{
				List<string> list = new List<string>();
				DRLAssetBundleLibrary assetBundleLibrary = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.Dynamic);
				list.AddRange(assetBundleLibrary.assetGUIDs);
				assetBundleLibrary = base.app.model.storage.GetAssetBundleLibrary(StorageAssetBundleLibraryId.DroneSelection);
				list.AddRange(assetBundleLibrary.assetGUIDs);
				bool flag = true;
				List<string> list2 = new List<string>();
				for (int i = 0; i < parts_guids.Count; i++)
				{
					string text = parts_guids[i];
					if (!text.Contains("PD-") && !text.Contains(".") && !list.Contains(text))
					{
						list2.Add(text);
						flag = false;
					}
				}
				if (flag)
				{
					if (p_on_complete != null)
					{
						p_on_complete();
					}
					return;
				}
			}
			storage.PreloadBundleData(p_library_id, delegate(AssetBundlePreloadState p_state, float p)
			{
				object result = null;
				switch (p_state)
				{
				case AssetBundlePreloadState.Init:
					result = parts_guids;
					if (is_all)
					{
						string p_caption = locale.Get("garage.loader.caption", "Workbench").ToUpper();
						string p_description = locale.Get("garage.loader.description", "Build your dream drone!").ToUpper();
						base.app.view.ui.fade.FadeIn(0.6f);
						base.app.view.ui.loader.ClearFooter();
						base.app.view.ui.loader.SetFooter(p_caption, p_description, LoaderFooterInfo.Workbench);
						base.app.view.ui.loader.background = state.player.garage.garageLoadBackground;
						base.app.view.ui.loader.fade.FadeIn(0.6f, 0.6f);
						base.app.view.ui.loader.progress = 0f;
					}
					else if ((bool)p_button)
					{
						p_button.status.SetLoading(0f);
						p_button.status.fade.FadeIn(0.2f);
					}
					break;
				case AssetBundlePreloadState.Start:
					if (!is_dynamic)
					{
						dsl.UnloadLibrary();
					}
					if (is_all)
					{
						base.app.view.ui.loader.progress = 0.05f;
					}
					break;
				case AssetBundlePreloadState.Progress:
					if (is_all)
					{
						base.app.view.ui.loader.progress = Mathf.Clamp01(p + 0.05f);
					}
					else if ((bool)p_button)
					{
						p_button.status.SetLoading(p);
					}
					break;
				case AssetBundlePreloadState.Complete:
					dsl.UnloadDependencies(p_all: false);
					Activity.RunOnce(delegate
					{
						base.app.view.ui.loader.progress = 0f;
						if (p_on_complete != null)
						{
							p_on_complete();
						}
					}, 1.3f);
					if (is_all)
					{
						base.app.view.ui.loader.fade.FadeOut(0.2f, 3f);
						base.app.view.ui.fade.FadeOut(0.2f, 3f);
					}
					else if ((bool)p_button)
					{
						p_button.status.fade.FadeOut(0.2f, 1f);
					}
					break;
				}
				return result;
			});
		}

		public List<T> Filter<T>(List<T> p_list, bool p_precise, params GameFlag[] p_contains) where T : DRLGameAsset
		{
			List<T> list = new List<T>();
			for (int i = 0; i < p_list.Count; i++)
			{
				T val = p_list[i];
				bool flag = true;
				if ((bool)val.tags && !val.tags.Match(p_precise, p_contains))
				{
					flag = false;
				}
				if (flag)
				{
					list.Add(val);
				}
			}
			return list;
		}

		public List<T> Filter<T>(List<T> p_list, params GameFlag[] p_contains) where T : DRLGameAsset
		{
			return Filter(p_list, p_precise: false, p_contains);
		}

		public List<DRLMap> GetAllMaps()
		{
			return library.FindAll<DRLMap>();
		}

		public List<DRLMap> GetMaps(bool p_allow_empty, params GameFlag[] p_contains)
		{
			List<DRLMap> list = library.FindAll<DRLMap>();
			FilterByRelease(list);
			for (int i = 0; i < list.Count; i++)
			{
				DRLMap p_map = list[i];
				List<DRLMapTrack> mapTracks = GetMapTracks(p_map);
				FilterByRelease(mapTracks);
				for (int j = 0; j < mapTracks.Count; j++)
				{
					GameFlagTag component = mapTracks[j].GetComponent<GameFlagTag>();
					if ((bool)component && p_contains.Length != 0 && !component.Match(p_contains))
					{
						mapTracks.RemoveAt(j--);
					}
				}
				if (!p_allow_empty && mapTracks.Count <= 0)
				{
					list.RemoveAt(i--);
				}
			}
			list.Sort((DRLMap a, DRLMap b) => (a.order >= b.order) ? 1 : (-1));
			return list;
		}

		public List<DRLMap> GetMaps(params GameFlag[] p_contains)
		{
			return GetMaps(p_allow_empty: false, p_contains);
		}

		public List<DRLMap> GetRaceMaps()
		{
			return GetMaps(GameFlag.Race);
		}

		public List<DRLMap> GetSDMaps()
		{
			return GetMaps(GameFlag.Collectable);
		}

		public DRLMap GetMapByGUID(string p_mapGUID)
		{
			return library.FindByGUID<DRLMap>(p_mapGUID);
		}

		public List<string> GetRaceMapNames()
		{
			List<DRLMap> raceMaps = GetRaceMaps();
			List<string> list = new List<string>();
			for (int i = 0; i < raceMaps.Count; i++)
			{
				list.Add(raceMaps[i].title);
			}
			return list;
		}

		public List<string> GetMapNames(params GameFlag[] p_contains)
		{
			List<DRLMap> list = GetMaps(p_contains);
			List<string> list2 = new List<string>();
			for (int i = 0; i < list.Count; i++)
			{
				list2.Add(list[i].title);
			}
			return list2;
		}

		public List<DRLMapTrack> GetMapTracks()
		{
			List<DRLMapTrack> list = library.FindAll<DRLMapTrack>();
			list.Sort((DRLMapTrack a, DRLMapTrack b) => (a.order >= b.order) ? 1 : (-1));
			return list;
		}

		public DRLMapTrack GetMapTrack(string p_map_id, string p_track_id, bool p_freestyle)
		{
			return GetMapTracks(p_map_id).Find(delegate(DRLMapTrack it)
			{
				bool num = (p_freestyle ? (it.id == "freefly") : it.id.Contains("race"));
				bool flag = p_track_id == it.guid;
				return num && flag;
			});
		}

		public List<DRLMapTrack> GetMapTracks(DRLMap p_map, GameFlag p_game_type, bool p_filter_build)
		{
			List<DRLMapTrack> mapTracks = GetMapTracks();
			GameFlag gameFlag = p_game_type;
			switch (p_game_type)
			{
			case GameFlag.FreeCamera:
				gameFlag = GameFlag.None;
				break;
			case GameFlag.MapEditor:
				gameFlag = GameFlag.Freestyle;
				break;
			}
			for (int i = 0; i < mapTracks.Count; i++)
			{
				DRLMapTrack dRLMapTrack = mapTracks[i];
				bool flag = !p_map || ((bool)dRLMapTrack.map && dRLMapTrack.map.guid == p_map.guid);
				if (gameFlag != GameFlag.None)
				{
					GameFlagTag component = dRLMapTrack.GetComponent<GameFlagTag>();
					if ((bool)component && !component.Contains(gameFlag))
					{
						flag = false;
					}
				}
				if (!flag)
				{
					mapTracks.RemoveAt(i--);
				}
			}
			if (p_filter_build)
			{
				for (int j = 0; j < mapTracks.Count; j++)
				{
					GameFlagTag component2 = mapTracks[j].GetComponent<GameFlagTag>();
					if ((bool)component2 && component2.Match(GameFlag.Development))
					{
						mapTracks.RemoveAt(j--);
					}
				}
			}
			return mapTracks;
		}

		public List<DRLMapTrack> GetMapTracks(DRLMap p_map, GameFlag p_game_type)
		{
			return GetMapTracks(p_map, p_game_type, p_filter_build: false);
		}

		public List<DRLMapTrack> GetMapTracks(DRLMap p_map)
		{
			return GetMapTracks(p_map, GameFlag.None);
		}

		public List<DRLMapTrack> GetMapTracks(string p_map_id, GameFlag p_game_type)
		{
			DRLMap dRLMap = library.FindByGUID<DRLMap>(p_map_id);
			if (!dRLMap)
			{
				return new List<DRLMapTrack>();
			}
			return GetMapTracks(dRLMap, p_game_type);
		}

		public List<DRLMapTrack> GetMapTracks(string p_map_id)
		{
			return GetMapTracks(p_map_id, GameFlag.None);
		}

		public List<string> GetMapTrackNames(DRLMap p_map, GameFlag p_game_type)
		{
			List<DRLMapTrack> mapTracks = GetMapTracks(p_map, p_game_type);
			List<string> list = new List<string>();
			for (int i = 0; i < mapTracks.Count; i++)
			{
				list.Add(mapTracks[i].title);
			}
			return list;
		}

		public List<string> GetMapTrackNames(DRLMap p_map)
		{
			return GetMapTrackNames(p_map, GameFlag.None);
		}

		public List<string> GetMapTrackNames()
		{
			return GetMapTrackNames(null, GameFlag.None);
		}

		public List<string> GetCampaignNames()
		{
			List<DRLCampaign> campaigns = GetCampaigns();
			List<string> list = new List<string>();
			for (int i = 0; i < campaigns.Count; i++)
			{
				list.Add(campaigns[i].title);
			}
			return list;
		}

		public List<DRLQuest> GetAllQuests()
		{
			return library.FindAll<DRLQuest>();
		}

		public List<DRLQuest> GetQuests()
		{
			List<DRLQuest> allQuests = GetAllQuests();
			FilterByRelease(allQuests);
			return allQuests;
		}

		public List<DRLQuest> GetQuests(GameFlag p_tag)
		{
			List<DRLQuest> allQuests = GetAllQuests();
			FilterByTag(allQuests, p_tag);
			FilterByRelease(allQuests);
			return allQuests;
		}

		public DRLQuest GetQuest(string p_guid)
		{
			List<DRLQuest> quests = GetQuests();
			for (int i = 0; i < quests.Count; i++)
			{
				if ((bool)quests[i] && quests[i].guid == p_guid)
				{
					return quests[i];
				}
			}
			return null;
		}

		public List<DronePhysicsSettings> GetAllPhysicsSettings(string p_frame_guid)
		{
			List<DronePhysicsSettings> list = new List<DronePhysicsSettings>();
			List<DronePhysicsSettings> list2 = library.FindAll<DronePhysicsSettings>();
			FilterByRelease(list2);
			for (int i = 0; i < list2.Count; i++)
			{
				DronePhysicsSettings dronePhysicsSettings = list2[i];
				if (dronePhysicsSettings.HasSupport(p_frame_guid))
				{
					list.Add(dronePhysicsSettings);
				}
			}
			return list;
		}

		public List<DronePhysicsSettings> GetAllPhysicsSettings(DroneFrame p_frame)
		{
			if (!p_frame)
			{
				return new List<DronePhysicsSettings>();
			}
			return GetAllPhysicsSettings(p_frame.guid);
		}

		public DronePhysicsSettings GetPhysicsSettings(DroneFrame p_frame)
		{
			List<DronePhysicsSettings> allPhysicsSettings = GetAllPhysicsSettings(p_frame);
			if (allPhysicsSettings.Count > 0)
			{
				return allPhysicsSettings[0];
			}
			return null;
		}

		public DronePhysicsSettings GetPhysicsSettings(string p_frame_guid)
		{
			List<DronePhysicsSettings> allPhysicsSettings = GetAllPhysicsSettings(p_frame_guid);
			if (allPhysicsSettings.Count > 0)
			{
				return allPhysicsSettings[0];
			}
			return null;
		}

		public List<DRLDroneRig> GetDrones()
		{
			List<DRLDroneRig> list = library.FindAll<DRLDroneRig>();
			FilterByRelease(list);
			list.Sort((DRLDroneRig a, DRLDroneRig b) => (a.order >= b.order) ? 1 : (-1));
			return list;
		}

		public List<string> GetDroneNames()
		{
			List<DRLDroneRig> drones = GetDrones();
			List<string> list = new List<string>();
			for (int i = 0; i < drones.Count; i++)
			{
				list.Add(drones[i].title);
			}
			return list;
		}

		public List<DRLOnboarding> GetAllOnboardingCampaigns()
		{
			return library.FindAll<DRLOnboarding>();
		}

		public DRLOnboarding GetOnboardingCampaigns(OnboardingCampaignMode p_tag)
		{
			List<DRLOnboarding> list = library.FindAll<DRLOnboarding>();
			List<DRLOnboarding> list2 = fullLibrary.FindAll<DRLOnboarding>();
			if (list2 != null && list2.Count > 0)
			{
				if (list == null)
				{
					list = new List<DRLOnboarding>(list2);
				}
				else
				{
					list.AddRange(list2);
				}
			}
			if (list == null || list.Count == 0)
			{
				return null;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].mode == p_tag)
				{
					return list[i];
				}
			}
			return null;
		}

		public List<DRLCampaign> GetCampaigns()
		{
			List<DRLCampaign> list = library.FindAll<DRLCampaign>();
			FilterByRelease(list);
			list.Sort((DRLCampaign a, DRLCampaign b) => (a.order >= b.order) ? 1 : (-1));
			return list;
		}

		public int GetReplayFilesInfo(out long p_size)
		{
			string replaysRoot = DRLPaths.Storage.replaysRoot;
			p_size = 0L;
			List<string> list = new List<string>();
			list.AddRange(Directory.GetFiles(replaysRoot, "*.json"));
			list.AddRange(Directory.GetFiles(replaysRoot, "*.bytes"));
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				string path = list[i];
				if (File.Exists(path))
				{
					FileStream fileStream = File.OpenRead(path);
					p_size += fileStream.Length;
					fileStream.Close();
					num++;
				}
			}
			return num;
		}

		public void FilterByRelease(IList p_list)
		{
			for (int i = 0; i < p_list.Count; i++)
			{
				object obj = p_list[i];
				if (obj == null)
				{
					continue;
				}
				Component component = null;
				if (obj is Component)
				{
					component = obj as Component;
				}
				if (obj is GameObject)
				{
					component = (obj as GameObject).transform;
				}
				if ((bool)component)
				{
					GameFlagTag component2 = component.GetComponent<GameFlagTag>();
					if ((bool)component2 && component2.Match(GameFlag.Development))
					{
						p_list.RemoveAt(i--);
					}
				}
			}
		}

		public void FilterByTag(IList p_list, GameFlag p_tag)
		{
			for (int i = 0; i < p_list.Count; i++)
			{
				object obj = p_list[i];
				if (obj == null)
				{
					continue;
				}
				Component component = null;
				if (obj is Component)
				{
					component = obj as Component;
				}
				if (obj is GameObject)
				{
					component = (obj as GameObject).transform;
				}
				if ((bool)component)
				{
					GameFlagTag component2 = component.GetComponent<GameFlagTag>();
					if ((bool)component2 && !component2.Match(p_tag))
					{
						p_list.RemoveAt(i--);
					}
				}
			}
		}

		public void LoadImageLocally(string p_url, int p_width, int p_height, Action<Texture> p_callback)
		{
			if (!File.Exists(p_url))
			{
				p_callback?.Invoke(null);
				return;
			}
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, 1, linear: false);
			texture2D.LoadImage(File.ReadAllBytes(p_url));
			p_callback?.Invoke(texture2D);
			Debug.Log("StorageModel> Loaded [" + p_url + "] picture locally!");
		}

		private IEnumerator LoadImage(string p_url, int p_width, int p_height, Action<Texture> p_callback)
		{
			using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(p_url);
			yield return uwr.SendWebRequest();
			if (uwr.result != UnityWebRequest.Result.Success)
			{
				Debug.Log("StorageModel>Load image: failed to load image - " + uwr.error + " [" + p_url + "]");
			}
			else
			{
				p_callback?.Invoke(DownloadHandlerTexture.GetContent(uwr));
			}
		}

		public void LoadPlayerState(Action<bool> p_callback)
		{
			if (File.Exists(DRLPaths.Storage.offlinePlayerStateFile))
			{
				try
				{
					string text = File.ReadAllText(DRLPaths.Storage.offlinePlayerStateFile);
					if (string.IsNullOrEmpty(text) || text.Trim() == "{}")
					{
						p_callback?.Invoke(obj: false);
						return;
					}
					Dictionary<string, object> data = Serialize.FromJson<Dictionary<string, object>>(text);
					state.player.data.data = data;
					p_callback?.Invoke(obj: true);
				}
				catch
				{
					p_callback?.Invoke(obj: false);
				}
			}
			else
			{
				p_callback?.Invoke(obj: false);
			}
			LoadImageLocally(DRLPaths.Storage.offlinePlayerStatePicture, 512, 512, delegate(Texture tex)
			{
				if (tex != null)
				{
					state.player.profile.photo = tex;
				}
			});
		}

		public bool ValidateLocalPlayerId(string p_playerId)
		{
			if (string.IsNullOrEmpty(p_playerId))
			{
				return false;
			}
			if (!File.Exists(DRLPaths.Storage.offlinePlayerStateFile))
			{
				return false;
			}
			try
			{
				string text = File.ReadAllText(DRLPaths.Storage.offlinePlayerStateFile);
				if (string.IsNullOrEmpty(text) || text.Trim() == "{}")
				{
					return false;
				}
				Dictionary<string, object> dictionary = Serialize.FromJson<Dictionary<string, object>>(text);
				if (!dictionary.ContainsKey("player-id"))
				{
					return false;
				}
				return (string)dictionary["player-id"] == p_playerId;
			}
			catch
			{
				return false;
			}
		}
	}
}
