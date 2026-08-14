using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLApp : BaseApplication<DRLModel, DRLView, DRLController>
	{
		private static long process_replay_mh_m0;

		private static long process_replay_mu_m0;

		private static long process_replay_ta_m0;

		private static long process_replay_tr_m0;

		private static long process_replay_tu_m0;

		[SerializeField]
		private static string m_branch_name;

		private LevelManager m_level;

		private static DRLSceneManager m_scene;

		private static BrightnessFX m_brightness;

		private DRLAppArguments m_args;

		private static bool m_offline;

		public static bool forceOffline;

		public Texture2D cursor;

		public static string buildHash => DRLVersion.value;

		public static string version => DRLVersion.major + "." + DRLVersion.minimum;

		public static string branchName
		{
			get
			{
				if (offline || forceOffline)
				{
					return m_branch_name = "public";
				}
				m_branch_name = "";
				if (SteamApps.GetCurrentBetaName(out m_branch_name, 128))
				{
					return m_branch_name = m_branch_name.Trim().ToLower();
				}
				return m_branch_name = "public";
			}
		}

		public LevelManager level
		{
			get
			{
				if ((bool)m_level)
				{
					return m_level;
				}
				m_level = Object.FindObjectOfType<LevelManager>();
				if ((bool)m_level)
				{
					return m_level;
				}
				GameObject gameObject = new GameObject("app.level");
				Object.DontDestroyOnLoad(gameObject);
				return m_level = gameObject.AddComponent<LevelManager>();
			}
		}

		public DRLSceneManager scene
		{
			get
			{
				return m_scene;
			}
			set
			{
				m_scene = value;
			}
		}

		public BrightnessFX brightness
		{
			get
			{
				return m_brightness;
			}
			set
			{
				m_brightness = value;
			}
		}

		public DRLBootController boot => Assert<DRLBootController>("boot");

		public DRLTime time => Assert<DRLTime>("time");

		public DRLACS acs => Assert<DRLACS>("acs");

		public bool online
		{
			get
			{
				if ((bool)base.model && (bool)base.model.network && !base.model.network.isOnline)
				{
					return false;
				}
				if ((bool)base.controller && (bool)base.controller.plm && (bool)base.controller.plm.network && !base.controller.plm.network.connected)
				{
					return false;
				}
				if (offline)
				{
					return false;
				}
				return true;
			}
		}

		public DRLAppArguments arguments
		{
			get
			{
				if ((bool)m_args)
				{
					return m_args;
				}
				m_args = Object.FindObjectOfType<DRLAppArguments>();
				if ((bool)m_args)
				{
					return m_args;
				}
				GameObject gameObject = new GameObject("app.args");
				Object.DontDestroyOnLoad(gameObject);
				return m_args = gameObject.AddComponent<DRLAppArguments>();
			}
		}

		public string hash
		{
			get
			{
				DRLMap map = scene.map;
				DRLMapTrack track = scene.track;
				DRLMission mission = scene.mission;
				string text = Format.DateHash() + "_";
				DRLTournamentLegacyData tournamentLegacy = arguments.game.tournamentLegacy;
				if (tournamentLegacy != null)
				{
					text = text + "Tournament_" + tournamentLegacy.guid.ToUpper() + "_" + tournamentLegacy.order + "_";
				}
				text = text + arguments.game.type.ToString() + "_";
				text += arguments.game.mode;
				if ((bool)mission)
				{
					text = text + "_" + mission.guid;
				}
				if ((bool)map)
				{
					text = text + "_" + map.guid;
				}
				if ((bool)track)
				{
					text = text + "_" + track.guid;
				}
				if ((bool)map && map.data != null)
				{
					text = text + "_" + map.data.guid;
				}
				return text;
			}
		}

		public bool inGame => base.model.game != null;

		public bool inMain => !inGame;

		public bool inGarage => SceneManager.GetActiveScene().name == "garage";

		public bool inCircuits => base.model.storage.state.player.circuits.inProgress;

		public bool inOnboarding
		{
			get
			{
				if (base.model.onboarding.inProgress && !inMultiplayer)
				{
					return !inTournament;
				}
				return false;
			}
		}

		public static bool offline
		{
			get
			{
				if (!m_offline)
				{
					return forceOffline;
				}
				return true;
			}
			set
			{
				m_offline = value;
			}
		}

		public static bool isLoading => DRLUINavigationSystem.IsLoading;

		public static string systemLocale
		{
			get
			{
				string result = "en-us";
				switch (Application.systemLanguage)
				{
				case SystemLanguage.English:
					result = "en-us";
					break;
				case SystemLanguage.Chinese:
				case SystemLanguage.ChineseSimplified:
					result = "zh";
					break;
				default:
					Debug.LogWarning($"DRLApp> GetSystemLanguage / No Locale File system-lang[{Application.systemLanguage}]");
					break;
				}
				return result;
			}
		}

		public bool inTournament
		{
			get
			{
				if (tournament != null)
				{
					return base.model.tournament.tournament != null;
				}
				return false;
			}
		}

		public bool inMultiplayer => base.model.network.room != null;

		public bool inVirtualSeason => false;

		public DRLTournamentData tournament
		{
			get
			{
				if (!inMain || arguments.tournament == null)
				{
					return arguments.game.tournamentData;
				}
				return arguments.tournament.data;
			}
		}

		public static void ClearMemStats()
		{
			long monoHeapSizeLong = Profiler.GetMonoHeapSizeLong();
			long monoUsedSizeLong = Profiler.GetMonoUsedSizeLong();
			long totalAllocatedMemoryLong = Profiler.GetTotalAllocatedMemoryLong();
			long totalReservedMemoryLong = Profiler.GetTotalReservedMemoryLong();
			long totalUnusedReservedMemoryLong = Profiler.GetTotalUnusedReservedMemoryLong();
			process_replay_mh_m0 = monoHeapSizeLong;
			process_replay_mu_m0 = monoUsedSizeLong;
			process_replay_ta_m0 = totalAllocatedMemoryLong;
			process_replay_tr_m0 = totalReservedMemoryLong;
			process_replay_tu_m0 = totalUnusedReservedMemoryLong;
		}

		public static void LogMemStats(string p_title, bool p_show_delta)
		{
			long monoHeapSizeLong = Profiler.GetMonoHeapSizeLong();
			long monoUsedSizeLong = Profiler.GetMonoUsedSizeLong();
			long totalAllocatedMemoryLong = Profiler.GetTotalAllocatedMemoryLong();
			long totalReservedMemoryLong = Profiler.GetTotalReservedMemoryLong();
			long totalUnusedReservedMemoryLong = Profiler.GetTotalUnusedReservedMemoryLong();
			float num = (float)(monoHeapSizeLong - process_replay_mh_m0) / 1024f / 1024f;
			float num2 = (float)(monoUsedSizeLong - process_replay_mu_m0) / 1024f / 1024f;
			float num3 = (float)(totalAllocatedMemoryLong - process_replay_ta_m0) / 1024f / 1024f;
			float num4 = (float)(totalReservedMemoryLong - process_replay_tr_m0) / 1024f / 1024f;
			float num5 = (float)(totalUnusedReservedMemoryLong - process_replay_tu_m0) / 1024f / 1024f;
			string text = ((Mathf.Abs(num) <= 0f) ? "#ccc" : ((num > 0f) ? "#f00" : "#0f0"));
			string text2 = ((Mathf.Abs(num2) <= 0f) ? "#ccc" : ((num2 > 0f) ? "#f00" : "#0f0"));
			string text3 = ((Mathf.Abs(num3) <= 0f) ? "#ccc" : ((num3 > 0f) ? "#f00" : "#0f0"));
			string text4 = ((Mathf.Abs(num4) <= 0f) ? "#ccc" : ((num4 > 0f) ? "#f00" : "#0f0"));
			string text5 = ((Mathf.Abs(num5) <= 0f) ? "#ccc" : ((num5 > 0f) ? "#f00" : "#0f0"));
			string text6 = ((Mathf.Abs(num) <= 0f) ? " " : ((num > 0f) ? "+" : ""));
			string text7 = ((Mathf.Abs(num2) <= 0f) ? " " : ((num2 > 0f) ? "+" : ""));
			string text8 = ((Mathf.Abs(num3) <= 0f) ? " " : ((num3 > 0f) ? "+" : ""));
			string text9 = ((Mathf.Abs(num4) <= 0f) ? " " : ((num4 > 0f) ? "+" : ""));
			string text10 = ((Mathf.Abs(num5) <= 0f) ? " " : ((num5 > 0f) ? "+" : ""));
			string arg = (p_show_delta ? (" /<b> <color=" + text + ">" + text6 + num.ToString("0.0") + "mb</color></b>") : "");
			string arg2 = (p_show_delta ? (" /<b> <color=" + text2 + ">" + text7 + num2.ToString("0.0") + "mb</color></b>") : "");
			string arg3 = (p_show_delta ? (" /<b> <color=" + text3 + ">" + text8 + num3.ToString("0.0") + "mb</color></b>") : "");
			string arg4 = (p_show_delta ? (" /<b> <color=" + text4 + ">" + text9 + num4.ToString("0.0") + "mb</color></b>") : "");
			string arg5 = (p_show_delta ? (" /<b> <color=" + text5 + ">" + text10 + num5.ToString("0.0") + "mb</color></b>") : "");
			List<string> values = new List<string>
			{
				"[MEM] " + p_title.PadRight(20),
				$"MonoHeap: {monoHeapSizeLong / 1024 / 1024}mb{arg}",
				$"MonoUsed: {monoUsedSizeLong / 1024 / 1024}mb{arg2}",
				$"TotalAlloc: {totalAllocatedMemoryLong / 1024 / 1024}mb{arg3}",
				$"TotalReserved: {totalReservedMemoryLong / 1024 / 1024}mb{arg4}",
				$"TotalUnused: {totalUnusedReservedMemoryLong / 1024 / 1024}mb{arg5}"
			};
			Debug.Log(string.Join("<color=#ff0> <b>|</b> </color>", values));
			ClearMemStats();
		}

		public static string GetVersionString()
		{
			return DRLVersion.full;
		}

		public static string GetVersionStringHash()
		{
			return GetVersionString().Substring(1).Replace('.', '-').Replace('*', '-')
				.Replace("---", "-")
				.Replace("--", "-");
		}

		public static bool IsLowRAMSpec()
		{
			if (GetCpuRAM() > 4)
			{
				return GetGpuRAM() <= 2;
			}
			return true;
		}

		public static bool IsHighRAMSpec()
		{
			if (GetCpuRAM() >= 16)
			{
				return GetGpuRAM() >= 2;
			}
			return false;
		}

		public static int GetCpuRAM()
		{
			return SystemInfo.systemMemorySize / 1000;
		}

		public static int GetGpuRAM()
		{
			return SystemInfo.graphicsMemorySize / 1000;
		}

		public static AppSystemInfo GetSystemInfo()
		{
			AppSystemInfo appSystemInfo = new AppSystemInfo();
			appSystemInfo.version = GetVersionString().ToString();
			appSystemInfo.operatingSystem = SystemInfo.operatingSystem.ToString();
			appSystemInfo.processorFrequency = SystemInfo.processorFrequency.ToString();
			appSystemInfo.processorCount = SystemInfo.processorCount.ToString();
			appSystemInfo.processorType = SystemInfo.processorType.ToString();
			appSystemInfo.deviceModel = SystemInfo.deviceModel.ToString();
			appSystemInfo.deviceName = SystemInfo.deviceName.ToString();
			appSystemInfo.deviceType = SystemInfo.deviceType.ToString();
			appSystemInfo.graphicsDeviceID = SystemInfo.graphicsDeviceID.ToString();
			appSystemInfo.graphicsDeviceName = SystemInfo.graphicsDeviceName.ToString();
			appSystemInfo.graphicsDeviceType = SystemInfo.graphicsDeviceType.ToString();
			appSystemInfo.graphicsDeviceVendor = SystemInfo.graphicsDeviceVendor.ToString();
			appSystemInfo.graphicsDeviceVendorID = SystemInfo.graphicsDeviceVendorID.ToString();
			appSystemInfo.graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion.ToString();
			appSystemInfo.systemMemorySize = SystemInfo.systemMemorySize.ToString();
			appSystemInfo.graphicsMemorySize = SystemInfo.graphicsMemorySize.ToString();
			appSystemInfo.graphicsMultiThreaded = SystemInfo.graphicsMultiThreaded.ToString();
			appSystemInfo.graphicsShaderLevel = SystemInfo.graphicsShaderLevel.ToString();
			appSystemInfo.maxTextureSize = SystemInfo.maxTextureSize.ToString();
			appSystemInfo.npotSupport = SystemInfo.npotSupport.ToString();
			appSystemInfo.supportSparseTexture = SystemInfo.supportsSparseTextures.ToString();
			appSystemInfo.supportedRenderTargetCount = SystemInfo.supportedRenderTargetCount.ToString();
			appSystemInfo.copyTextureSupport = SystemInfo.copyTextureSupport.ToString();
			appSystemInfo.supports3DTextures = SystemInfo.supports3DTextures.ToString();
			appSystemInfo.supportsShadows = SystemInfo.supportsShadows.ToString();
			appSystemInfo.currentResolutionWidth = Screen.currentResolution.width.ToString();
			appSystemInfo.currentResolutionHeight = Screen.currentResolution.height.ToString();
			appSystemInfo.quality = QualitySettings.GetQualityLevel().ToString();
			appSystemInfo.displayCount = Display.displays.Length.ToString() ?? "";
			appSystemInfo.displayResolutions = "";
			for (int i = 0; i < Display.displays.Length; i++)
			{
				Display display = Display.displays[i];
				if (i > 0)
				{
					appSystemInfo.displayResolutions += ",";
				}
				AppSystemInfo appSystemInfo2 = appSystemInfo;
				appSystemInfo2.displayResolutions = appSystemInfo2.displayResolutions + display.systemWidth + "x" + display.systemHeight + "@" + display.active;
			}
			return appSystemInfo;
		}
	}
}
