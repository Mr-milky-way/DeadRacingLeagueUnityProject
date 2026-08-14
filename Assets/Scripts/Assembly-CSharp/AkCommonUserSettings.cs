using System;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class AkCommonUserSettings
{
	[Serializable]
	public class SpatialAudioSettings
	{
		public enum DiffractionFlags
		{
			UseBuiltInParam = 1,
			UseObstruction = 2,
			CalcEmitterVirtualPosition = 8
		}

		[Tooltip("Maximum number of portals that sound can propagate through.")]
		[Range(0f, 8f)]
		public uint m_MaxSoundPropagationDepth = 8u;

		[Tooltip("Determines whether diffraction values for sound passing through portals will be calculated, and how to apply those calculations to Wwise parameters.")]
		[AkEnumFlag(typeof(DiffractionFlags))]
		public DiffractionFlags m_DiffractionFlags = (DiffractionFlags)(-1);

		[Tooltip("Distance (in game units) that an emitter or listener has to move to trigger a recalculation of reflections/diffraction. Larger values can reduce the CPU load at the cost of reduced accuracy.")]
		public float m_MovementThreshold = 1f;

		[Tooltip("The number of primary rays used in stochastic ray casting.")]
		public uint m_NumberOfPrimaryRays = 100u;

		[Range(0f, 4f)]
		[Tooltip("The maximum number of reflections that will be processed for a sound path before it reaches the listener.")]
		[FormerlySerializedAs("m_ReflectionsOrder")]
		public uint m_MaxReflectionOrder = 1u;

		[Tooltip("Length of the rays that are cast inside Spatial Audio. Effectively caps the maximum length of an individual segment in a reflection or diffraction path.")]
		public float m_MaxPathLength = 10000f;

		[Tooltip("Enable computation of diffraction along reflection paths.")]
		[FormerlySerializedAs("m_EnableDiffraction")]
		public bool m_EnableDiffractionOnReflections = true;

		[Tooltip("Enable direct path diffraction. Diffraction must be enabled for a source in the authoring tool positioning tab.")]
		public bool m_EnableDirectPathDiffraction = true;

		[Tooltip("Enable modeling of transmission of sound through walls.")]
		public bool m_EnableTransmission = true;
	}

	[Tooltip("Path for the SoundBanks. This must contain one sub folder per platform, with the same as in the Wwise project.")]
	public string m_BasePath = AkBasePathGetter.DefaultBasePath;

	[Tooltip("Language sub-folder used at startup.")]
	public string m_StartupLanguage = "English(US)";

	[Tooltip("CallbackManager buffer size. The size of the buffer used per-frame to transfer callback data. Default size is 4 KB, but you should increase this, if required.")]
	public int m_CallbackManagerBufferSize = AkCallbackManager.InitializationSettings.DefaultBufferSize;

	[Tooltip("Enable Wwise engine logging. This is used to turn on/off the logging of the Wwise engine.")]
	public bool m_EngineLogging = AkCallbackManager.InitializationSettings.DefaultIsLoggingEnabled;

	[Tooltip("Maximum number of automation paths for positioning sounds.")]
	public uint m_MaximumNumberOfPositioningPaths = 255u;

	[Tooltip("Size of the command queue.")]
	public uint m_CommandQueueSize = 262144u;

	[Tooltip("Number of samples per audio frame (256, 512, 1024, or 2048).")]
	public uint m_SamplesPerFrame = 1024u;

	[Tooltip("Main output device settings.")]
	public AkCommonOutputSettings m_MainOutputSettings;

	[Tooltip("Multiplication factor for all streaming look-ahead heuristic values.")]
	[Range(0f, 1f)]
	public float m_StreamingLookAheadRatio = 1f;

	[Tooltip("Sampling Rate. Default is 48000 Hz. Use 24000hz for low quality. Any positive reasonable sample rate is supported; however, be careful setting a custom value. Using an odd or really low sample rate may cause the sound engine to malfunction.")]
	public uint m_SampleRate = 48000u;

	[Tooltip("Number of refill buffers in voice buffer. Set to 2 for double-buffered, defaults to 4.")]
	public ushort m_NumberOfRefillsInVoice = 4;

	[Tooltip("Spatial audio common settings.")]
	public SpatialAudioSettings m_SpatialAudioSettings;

	protected static string GetPluginPath()
	{
		string text = Path.Combine(Application.dataPath, "Plugins" + Path.DirectorySeparatorChar);
		string text2 = "x86";
		text2 += "_64";
		if (File.Exists(Path.Combine(text, "AkSoundEngine.dll")))
		{
			return text;
		}
		if (File.Exists(Path.Combine(text, text2, "AkSoundEngine.dll")))
		{
			return Path.Combine(text, text2);
		}
		Debug.Log("Cannot find Wwise plugin path");
		return null;
	}

	public virtual void CopyTo(AkInitSettings settings)
	{
		settings.uMaxNumPaths = m_MaximumNumberOfPositioningPaths;
		settings.uCommandQueueSize = m_CommandQueueSize;
		settings.uNumSamplesPerFrame = m_SamplesPerFrame;
		m_MainOutputSettings.CopyTo(settings.settingsMainOutput);
		settings.szPluginDLLPath = GetPluginPath();
		Debug.Log("WwiseUnity: Setting Plugin DLL path to: " + ((settings.szPluginDLLPath == null) ? "NULL" : settings.szPluginDLLPath));
	}

	public void CopyTo(AkMusicSettings settings)
	{
		settings.fStreamingLookAheadRatio = m_StreamingLookAheadRatio;
	}

	public void CopyTo(AkStreamMgrSettings settings)
	{
	}

	public virtual void CopyTo(AkDeviceSettings settings)
	{
	}

	public virtual void CopyTo(AkPlatformInitSettings settings)
	{
		settings.uSampleRate = m_SampleRate;
		settings.uNumRefillsInVoice = m_NumberOfRefillsInVoice;
	}

	public virtual void CopyTo(AkSpatialAudioInitSettings settings)
	{
		settings.uMaxSoundPropagationDepth = m_SpatialAudioSettings.m_MaxSoundPropagationDepth;
		settings.uDiffractionFlags = (uint)m_SpatialAudioSettings.m_DiffractionFlags;
		settings.fMovementThreshold = m_SpatialAudioSettings.m_MovementThreshold;
		settings.uNumberOfPrimaryRays = m_SpatialAudioSettings.m_NumberOfPrimaryRays;
		settings.uMaxReflectionOrder = m_SpatialAudioSettings.m_MaxReflectionOrder;
		settings.fMaxPathLength = m_SpatialAudioSettings.m_MaxPathLength;
		settings.bEnableDiffractionOnReflection = m_SpatialAudioSettings.m_EnableDiffractionOnReflections;
		settings.bEnableDirectPathDiffraction = m_SpatialAudioSettings.m_EnableDirectPathDiffraction;
		settings.bEnableTransmission = m_SpatialAudioSettings.m_EnableTransmission;
	}

	public virtual void CopyTo(AkUnityPlatformSpecificSettings settings)
	{
	}

	public virtual void Validate()
	{
		if (m_SpatialAudioSettings.m_MovementThreshold < 0f)
		{
			m_SpatialAudioSettings.m_MovementThreshold = 0f;
		}
		if (m_SpatialAudioSettings.m_MaxPathLength < 0f)
		{
			m_SpatialAudioSettings.m_MaxPathLength = 0f;
		}
	}
}
