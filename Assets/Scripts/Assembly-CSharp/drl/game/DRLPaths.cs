using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class DRLPaths
	{
		public class Content
		{
			public static bool useLibraryLow = false;

			public static List<string> bundleFiles = new List<string>();

			private static int m_bundle_maps_max_size = -1;

			public static string root
			{
				get
				{
					string text = platformRoot;
					switch (Application.platform)
					{
					case RuntimePlatform.OSXEditor:
						text += "game/osx/content/";
						break;
					case RuntimePlatform.WindowsEditor:
						text += "game/win/content/";
						if (!Directory.Exists(text))
						{
							text = streamingAssetsRoot + "game/content/";
						}
						break;
					case RuntimePlatform.OSXPlayer:
						text = streamingAssetsRoot;
						text += "game/content/";
						break;
					case RuntimePlatform.WindowsPlayer:
						text = streamingAssetsRoot;
						text += "game/content/";
						break;
					case RuntimePlatform.XboxOne:
						text += streamingAssetsRoot;
						text += "game/content/";
						break;
					case RuntimePlatform.PS4:
						text += streamingAssetsRoot;
						text += "content/";
						break;
					}
					return Assert(text);
				}
			}

			public static string bundleChangeLog => root + ".messages";

			public static int bundleMaxSize
			{
				get
				{
					List<int> bundleFilesSize = GetBundleFilesSize(true);
					int num = 0;
					for (int i = 0; i < bundleFilesSize.Count; i++)
					{
						num = Mathf.Max(bundleFilesSize[i], num);
					}
					return num;
				}
			}

			public static string libraryRoot => Assert(root + "library/");

			public static string[] libraryBundlePaths => new string[3]
			{
				libraryRoot ?? "",
				libraryRoot + "dependencies/",
				libraryRoot + "dependencies/" + (useLibraryLow ? "low" : "high") + "/"
			};

			public static string mapsRoot => Assert(root + "maps/");

			public static int bundleMapsMaxSize
			{
				get
				{
					if (m_bundle_maps_max_size >= 0)
					{
						return m_bundle_maps_max_size;
					}
					List<string> list = GetBundleFiles("maps/.*");
					int num = 0;
					for (int i = 0; i < list.Count; i++)
					{
						FileInfo fileInfo = new FileInfo(list[i]);
						if (fileInfo.Exists)
						{
							num = Mathf.Max((int)fileInfo.Length, num);
						}
					}
					return m_bundle_maps_max_size = num;
				}
			}

			public static List<string> localCustomMapFiles
			{
				get
				{
					string text = mapsRoot;
					return new List<string>
					{
						text + "multi-gp/multigp-maps.json",
						text + "featured/featured-tracks.json",
						text + "california-nights/california-nights.json",
						text + "skatepark-la/skatepark-la.json",
						text + "bridge/bridge.json",
						text + "adventuredome/adventuredome.json",
						text + "allianz-riviera/allianz-riviera.json",
						text + "biosphere/biosphere.json",
						text + "bmw-welt/bmw-welt.json",
						text + "drone-park/drone-park.json"
					};
				}
			}

			public static string localeRoot => Assert(root + "locale/");

			public static void CollectBundleFiles()
			{
				if (bundleFiles == null)
				{
					bundleFiles = new List<string>();
				}
				bundleFiles.Clear();
				List<string> list = bundleFiles;
				string text = root;
				if (!Directory.Exists(text))
				{
					Debug.LogWarning("DRLPaths> CollectBundleFiles / root[" + text + "] not found!");
					return;
				}
				list.AddRange(Directory.GetFiles(text, "*.bytes", SearchOption.AllDirectories));
				list.AddRange(Directory.GetFiles(text, "*.ablm", SearchOption.AllDirectories));
				for (int i = 0; i < list.Count; i++)
				{
					list[i] = list[i].Replace("\\", "/");
					if (list[i].Contains("/replays"))
					{
						list.RemoveAt(i--);
					}
				}
			}

			public static List<string> GetBundleFiles(string p_search = "*")
			{
				List<string> list = new List<string>();
				try
				{
					Regex rule = new Regex(p_search);
					list = bundleFiles.FindAll((string v) => rule.IsMatch(v));
				}
				catch (Exception)
				{
					string text = root;
					if (!Directory.Exists(text))
					{
						Debug.LogWarning("DRLPaths> GetBundleFiles / root[" + text + "] not found!");
						return list;
					}
					list.AddRange(Directory.GetFiles(text, p_search + ".bytes", SearchOption.AllDirectories));
					for (int num = 0; num < list.Count; num++)
					{
						list[num] = list[num].Replace("\\", "/");
					}
				}
				return list;
			}

			public static List<string> GetBundleFilesByFilter(bool p_ignore, params string[] p_filter)
			{
				List<string> list = GetBundleFiles();
				list.RemoveAll(delegate(string it)
				{
					foreach (string value in p_filter)
					{
						if (it.Contains(value))
						{
							return p_ignore;
						}
					}
					return !p_ignore;
				});
				return list;
			}

			public static List<int> GetBundleFilesSize(bool p_ignore, params string[] p_filter)
			{
				List<string> bundleFilesByFilter = GetBundleFilesByFilter(p_ignore, p_filter);
				List<int> list = new List<int>();
				for (int i = 0; i < bundleFilesByFilter.Count; i++)
				{
					_ = bundleFilesByFilter[i];
					FileInfo fileInfo = new FileInfo(bundleFilesByFilter[i]);
					if (!fileInfo.Exists)
					{
						list.Add(0);
					}
					else
					{
						list.Add((int)fileInfo.Length);
					}
				}
				return list;
			}
		}

		public class Storage
		{
			private static string m_root = null;

			public static List<string> bundleFiles = new List<string>();

			public static string root
			{
				get
				{
					if (!string.IsNullOrEmpty(m_root))
					{
						return m_root;
					}
					string text = platformRoot;
					switch (Application.platform)
					{
					case RuntimePlatform.OSXEditor:
						text += "game/osx/storage/";
						break;
					case RuntimePlatform.WindowsEditor:
						text += "game/win/storage/";
						break;
					case RuntimePlatform.OSXPlayer:
						text = streamingAssetsRoot;
						text += "game/storage/";
						break;
					case RuntimePlatform.WindowsPlayer:
						text = streamingAssetsRoot;
						text += "game/storage/";
						break;
					case RuntimePlatform.XboxOne:
					{
						string persistentDataPath2 = Application.persistentDataPath;
						string temporaryCachePath = Application.temporaryCachePath;
						string p_path = (string.IsNullOrEmpty(persistentDataPath2) ? temporaryCachePath : persistentDataPath2);
						text += Assert(p_path);
						text += "game/storage/";
						break;
					}
					case RuntimePlatform.PS4:
					{
						string persistentDataPath = Application.persistentDataPath;
						text += Assert(persistentDataPath);
						text += "game/storage/";
						break;
					}
					}
					return m_root = Assert(text, p_create: true);
				}
			}

			public static string consoleLogFile => root + "PlayerLog.log";

			public static string stateRoot => Assert(root + "state/", p_create: true);

			public static string manifestRoot => Assert(root + "manifests/", p_create: true);

			public static string userDocumentsRoot
			{
				get
				{
					RuntimePlatform platform = Application.platform;
					if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
					{
						return Assert(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/DRLSim/", p_create: true);
					}
					return stateRoot;
				}
			}

			public static string replaysRoot => Assert(root + "replays/", p_create: true);

			public static string replaysTemp => Assert(replaysRoot + "temp/", p_create: true);

			public static string replaysMapEditorRoot => Assert(replaysRoot + "map-editor/", p_create: true);

			public static string videoRecordRoot => Assert(root + "videos/", p_create: true);

			public static string videoRecordTempRoot => Assert(videoRecordRoot + "temp/", p_create: true);

			public static string videoRecorderExecutableRoot
			{
				get
				{
					string p_path = "";
					switch (Application.platform)
					{
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.WindowsEditor:
						p_path = streamingAssetsRoot + "VideoRecorder/";
						break;
					case RuntimePlatform.OSXPlayer:
					case RuntimePlatform.WindowsPlayer:
						p_path = root + "/videos/VideoRecorder/";
						break;
					}
					return Assert(p_path, p_create: true);
				}
			}

			public static string videoRecorderExecutable
			{
				get
				{
					string text = Assert(videoRecorderExecutableRoot, p_create: true);
					switch (Application.platform)
					{
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsEditor:
						text += "recorder.exe";
						break;
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.OSXPlayer:
						text += "recorder";
						break;
					}
					return text;
				}
			}

			public static int bundleMaxSize
			{
				get
				{
					List<int> bundleFilesSize = GetBundleFilesSize(true);
					int num = 0;
					for (int i = 0; i < bundleFilesSize.Count; i++)
					{
						num = Mathf.Max(bundleFilesSize[i], num);
					}
					return num;
				}
			}

			public static string libraryRoot => Assert(root + "library/", p_create: true);

			public static string localeRoot => Assert(root + "locale/");

			public static string mapsRoot => Assert(root + "maps/", p_create: true);

			public static string offlineRoot => Assert(root + "offline/", p_create: true);

			public static string offlineStateRoot => Assert(offlineRoot + "state/", p_create: true);

			public static string offlineMapsRoot => Assert(offlineRoot + "maps/", p_create: true);

			public static string offlineMapEditorMapsRoot => Assert(offlineMapsRoot + "map-editor/", p_create: true);

			public static string offlineMapsHashFilename => "md-cache.mdc";

			public static string offlineMapEditorMapsHash => "mde-cache.mdc";

			public static string offlineMapsCustomRoot => Assert(offlineMapsRoot + "custom-maps/", p_create: true);

			public static string offlineMapsCustomHash => offlineMapsCustomRoot + "mdc-cache.mdc";

			public static string offlineMapsHash => offlineMapsRoot + offlineMapsHashFilename;

			public static string offlinePlayerStateRoot => Assert(offlineStateRoot + "player/", p_create: true);

			public static string offlinePlayerStateFile => offlinePlayerStateRoot + "player-state.json";

			public static string offlinePlayerStatePicture => offlinePlayerStateRoot + "player-profile.png";

			public static string offlineGameStateRoot => Assert(offlineStateRoot + "game/", p_create: true);

			public static string offlineGameStateFile => offlineGameStateRoot + "game-state.json";

			public static string offlineLeaderboardsRoot => Assert(offlineStateRoot + "leaderboards/", p_create: true);

			public static string offlineCircuitsRoot => Assert(offlineRoot + "circuits/", p_create: true);

			public static void ClearReplaysTemp()
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(replaysTemp);
				if (!directoryInfo.Exists)
				{
					return;
				}
				FileInfo[] files = directoryInfo.GetFiles();
				if (files.Length != 0)
				{
					Debug.Log($"DRLPaths> Clearing {files.Length} Temp Replay Files");
					for (int i = 0; i < files.Length; i++)
					{
						files[i].Delete();
					}
				}
			}

			public static void CollectBundleFiles()
			{
				if (bundleFiles == null)
				{
					bundleFiles = new List<string>();
				}
				bundleFiles.Clear();
				List<string> list = bundleFiles;
				string text = root;
				if (!Directory.Exists(text))
				{
					Debug.LogWarning("DRLPaths> CollectBundleFiles / root[" + text + "] not found!");
					return;
				}
				list.AddRange(Directory.GetFiles(text, "*.bytes", SearchOption.AllDirectories));
				list.AddRange(Directory.GetFiles(text, "*.ablm", SearchOption.AllDirectories));
				for (int i = 0; i < list.Count; i++)
				{
					list[i] = list[i].Replace("\\", "/");
					if (list[i].Contains("/replays"))
					{
						list.RemoveAt(i--);
					}
				}
			}

			public static List<string> GetBundleFiles(string p_search = "*")
			{
				List<string> list = new List<string>();
				try
				{
					Regex rule = new Regex(p_search);
					list = bundleFiles.FindAll((string v) => rule.IsMatch(v));
				}
				catch (Exception)
				{
					string text = root;
					if (!Directory.Exists(text))
					{
						Debug.LogWarning("DRLPaths> GetBundleFiles / root[" + text + "] not found!");
						return list;
					}
					list.AddRange(Directory.GetFiles(text, p_search + ".bytes", SearchOption.AllDirectories));
					for (int num = 0; num < list.Count; num++)
					{
						list[num] = list[num].Replace("\\", "/");
					}
				}
				return list;
			}

			public static List<string> GetBundleFilesByFilter(bool p_ignore, params string[] p_filter)
			{
				List<string> list = GetBundleFiles();
				list.RemoveAll(delegate(string it)
				{
					foreach (string value in p_filter)
					{
						if (it.Contains(value))
						{
							return p_ignore;
						}
					}
					return !p_ignore;
				});
				return list;
			}

			public static List<int> GetBundleFilesSize(bool p_ignore, params string[] p_filter)
			{
				List<string> bundleFilesByFilter = GetBundleFilesByFilter(p_ignore, p_filter);
				List<int> list = new List<int>();
				for (int i = 0; i < bundleFilesByFilter.Count; i++)
				{
					_ = bundleFilesByFilter[i];
					FileInfo fileInfo = new FileInfo(bundleFilesByFilter[i]);
					if (!fileInfo.Exists)
					{
						list.Add(0);
					}
					else
					{
						list.Add((int)fileInfo.Length);
					}
				}
				return list;
			}

			public static string GetMapDataCacheHash(string p_version)
			{
				return string.Join("-", DRLApp.GetVersionStringHash(), p_version);
			}

			public static string GetMapDataCacheFileName(string p_version)
			{
				return "smd-" + GetMapDataCacheHash(p_version) + ".mdc";
			}

			public static string GetMapDataCachePath(string p_version)
			{
				return mapsRoot + GetMapDataCacheFileName(p_version);
			}

			public static List<string> GetMapFiles(string p_search = "*.json")
			{
				List<string> list = new List<string>();
				string text = mapsRoot;
				if (!Directory.Exists(text))
				{
					Debug.LogWarning("DRLPaths> GetMapFiles / root[" + text + "] not found!");
					return list;
				}
				list.AddRange(Directory.GetFiles(text, p_search, SearchOption.AllDirectories));
				for (int i = 0; i < list.Count; i++)
				{
					list[i] = list[i].Replace("\\", "/");
				}
				return list;
			}
		}

		public class Tools
		{
			public static string toolsRoot => Assert(platformRoot + "Tools/");

			public static string osAppRoot
			{
				get
				{
					string result = "";
					switch (Application.platform)
					{
					case RuntimePlatform.WindowsEditor:
						result = string.Format("{0}/../Local/", Environment.GetEnvironmentVariable("AppData"));
						break;
					case RuntimePlatform.WindowsPlayer:
						result = string.Format("{0}/../LocalLow/", Environment.GetEnvironmentVariable("AppData"));
						break;
					case RuntimePlatform.OSXEditor:
						result = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/";
						break;
					case RuntimePlatform.OSXPlayer:
						result = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/";
						break;
					}
					return result;
				}
			}

			public static string consoleLogFile
			{
				get
				{
					string text = osAppRoot;
					switch (Application.platform)
					{
					case RuntimePlatform.WindowsEditor:
						text += "Unity/Editor/Editor.log";
						break;
					case RuntimePlatform.WindowsPlayer:
						text += "Drone Racing League/DRL Simulator/Player.log";
						break;
					case RuntimePlatform.OSXEditor:
						text += "Library/Logs/Unity/Editor.log";
						break;
					case RuntimePlatform.OSXPlayer:
						text += "Library/Logs/Drone Racing League/DRL Simulator/Player.log";
						break;
					case RuntimePlatform.XboxOne:
						text = "";
						break;
					case RuntimePlatform.PS4:
						text = "";
						break;
					}
					return text;
				}
			}

			public static string playerLogFile
			{
				get
				{
					string text = "";
					switch (Application.platform)
					{
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsEditor:
						text = string.Format("{0}/../LocalLow/", Environment.GetEnvironmentVariable("AppData"));
						break;
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.OSXPlayer:
						text = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/";
						break;
					}
					switch (Application.platform)
					{
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsEditor:
						text += "Drone Racing League/DRL Simulator/Player.log";
						break;
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.OSXPlayer:
						text += "Library/Logs/Drone Racing League/DRL Simulator/Player.log";
						break;
					case RuntimePlatform.XboxOne:
						text = "";
						break;
					case RuntimePlatform.PS4:
						text = "";
						break;
					}
					return text;
				}
			}

			public static string GetSteamDeployToolExecutable(bool p_is_dev, bool p_is_offline = false)
			{
				string text = "";
				string text2 = (p_is_dev ? "development" : "release");
				string text3 = (p_is_offline ? "offline" : "steam");
				switch (Application.platform)
				{
				case RuntimePlatform.WindowsEditor:
					text = "deploy_" + text3 + "_" + text2 + ".bat";
					break;
				case RuntimePlatform.OSXEditor:
					text = "deploy_" + text3 + "_" + text2 + ".sh";
					break;
				}
				return toolsRoot + text;
			}
		}

		private static string m_data_path;

		public static List<string> bundleFiles = new List<string>();

		public static string DataPath
		{
			get
			{
				if (!string.IsNullOrEmpty(m_data_path))
				{
					return m_data_path;
				}
				return m_data_path = Application.dataPath;
			}
		}

		public static string editorDeployRoot
		{
			get
			{
				string text = "";
				text = DataPath + "/";
				text = text.Substring(0, text.LastIndexOf("Assets")) + "Deploy/";
				return Assert(text);
			}
		}

		public static string platformRoot
		{
			get
			{
				string p_path = "";
				switch (Application.platform)
				{
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.WindowsEditor:
					p_path = editorDeployRoot;
					break;
				case RuntimePlatform.OSXPlayer:
				case RuntimePlatform.WindowsPlayer:
					p_path = DataPath + "/";
					break;
				}
				return Assert(p_path);
			}
		}

		public static string gameRoot
		{
			get
			{
				string text = platformRoot;
				switch (Application.platform)
				{
				case RuntimePlatform.OSXEditor:
					text += "game/osx/";
					break;
				case RuntimePlatform.WindowsEditor:
					text += "game/win/";
					break;
				case RuntimePlatform.OSXPlayer:
					text = streamingAssetsRoot;
					text += "game/";
					break;
				case RuntimePlatform.WindowsPlayer:
					text = streamingAssetsRoot;
					text += "game/";
					break;
				case RuntimePlatform.XboxOne:
					text += streamingAssetsRoot;
					text += "game/";
					break;
				case RuntimePlatform.PS4:
					text += streamingAssetsRoot;
					text = text ?? "";
					break;
				}
				return Assert(text);
			}
		}

		public static string streamingAssetsRoot => Assert(Application.streamingAssetsPath);

		public static string checksumRoot
		{
			get
			{
				RuntimePlatform platform = Application.platform;
				if (platform != RuntimePlatform.OSXPlayer)
				{
					if (platform != RuntimePlatform.WindowsPlayer)
					{
						_ = 25;
					}
					return Application.dataPath;
				}
				return Application.dataPath + "/Resources/Data";
			}
		}

		public static int bundleMaxSize
		{
			get
			{
				List<int> bundleFilesSize = GetBundleFilesSize(true);
				int num = 0;
				for (int i = 0; i < bundleFilesSize.Count; i++)
				{
					num = Mathf.Max(bundleFilesSize[i], num);
				}
				return num;
			}
		}

		public static string appName => "DRL Simulator";

		public static string appBuildHash
		{
			get
			{
				string p_thf = "HH";
				string s = Format.DateHash("yy", "MM", "dd", p_thf, "mm", "");
				uint result = 65535u;
				uint.TryParse(s, out result);
				return result.ToString("x");
			}
		}

		public static string Assert(string p_path, bool p_create = false)
		{
			if (string.IsNullOrEmpty(p_path))
			{
				return p_path;
			}
			p_path = p_path.Replace("\\", "/");
			if (!p_path.EndsWith("/"))
			{
				p_path += "/";
			}
			if (p_create)
			{
				try
				{
					if (!Directory.Exists(p_path))
					{
						Directory.CreateDirectory(p_path);
					}
				}
				catch (Exception)
				{
					Debug.LogWarning("DRLPaths> Assert / Failed to Create Directory - path[" + p_path + "]");
				}
			}
			return p_path;
		}

		public static void Log()
		{
			List<string> list = new List<string>();
			list.Add($"DRLPaths> Structure / platform[{Application.platform}]");
			list.Add("Core");
			list.Add("  data-path:       " + DataPath);
			list.Add("  persistent-path: " + Application.persistentDataPath);
			list.Add("  temp-cache-path: " + Application.temporaryCachePath);
			list.Add("  platform-root:   " + platformRoot);
			list.Add("  streaming-root:  " + streamingAssetsRoot);
			list.Add("Content");
			list.Add($"  library-low:     {Content.useLibraryLow}");
			list.Add("  root:            " + Content.root);
			list.Add("  maps:            " + Content.mapsRoot);
			list.Add("  library:         " + Content.libraryRoot);
			list.Add("Storage");
			list.Add("  root:            " + Storage.root);
			list.Add("  replays:         " + Storage.replaysRoot);
			list.Add("  library:         " + Storage.libraryRoot);
			list.Add("  replays-me:      " + Storage.replaysMapEditorRoot);
			list.Add("  videos:          " + Storage.videoRecordRoot);
			list.Add("  maps:            " + Storage.mapsRoot);
			if (Application.platform == RuntimePlatform.XboxOne)
			{
				for (int i = 0; i < list.Count; i++)
				{
					Debug.Log(list[i]);
				}
			}
			else
			{
				Debug.Log(string.Join("\n", list.ToArray()));
			}
		}

		public static void CollectBundleFiles()
		{
			if (bundleFiles == null)
			{
				bundleFiles = new List<string>();
			}
			bundleFiles.Clear();
			Content.CollectBundleFiles();
			Storage.CollectBundleFiles();
			List<string> list = new List<string>(Content.bundleFiles);
			List<string> list2 = new List<string>(Storage.bundleFiles);
			if (list2.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					FileInfo fileInfo = new FileInfo(list[i]);
					if (!fileInfo.Exists)
					{
						continue;
					}
					for (int j = 0; j < list2.Count; j++)
					{
						FileInfo fileInfo2 = new FileInfo(list2[j]);
						if (fileInfo2.Exists && !(fileInfo.Name != fileInfo2.Name))
						{
							list.RemoveAt(i--);
							break;
						}
					}
				}
			}
			bundleFiles.AddRange(list);
			bundleFiles.AddRange(list2);
			bundleFiles.Sort();
		}

		public static List<string> GetBundleFiles(string p_search = ".*")
		{
			List<string> result = new List<string>();
			try
			{
				Regex rule = new Regex(p_search);
				result = bundleFiles.FindAll((string v) => rule.IsMatch(v));
			}
			catch (Exception)
			{
			}
			return result;
		}

		public static List<string> GetBundleFilesByFilter(bool p_ignore, params string[] p_filter)
		{
			List<string> list = GetBundleFiles();
			list.RemoveAll(delegate(string it)
			{
				foreach (string value in p_filter)
				{
					if (it.Contains(value))
					{
						return p_ignore;
					}
				}
				return !p_ignore;
			});
			return list;
		}

		public static List<int> GetBundleFilesSize(bool p_ignore, params string[] p_filter)
		{
			List<string> bundleFilesByFilter = GetBundleFilesByFilter(p_ignore, p_filter);
			List<int> list = new List<int>();
			for (int i = 0; i < bundleFilesByFilter.Count; i++)
			{
				_ = bundleFilesByFilter[i];
				FileInfo fileInfo = new FileInfo(bundleFilesByFilter[i]);
				if (!fileInfo.Exists)
				{
					list.Add(0);
				}
				else
				{
					list.Add((int)fileInfo.Length);
				}
			}
			return list;
		}

		public static string GetAppBuildTimeHash(int p_offset, int p_length = -1)
		{
			DateTime utcNow = DateTime.UtcNow;
			int num = utcNow.Month + p_offset;
			int num2 = utcNow.Day + p_offset;
			int num3 = utcNow.Hour / 3;
			string text = $"{num}{num2}{num3}";
			int length = Mathf.Min(text.Length, p_length);
			if (p_length >= 0)
			{
				text = text.Substring(0, length);
			}
			uint result = 40700u;
			uint.TryParse(text, out result);
			return result.ToString();
		}
	}
}
