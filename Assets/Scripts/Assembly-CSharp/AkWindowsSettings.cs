using System;
using UnityEngine;

public class AkWindowsSettings : AkWwiseInitializationSettings.PlatformSettings
{
	[Serializable]
	public class PlatformAdvancedSettings : AkCommonAdvancedSettings
	{
		public enum AudioAPI
		{
			None = 0,
			Wasapi = 1,
			XAudio2 = 2,
			DirectSound = 4,
			Default = -1
		}

		[Tooltip("Main audio API to use. Leave set to \"Default\" for the default audio sink. This value will be ignored if a valid \"AudioDeviceShareset\" is provided.")]
		[AkEnumFlag(typeof(AudioAPI))]
		public AudioAPI m_AudioAPI = AudioAPI.Default;

		[Tooltip("Only used when \"AudioAPI\" is \"DirectSound\", sounds will be muted if set to false when the game loses the focus.")]
		public bool m_GlobalFocus = true;

		public override void CopyTo(AkPlatformInitSettings settings)
		{
			settings.eAudioAPI = (AkAudioAPI)m_AudioAPI;
			settings.bGlobalFocus = m_GlobalFocus;
		}
	}

	[HideInInspector]
	public AkCommonUserSettings UserSettings;

	[HideInInspector]
	public PlatformAdvancedSettings AdvancedSettings;

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
