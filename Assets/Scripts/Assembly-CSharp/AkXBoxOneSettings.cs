using System;
using UnityEngine;

public class AkXBoxOneSettings : AkWwiseInitializationSettings.PlatformSettings
{
	[Serializable]
	public class PlatformAdvancedSettings : AkCommonAdvancedSettings
	{
		[Tooltip("Maximum number of hardware-accelerated XMA voices used at run-time. Default is 128 voices.")]
		public ushort MaximumNumberOfXMAVoices = 128;

		[Tooltip("Use low latency mode for hardware XMA decoding (default is false). If true, decoding jobs are submitted at the beginning of the Wwise update and it will be necessary to wait for the result.")]
		public bool UseHardwareCodecLowLatencyMode;

		[Tooltip("APU heap cached size sent to the \"ApuCreateHeap()\" function.")]
		public uint APUHeapCachedSize = 67108864u;

		[Tooltip("APU heap non-cached size sent to the \"ApuCreateHeap()\" function.")]
		public uint APUHeapNonCachedSize;

		public override void CopyTo(AkPlatformInitSettings settings)
		{
		}

		public override void CopyTo(AkUnityPlatformSpecificSettings settings)
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
	public AkCommonCommSettings CommsSettings = new AkCommonCommSettings
	{
		m_DiscoveryBroadcastPort = AkCommonCommSettings.DefaultDiscoveryBroadcastPort,
		m_CommandPort = (ushort)(AkCommonCommSettings.DefaultDiscoveryBroadcastPort + 1),
		m_NotificationPort = (ushort)(AkCommonCommSettings.DefaultDiscoveryBroadcastPort + 2)
	};

	public AkXBoxOneSettings()
	{
		IgnorePropertyValue("UserSettings.m_SampleRate");
		SetUseGlobalPropertyValue("CommsSettings.m_CommandPort", use: false);
		SetUseGlobalPropertyValue("CommsSettings.m_NotificationPort", use: false);
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
