using System;

public class AkSpatialAudioInitSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint uMaxSoundPropagationDepth
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uMaxSoundPropagationDepth_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uMaxSoundPropagationDepth_set(swigCPtr, value);
		}
	}

	public uint uDiffractionFlags
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uDiffractionFlags_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uDiffractionFlags_set(swigCPtr, value);
		}
	}

	public float fDiffractionShadowAttenFactor
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_fDiffractionShadowAttenFactor_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_fDiffractionShadowAttenFactor_set(swigCPtr, value);
		}
	}

	public float fDiffractionShadowDegrees
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_fDiffractionShadowDegrees_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_fDiffractionShadowDegrees_set(swigCPtr, value);
		}
	}

	public float fMovementThreshold
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_fMovementThreshold_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_fMovementThreshold_set(swigCPtr, value);
		}
	}

	public uint uNumberOfPrimaryRays
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uNumberOfPrimaryRays_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uNumberOfPrimaryRays_set(swigCPtr, value);
		}
	}

	public uint uMaxReflectionOrder
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uMaxReflectionOrder_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uMaxReflectionOrder_set(swigCPtr, value);
		}
	}

	public float fMaxPathLength
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_fMaxPathLength_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_fMaxPathLength_set(swigCPtr, value);
		}
	}

	public bool bEnableDiffractionOnReflection
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_bEnableDiffractionOnReflection_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_bEnableDiffractionOnReflection_set(swigCPtr, value);
		}
	}

	public bool bEnableDirectPathDiffraction
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_bEnableDirectPathDiffraction_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_bEnableDirectPathDiffraction_set(swigCPtr, value);
		}
	}

	public bool bEnableTransmission
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_bEnableTransmission_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_bEnableTransmission_set(swigCPtr, value);
		}
	}

	internal AkSpatialAudioInitSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkSpatialAudioInitSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkSpatialAudioInitSettings()
	{
		Dispose();
	}

	public virtual void Dispose()
	{
		lock (this)
		{
			if (swigCPtr != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					AkSoundEnginePINVOKE.CSharp_delete_AkSpatialAudioInitSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkSpatialAudioInitSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkSpatialAudioInitSettings(), cMemoryOwn: true)
	{
	}
}
