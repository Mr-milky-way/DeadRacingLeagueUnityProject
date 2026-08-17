using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AkWwiseInitializationSettings : AkCommonPlatformSettings
{
	public abstract class PlatformSettings : AkCommonPlatformSettings
	{
		[SerializeField]
		[HideInInspector]
		private List<string> IgnorePropertyNameList = new List<string>();

		[SerializeField]
		[HideInInspector]
		private List<string> GlobalPropertyNameList = new List<string>();

		private HashSet<string> _GlobalPropertyHashSet;

		public HashSet<string> GlobalPropertyHashSet
		{
			get
			{
				if (_GlobalPropertyHashSet == null)
				{
					_GlobalPropertyHashSet = new HashSet<string>(GlobalPropertyNameList);
				}
				return _GlobalPropertyHashSet;
			}
			set
			{
				_GlobalPropertyHashSet = value;
			}
		}

		public void IgnorePropertyValue(string propertyPath)
		{
			if (!IsPropertyIgnored(propertyPath))
			{
				IgnorePropertyNameList.Add(propertyPath);
				SetUseGlobalPropertyValue(propertyPath, use: false);
			}
		}

		public bool IsPropertyIgnored(string propertyPath)
		{
			return IgnorePropertyNameList.Contains(propertyPath);
		}

		protected PlatformSettings()
		{
			SetGlobalPropertyValues(AllGlobalValues);
		}

		public void SetUseGlobalPropertyValue(string propertyPath, bool use)
		{
			if (IsUsingGlobalPropertyValue(propertyPath) != use)
			{
				if (use)
				{
					GlobalPropertyNameList.Add(propertyPath);
				}
				else
				{
					GlobalPropertyNameList.Remove(propertyPath);
				}
				_GlobalPropertyHashSet = null;
			}
		}

		public void SetGlobalPropertyValues(IEnumerable enumerable)
		{
			foreach (object item in enumerable)
			{
				string text = item as string;
				if (!IsUsingGlobalPropertyValue(text))
				{
					GlobalPropertyNameList.Add(text);
				}
			}
		}

		private bool IsUsingGlobalPropertyValue(string propertyPath)
		{
			return GlobalPropertyNameList.Contains(propertyPath);
		}
	}

	public class CommonPlatformSettings : PlatformSettings
	{
		[HideInInspector]
		public AkCommonUserSettings UserSettings;

		[HideInInspector]
		public AkCommonAdvancedSettings AdvancedSettings;

		[HideInInspector]
		public AkCommonCommSettings CommsSettings;

		protected override AkCommonUserSettings GetUserSettings()
		{
			return UserSettings;
		}

		protected override AkCommonAdvancedSettings GetAdvancedSettings()
		{
			return AdvancedSettings;
		}

		protected override AkCommonCommSettings GetCommsSettings()
		{
			return CommsSettings;
		}
	}

	[HideInInspector]
	public List<string> PlatformSettingsNameList = new List<string>();

	[HideInInspector]
	public List<PlatformSettings> PlatformSettingsList = new List<PlatformSettings>();

	[HideInInspector]
	public List<string> InvalidReferencePlatforms = new List<string>();

	[HideInInspector]
	public AkCommonUserSettings UserSettings;

	[HideInInspector]
	public AkCommonAdvancedSettings AdvancedSettings;

	[HideInInspector]
	public AkCommonCommSettings CommsSettings;

	private static readonly string[] AllGlobalValues = new string[55]
	{
		"UserSettings.m_BasePath", "UserSettings.m_StartupLanguage", "UserSettings.m_CallbackManagerBufferSize", "UserSettings.m_EngineLogging", "UserSettings.m_MaximumNumberOfMemoryPools", "UserSettings.m_MaximumNumberOfPositioningPaths", "UserSettings.m_DefaultPoolSize", "UserSettings.m_MemoryCutoffThreshold", "UserSettings.m_CommandQueueSize", "UserSettings.m_SamplesPerFrame",
		"UserSettings.m_MainOutputSettings.m_AudioDeviceShareset", "UserSettings.m_MainOutputSettings.m_DeviceID", "UserSettings.m_MainOutputSettings.m_PanningRule", "UserSettings.m_MainOutputSettings.m_ChannelConfig.m_ChannelConfigType", "UserSettings.m_MainOutputSettings.m_ChannelConfig.m_ChannelMask", "UserSettings.m_MainOutputSettings.m_ChannelConfig.m_NumberOfChannels", "UserSettings.m_StreamingLookAheadRatio", "UserSettings.m_StreamManagerPoolSize", "UserSettings.m_SampleRate", "UserSettings.m_LowerEnginePoolSize",
		"UserSettings.m_LowerEngineMemoryCutoffThreshold", "UserSettings.m_NumberOfRefillsInVoice", "UserSettings.m_SpatialAudioSettings.m_PoolSize", "UserSettings.m_SpatialAudioSettings.m_MaxSoundPropagationDepth", "UserSettings.m_SpatialAudioSettings.m_DiffractionFlags", "UserSettings.m_SpatialAudioSettings.m_MovementThreshold", "UserSettings.m_SpatialAudioSettings.m_NumberOfPrimaryRays", "UserSettings.m_SpatialAudioSettings.m_MaxReflectionOrder", "UserSettings.m_SpatialAudioSettings.m_MaxPathLength", "UserSettings.m_SpatialAudioSettings.m_EnableDiffractionOnReflections",
		"UserSettings.m_SpatialAudioSettings.m_EnableDirectPathDiffraction", "UserSettings.m_SpatialAudioSettings.m_EnableTransmission", "CommsSettings.m_PoolSize", "CommsSettings.m_DiscoveryBroadcastPort", "CommsSettings.m_CommandPort", "CommsSettings.m_NotificationPort", "CommsSettings.m_InitializeSystemComms", "CommsSettings.m_NetworkName", "AdvancedSettings.m_IOMemorySize", "AdvancedSettings.m_TargetAutoStreamBufferLengthMs",
		"AdvancedSettings.m_UseStreamCache", "AdvancedSettings.m_MaximumPinnedBytesInCache", "AdvancedSettings.m_PrepareEventMemoryPoolID", "AdvancedSettings.m_EnableGameSyncPreparation", "AdvancedSettings.m_ContinuousPlaybackLookAhead", "AdvancedSettings.m_MonitorPoolSize", "AdvancedSettings.m_MonitorQueuePoolSize", "AdvancedSettings.m_MaximumHardwareTimeoutMs", "AdvancedSettings.m_SpatialAudioSettings.m_DiffractionShadowAttenuationFactor", "AdvancedSettings.m_SpatialAudioSettings.m_DiffractionShadowDegrees",
		"AdvancedSettings.m_RenderDuringFocusLoss", "AdvancedSettings.m_UseAsyncOpen", "AdvancedSettings.m_SoundBankPersistentDataPath", "AdvancedSettings.m_DebugOutOfRangeCheckEnabled", "AdvancedSettings.m_DebugOutOfRangeLimit"
	};

	private static AkWwiseInitializationSettings m_Instance = null;

	private static AkBasePlatformSettings m_ActivePlatformSettings = null;

	public bool IsValid => PlatformSettingsNameList.Count == PlatformSettingsList.Count;

	public int Count => PlatformSettingsList.Count;

	public static AkWwiseInitializationSettings Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = Resources.Load<AkWwiseInitializationSettings>("AkWwiseInitializationSettings");
				if (m_Instance == null)
				{
					m_Instance = ScriptableObject.CreateInstance<AkWwiseInitializationSettings>();
					Debug.LogWarning("WwiseUnity: No platform specific settings were created. Default initialization settings will be used.");
				}
			}
			return m_Instance;
		}
	}

	public static AkBasePlatformSettings ActivePlatformSettings
	{
		get
		{
			if (m_ActivePlatformSettings == null)
			{
				m_ActivePlatformSettings = GetPlatformSettings(AkBasePathGetter.GetPlatformName());
			}
			return m_ActivePlatformSettings;
		}
	}

	protected override AkCommonUserSettings GetUserSettings()
	{
		return UserSettings;
	}

	protected override AkCommonAdvancedSettings GetAdvancedSettings()
	{
		return AdvancedSettings;
	}

	protected override AkCommonCommSettings GetCommsSettings()
	{
		return CommsSettings;
	}

	private static AkBasePlatformSettings GetPlatformSettings(string platformName)
	{
		AkWwiseInitializationSettings instance = Instance;
		if (!instance.IsValid)
		{
			return instance;
		}
		for (int i = 0; i < instance.Count; i++)
		{
			PlatformSettings platformSettings = instance.PlatformSettingsList[i];
			if ((bool)platformSettings && string.Equals(platformName, instance.PlatformSettingsNameList[i], StringComparison.OrdinalIgnoreCase))
			{
				return platformSettings;
			}
		}
		Debug.LogWarning("WwiseUnity: Platform specific settings cannot be found for <" + platformName + ">. Using global settings.");
		return instance;
	}

	private void OnEnable()
	{
		if (m_Instance == null)
		{
			m_Instance = this;
		}
		else if (m_Instance != this)
		{
			Debug.LogWarning("WwiseUnity: There are multiple AkWwiseInitializationSettings objects instantiated; only one will be used.");
		}
	}

	public static bool InitializeSoundEngine()
	{
		Debug.LogFormat("WwiseUnity: Wwise(R) SDK Version {0}.", AkSoundEngine.WwiseVersion);
		if (AkSoundEngine.Init(ActivePlatformSettings.AkInitializationSettings) != AKRESULT.AK_Success)
		{
			Debug.LogError("WwiseUnity: Failed to initialize the sound engine. Abort.");
			AkSoundEngine.Term();
			return false;
		}
		if (AkSoundEngine.InitSpatialAudio(ActivePlatformSettings.AkSpatialAudioInitSettings) != AKRESULT.AK_Success)
		{
			Debug.LogWarning("WwiseUnity: Failed to initialize spatial audio.");
		}
		AkSoundEngine.InitCommunication(ActivePlatformSettings.AkCommunicationSettings);
		AkBasePathGetter.EvaluateGamePaths();
		string soundBankBasePath = AkBasePathGetter.SoundBankBasePath;
		if (string.IsNullOrEmpty(soundBankBasePath))
		{
			Debug.LogError("WwiseUnity: Couldn't find SoundBanks base path. Terminating sound engine.");
			AkSoundEngine.Term();
			return false;
		}
		string persistentDataPath = AkBasePathGetter.PersistentDataPath;
		bool num = soundBankBasePath == persistentDataPath;
		bool flag = true;
		bool flag2 = !num;
		if (flag && AkSoundEngine.SetBasePath(soundBankBasePath) != AKRESULT.AK_Success)
		{
			Debug.LogErrorFormat("WwiseUnity: Failed to set SoundBanks base path to <{0}>. Make sure SoundBank path is correctly set under Edit > Project Settings > Wwise > Initialization.", soundBankBasePath);
			AkSoundEngine.Term();
			return false;
		}
		if (flag2 && !string.IsNullOrEmpty(persistentDataPath))
		{
			AkSoundEngine.AddBasePath(persistentDataPath);
		}
		string decodedBankFullPath = AkBasePathGetter.DecodedBankFullPath;
		if (!string.IsNullOrEmpty(decodedBankFullPath))
		{
			AkSoundEngine.SetDecodedBankPath(decodedBankFullPath);
			AkSoundEngine.AddBasePath(decodedBankFullPath);
		}
		AkSoundEngine.SetCurrentLanguage(ActivePlatformSettings.InitialLanguage);
		AkCallbackManager.Init(ActivePlatformSettings.CallbackManagerInitializationSettings);
		Debug.Log("WwiseUnity: Sound engine initialized successfully.");
		AkBankManager.LoadInitBank();
		return true;
	}

	public static bool ResetSoundEngine(bool isPlaying)
	{
		if (isPlaying)
		{
			AkSoundEngine.ClearBanks();
			AkBankManager.LoadInitBank();
		}
		AkCallbackManager.Init(ActivePlatformSettings.CallbackManagerInitializationSettings);
		return true;
	}

	public static void TerminateSoundEngine()
	{
		if (!AkSoundEngine.IsInitialized())
		{
			return;
		}
		AkSoundEngine.StopAll();
		AkSoundEngine.ClearBanks();
		AkSoundEngine.RenderAudio();
		int num = 0;
		while (num < 5)
		{
			if (AkCallbackManager.PostCallbacks() == 0)
			{
				SleepForMilliseconds(10.0);
				num++;
			}
			SleepForMilliseconds(1.0);
		}
		AkSoundEngine.Term();
		AkCallbackManager.PostCallbacks();
		AkCallbackManager.Term();
		AkBankManager.Reset();
	}

	private static void SleepForMilliseconds(double milliseconds)
	{
		using ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
		manualResetEvent.WaitOne(TimeSpan.FromMilliseconds(milliseconds));
	}
}
