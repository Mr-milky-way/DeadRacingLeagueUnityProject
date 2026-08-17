public abstract class AkCommonPlatformSettings : AkBasePlatformSettings
{
	public override AkInitializationSettings AkInitializationSettings
	{
		get
		{
			AkInitializationSettings akInitializationSettings = base.AkInitializationSettings;
			AkCommonUserSettings userSettings = GetUserSettings();
			userSettings.CopyTo(akInitializationSettings.deviceSettings);
			userSettings.CopyTo(akInitializationSettings.streamMgrSettings);
			userSettings.CopyTo(akInitializationSettings.initSettings);
			userSettings.CopyTo(akInitializationSettings.platformSettings);
			userSettings.CopyTo(akInitializationSettings.musicSettings);
			userSettings.CopyTo(akInitializationSettings.unityPlatformSpecificSettings);
			AkCommonAdvancedSettings advancedSettings = GetAdvancedSettings();
			advancedSettings.CopyTo(akInitializationSettings.deviceSettings);
			advancedSettings.CopyTo(akInitializationSettings.initSettings);
			advancedSettings.CopyTo(akInitializationSettings.platformSettings);
			advancedSettings.CopyTo(akInitializationSettings.unityPlatformSpecificSettings);
			akInitializationSettings.useAsyncOpen = advancedSettings.m_UseAsyncOpen;
			return akInitializationSettings;
		}
	}

	public override AkSpatialAudioInitSettings AkSpatialAudioInitSettings
	{
		get
		{
			AkSpatialAudioInitSettings akSpatialAudioInitSettings = base.AkSpatialAudioInitSettings;
			GetUserSettings().CopyTo(akSpatialAudioInitSettings);
			GetAdvancedSettings().CopyTo(akSpatialAudioInitSettings);
			return akSpatialAudioInitSettings;
		}
	}

	public override AkCallbackManager.InitializationSettings CallbackManagerInitializationSettings
	{
		get
		{
			AkCommonUserSettings userSettings = GetUserSettings();
			return new AkCallbackManager.InitializationSettings
			{
				BufferSize = userSettings.m_CallbackManagerBufferSize,
				IsLoggingEnabled = userSettings.m_EngineLogging
			};
		}
	}

	public override string InitialLanguage => GetUserSettings().m_StartupLanguage;

	public override string SoundBankPersistentDataPath => GetAdvancedSettings().m_SoundBankPersistentDataPath;

	public override bool RenderDuringFocusLoss
	{
		get
		{
			AkCommonAdvancedSettings advancedSettings = GetAdvancedSettings();
			return advancedSettings != null && advancedSettings.m_RenderDuringFocusLoss;
		}
	}

	public override string SoundbankPath => GetUserSettings().m_BasePath;

	public override bool UseAsyncOpen => GetAdvancedSettings().m_UseAsyncOpen;

	public override AkCommunicationSettings AkCommunicationSettings
	{
		get
		{
			AkCommunicationSettings akCommunicationSettings = base.AkCommunicationSettings;
			GetCommsSettings().CopyTo(akCommunicationSettings);
			return akCommunicationSettings;
		}
	}

	protected abstract AkCommonUserSettings GetUserSettings();

	protected abstract AkCommonAdvancedSettings GetAdvancedSettings();

	protected abstract AkCommonCommSettings GetCommsSettings();
}
