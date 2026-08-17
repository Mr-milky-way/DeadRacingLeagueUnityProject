using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace thelab.core
{
	public class LevelManager : MonoBehaviour
	{
		private static List<AssetBundle> _bundles;

		private List<string> m_level_names;

		[SerializeField]
		internal List<string> m_builtLevelNames;

		[SerializeField]
		private LevelManagerCallback m_OnEvent;

		private Dictionary<string, AsyncOperation> _loaders;

		private Dictionary<string, Action<LevelEvent>> _callbacks;

		internal List<string> m_gc;

		private static List<AssetBundle> m_bundles => Reflection<object>.Assert(ref _bundles);

		public Scene level => SceneManager.GetActiveScene();

		public List<Scene> levels
		{
			get
			{
				List<Scene> list = new List<Scene>();
				for (int i = 0; i < count; i++)
				{
					list.Add(SceneManager.GetSceneAt(i));
				}
				return list;
			}
		}

		public int count => SceneManager.sceneCount;

		public int levelBuildId => level.buildIndex;

		public string levelName => level.name;

		public List<string> levelNames
		{
			get
			{
				if (m_level_names == null)
				{
					m_level_names = new List<string>();
				}
				m_level_names.Clear();
				List<Scene> list = levels;
				for (int i = 0; i < list.Count; i++)
				{
					m_level_names.Add(list[i].name);
				}
				return m_level_names;
			}
		}

		public List<string> builtLevelNames
		{
			get
			{
				if (m_builtLevelNames == null)
				{
					m_builtLevelNames = new List<string>();
				}
				if (Application.isEditor)
				{
					List<string> buildSettingsScenes = GetBuildSettingsScenes();
					if (buildSettingsScenes != null)
					{
						m_builtLevelNames = buildSettingsScenes;
					}
				}
				for (int i = 0; i < m_bundles.Count; i++)
				{
					string[] array = (m_bundles[i] ? m_bundles[i].GetAllScenePaths() : new string[0]);
					for (int j = 0; j < array.Length; j++)
					{
						string text = array[j];
						int num = Mathf.Max(0, text.LastIndexOf("/"));
						int num2 = text.LastIndexOf(".") - 1;
						num2 = ((num2 < 0) ? (text.Length - 1) : num2);
						text = text.Substring(num + 1, num2 - num);
						if (!m_builtLevelNames.Contains(text))
						{
							m_builtLevelNames.Add(text);
						}
					}
				}
				return m_builtLevelNames;
			}
		}

		public LevelManagerCallback OnEvent
		{
			get
			{
				if (m_OnEvent != null)
				{
					return m_OnEvent;
				}
				return m_OnEvent = new LevelManagerCallback();
			}
		}

		internal Dictionary<string, AsyncOperation> m_loaders
		{
			get
			{
				if (_loaders != null)
				{
					return _loaders;
				}
				return _loaders = new Dictionary<string, AsyncOperation>();
			}
		}

		internal Dictionary<string, Action<LevelEvent>> m_callbacks
		{
			get
			{
				if (_callbacks != null)
				{
					return _callbacks;
				}
				return _callbacks = new Dictionary<string, Action<LevelEvent>>();
			}
		}

		protected static List<string> GetBuildSettingsScenes(bool p_name_only = true)
		{
			List<string> scenes = new List<string>();
			for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
			{
				string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
				if (string.IsNullOrEmpty(scenePath))
				{
					continue;
				}
				scenes.Add(p_name_only ? System.IO.Path.GetFileNameWithoutExtension(scenePath) : scenePath);
			}
			return scenes;
		}

		public static T GetRootComponent<T>(string p_name) where T : Component
		{
			GameObject rootGameObject = GetRootGameObject(p_name);
			if (!rootGameObject)
			{
				return null;
			}
			return rootGameObject.GetComponent<T>();
		}

		public static GameObject GetRootGameObject(string p_scene, string p_name)
		{
			GameObject[] rootGameObjects = GetRootGameObjects(p_scene);
			if (rootGameObjects == null)
			{
				return null;
			}
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				if (rootGameObjects[i].name == p_name)
				{
					return rootGameObjects[i];
				}
			}
			return null;
		}

		public static GameObject[] GetRootGameObjects(string p_scene)
		{
			Scene scene = (string.IsNullOrEmpty(p_scene) ? SceneManager.GetActiveScene() : SceneManager.GetSceneByName(p_scene));
			if (!scene.IsValid())
			{
				return null;
			}
			if (!scene.isLoaded)
			{
				return null;
			}
			return scene.GetRootGameObjects();
		}

		public static GameObject GetRootGameObject(string p_scene, int p_id)
		{
			Scene scene = (string.IsNullOrEmpty(p_scene) ? SceneManager.GetActiveScene() : SceneManager.GetSceneByName(p_scene));
			if (!scene.IsValid())
			{
				return null;
			}
			if (!scene.isLoaded)
			{
				return null;
			}
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			if (p_id < 0)
			{
				return null;
			}
			if (p_id >= rootGameObjects.Length)
			{
				return null;
			}
			return rootGameObjects[p_id];
		}

		public static GameObject GetRootGameObject(string p_name)
		{
			return GetRootGameObject("", p_name);
		}

		public static Scene GetDontDestroyScene()
		{
			GameObject obj = new GameObject();
			UnityEngine.Object.DontDestroyOnLoad(obj);
			Scene scene = obj.scene;
			UnityEngine.Object.Destroy(obj);
			return scene;
		}

		public static GameObject[] GetDontDestroyRootObjects()
		{
			Scene dontDestroyScene = GetDontDestroyScene();
			if (!dontDestroyScene.IsValid())
			{
				return null;
			}
			return dontDestroyScene.GetRootGameObjects();
		}

		public static GameObject GetDontDestroyObject(string p_name)
		{
			GameObject[] dontDestroyRootObjects = GetDontDestroyRootObjects();
			if (dontDestroyRootObjects == null)
			{
				return null;
			}
			foreach (GameObject gameObject in dontDestroyRootObjects)
			{
				if ((bool)gameObject && gameObject.name == p_name)
				{
					return gameObject;
				}
			}
			return null;
		}

		public static T GetDontDestroyObject<T>() where T : Component
		{
			GameObject[] dontDestroyRootObjects = GetDontDestroyRootObjects();
			if (dontDestroyRootObjects == null)
			{
				return null;
			}
			foreach (GameObject gameObject in dontDestroyRootObjects)
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

		public static void ClearDontDestroyScene()
		{
			GameObject[] dontDestroyRootObjects = GetDontDestroyRootObjects();
			for (int i = 0; i < dontDestroyRootObjects.Length; i++)
			{
				if ((bool)dontDestroyRootObjects[i])
				{
					UnityEngine.Object.Destroy(dontDestroyRootObjects[i]);
				}
			}
		}

		protected void Awake()
		{
			m_gc = new List<string>();
		}

		public void AddBundle(AssetBundle p_bundle)
		{
			if (!m_bundles.Contains(p_bundle))
			{
				m_bundles.Add(p_bundle);
			}
		}

		public void RemoveBundle(AssetBundle p_bundle)
		{
			if (m_bundles.Contains(p_bundle))
			{
				m_bundles.Remove(p_bundle);
			}
		}

		public void UnloadBundles()
		{
			for (int i = 0; i < m_bundles.Count; i++)
			{
				AssetBundle assetBundle = m_bundles[i];
				if ((bool)assetBundle)
				{
					assetBundle.Unload(unloadAllLoadedObjects: true);
					UnityEngine.Object.Destroy(assetBundle);
				}
			}
			m_bundles.Clear();
		}

		public bool IsLevelLoaded(string p_name)
		{
			return levelNames.Contains(p_name);
		}

		public bool IsLevelBuiltIn(string p_name)
		{
			return builtLevelNames.Contains(p_name);
		}

		public bool IsLevelInBuildSettings(string p_name)
		{
			return GetBuildSettingsScenes().Contains(p_name);
		}

		public void WaitLevel(string p_name, Action<bool> p_callback)
		{
			Scene scn = SceneManager.GetSceneByName(p_name);
			if (!scn.IsValid() && p_callback != null)
			{
				p_callback(obj: false);
			}
			Activity.Run((Func<bool>)delegate
			{
				if (!scn.isLoaded)
				{
					return true;
				}
				if (p_callback != null)
				{
					p_callback(obj: true);
				}
				return false;
			}, 0f, false);
		}

		public void SetLevelActive(string p_level)
		{
			Scene scn = SceneManager.GetSceneByName(p_level);
			Activity.RunOnce(delegate
			{
				SceneManager.SetActiveScene(scn);
			}, 0.05f);
		}

		public void LoadLevel(string p_name, bool p_force)
		{
			if (!p_force && IsLevelLoaded(p_name))
			{
				Debug.LogWarning("LevelManager> Level [" + p_name + "] already loaded.");
				return;
			}
			if (!IsLevelBuiltIn(p_name))
			{
				Debug.LogWarning("LevelManager> Level [" + p_name + "] don't exists!");
				return;
			}
			SendEvent(p_name, LevelEventType.Progress);
			SendEvent(p_name, LevelEventType.Progress, 1f);
			SendEvent(p_name, LevelEventType.Complete, 1f);
			SceneManager.LoadScene(p_name, LoadSceneMode.Single);
		}

		public void LoadLevel(string p_name)
		{
			LoadLevel(p_name, p_force: false);
		}

		public void AddLevel(string p_name)
		{
			if (IsLevelLoaded(p_name))
			{
				Debug.LogWarning("LevelManager> Level [" + p_name + "] already loaded.");
				return;
			}
			if (!IsLevelBuiltIn(p_name))
			{
				Debug.LogWarning("LevelManager> Level [" + p_name + "] don't exists!");
				return;
			}
			SendEvent(p_name, LevelEventType.Progress);
			SceneManager.LoadScene(p_name, LoadSceneMode.Additive);
			levelNames.Add(p_name);
			SendEvent(p_name, LevelEventType.Progress, 1f);
			SendEvent(p_name, LevelEventType.Complete, 1f);
		}

		public void LoadLevelAsync(string p_name, Action<LevelEvent> p_callback = null)
		{
			if (IsLevelLoaded(p_name))
			{
				Debug.LogWarning("LevelManager> Level [" + p_name + "] already loaded.");
				SendEvent(p_name, LevelEventType.Complete, 1f, p_callback);
				return;
			}
			if (!IsLevelBuiltIn(p_name))
			{
				Debug.LogWarning("LevelManager> Level [" + p_name + "] don't exists!");
				return;
			}
			AsyncOperation asyncOperation = null;
			SendEvent(p_name, LevelEventType.Progress, 0f, p_callback);
			asyncOperation = SceneManager.LoadSceneAsync(p_name, LoadSceneMode.Single);
			m_loaders[p_name] = asyncOperation;
			m_callbacks[p_name] = p_callback;
		}

		public void AddLevelAsync(string p_name, Action<LevelEvent> p_callback, bool p_set_active)
		{
			if (IsLevelLoaded(p_name))
			{
				Debug.LogWarning("LevelManager> Level [" + p_name + "] already loaded.");
				SendEvent(p_name, LevelEventType.Complete, 1f, p_callback);
				if (p_set_active)
				{
					SetLevelActive(p_name);
				}
				return;
			}
			if (!IsLevelBuiltIn(p_name))
			{
				Debug.LogWarning("LevelManager> Level [" + p_name + "] don't exists!");
				SendEvent(p_name, LevelEventType.Complete, 1f, p_callback);
				return;
			}
			AsyncOperation asyncOperation = null;
			SendEvent(p_name, LevelEventType.Progress, 0f, p_callback);
			asyncOperation = SceneManager.LoadSceneAsync(p_name, LoadSceneMode.Additive);
			asyncOperation.allowSceneActivation = false;
			m_loaders[p_name] = asyncOperation;
			m_callbacks[p_name] = p_callback;
			UnityAction<LevelEvent> cb = null;
			cb = delegate(LevelEvent ev)
			{
				if (ev.name == p_name && p_set_active)
				{
					SetLevelActive(p_name);
				}
				OnEvent.RemoveListener(cb);
			};
			OnEvent.AddListener(cb);
		}

		public void AddLevelAsync(string p_name, Action<LevelEvent> p_callback)
		{
			AddLevelAsync(p_name, p_callback, p_set_active: false);
		}

		public void AddLevelAsync(string p_name, bool p_set_active)
		{
			AddLevelAsync(p_name, null, p_set_active);
		}

		public void AddLevelAsync(string p_name)
		{
			AddLevelAsync(p_name, null, p_set_active: false);
		}

		public AsyncOperation UnloadAsync(string p_name, Action p_callback = null)
		{
			if (!IsLevelLoaded(p_name))
			{
				p_callback?.Invoke();
				return null;
			}
			levelNames.Remove(p_name);
			AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(p_name);
			StartCoroutine(UnloadAsyncHandler(asyncOperation, p_callback));
			return asyncOperation;
		}

		private IEnumerator UnloadAsyncHandler(AsyncOperation p_operation, Action p_callback = null)
		{
			while (p_operation != null && !p_operation.isDone)
			{
				yield return p_operation.isDone;
			}
			p_callback?.Invoke();
		}

		public void UnloadAll()
		{
			for (int i = 0; i < levelNames.Count; i++)
			{
				UnloadAsync(levelNames[i]);
			}
		}

		protected void Update()
		{
			foreach (KeyValuePair<string, AsyncOperation> loader in m_loaders)
			{
				string key = loader.Key;
				AsyncOperation value = loader.Value;
				Action<LevelEvent> p_callback = (m_callbacks.ContainsKey(key) ? m_callbacks[key] : null);
				if (value == null)
				{
					continue;
				}
				if (!value.allowSceneActivation && value.progress >= 0.9f)
				{
					value.allowSceneActivation = true;
				}
				SendEvent(key, LevelEventType.Progress, value.progress * 0.999f, p_callback);
				if (value.isDone)
				{
					if (levelNames.IndexOf(key) < 0)
					{
						levelNames.Add(key);
					}
					if (m_gc != null)
					{
						m_gc.Add(key);
					}
					SendEvent(key, LevelEventType.Progress, 1f, p_callback);
					SendEvent(key, LevelEventType.Complete, 1f, p_callback);
				}
			}
			if (((m_gc != null && m_gc.Count != 0) ? 1 : 0) > (false ? 1 : 0))
			{
				for (int i = 0; i < m_gc.Count; i++)
				{
					m_loaders.Remove(m_gc[i]);
					m_callbacks.Remove(m_gc[i]);
				}
				m_gc.Clear();
			}
		}

		private void SendEvent(string p_name, LevelEventType p_type, float p_progress = 0f, Action<LevelEvent> p_callback = null)
		{
			LevelEvent levelEvent = new LevelEvent(p_name, p_type, this, p_progress);
			if (OnEvent != null)
			{
				OnEvent.Invoke(levelEvent);
			}
			p_callback?.Invoke(levelEvent);
		}
	}
}
