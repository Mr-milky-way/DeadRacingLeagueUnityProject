using System;
using UnityEngine;

public class AkPS4Settings : AkWwiseInitializationSettings.PlatformSettings
{
	[Serializable]
	public class PlatformAdvancedSettings : AkCommonAdvancedSettings
	{
		[Tooltip("ACP batch buffer size used for ATRAC9 decoding.")]
		public uint ACPBatchBufferSize = 92160u;

		[Tooltip("Use low latency mode for ATRAC9 (default is false). If true, decoding jobs are submitted at the beginning of the Wwise update and it will be necessary to wait for the result.")]
		public bool UseHardwareCodecLowLatencyMode;

		public override void CopyTo(AkPlatformInitSettings settings)
		{
		}
	}

	[HideInInspector]
	public AkCommonUserSettings UserSettings = new AkCommonUserSettings
	{
		m_SamplesPerFrame = 512u
	};

	[HideInInspector]
	public PlatformAdvancedSettings AdvancedSettings = new PlatformAdvancedSettings
	{
		m_RenderDuringFocusLoss = true
	};

	[HideInInspector]
	public AkCommonCommSettings CommsSettings;

	public AkPS4Settings()
	{
		IgnorePropertyValue("UserSettings.m_SampleRate");
		SetUseGlobalPropertyValue("AdvancedSettings.m_RenderDuringFocusLoss", use: false);
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
}
