using System;
using UnityEngine;

public class AkImageSourceSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkImageSourceParams params_
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkImageSourceSettings_params__get(swigCPtr);
			if (!(intPtr == IntPtr.Zero))
			{
				return new AkImageSourceParams(intPtr, cMemoryOwn: false);
			}
			return null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkImageSourceSettings_params__set(swigCPtr, AkImageSourceParams.getCPtr(value));
		}
	}

	internal AkImageSourceSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkImageSourceSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkImageSourceSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkImageSourceSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkImageSourceSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkImageSourceSettings__SWIG_0(), cMemoryOwn: true)
	{
	}

	public AkImageSourceSettings(Vector3 in_sourcePosition, float in_fDistanceScalingFactor, float in_fLevel)
		: this(AkSoundEnginePINVOKE.CSharp_new_AkImageSourceSettings__SWIG_1(in_sourcePosition, in_fDistanceScalingFactor, in_fLevel), cMemoryOwn: true)
	{
	}

	public void SetOneTexture(uint in_texture)
	{
		AkSoundEnginePINVOKE.CSharp_AkImageSourceSettings_SetOneTexture(swigCPtr, in_texture);
	}

	public void SetName(string in_pName)
	{
		AkSoundEnginePINVOKE.CSharp_AkImageSourceSettings_SetName(swigCPtr, in_pName);
	}
}
