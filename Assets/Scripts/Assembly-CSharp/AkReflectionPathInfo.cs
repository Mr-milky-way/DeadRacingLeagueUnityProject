using System;
using UnityEngine;

public class AkReflectionPathInfo : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public Vector3 imageSource
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_imageSource_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_imageSource_set(swigCPtr, value);
		}
	}

	public uint numPathPoints
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_numPathPoints_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_numPathPoints_set(swigCPtr, value);
		}
	}

	public uint numReflections
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_numReflections_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_numReflections_set(swigCPtr, value);
		}
	}

	public float level
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_level_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_level_set(swigCPtr, value);
		}
	}

	public bool isOccluded
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_isOccluded_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_isOccluded_set(swigCPtr, value);
		}
	}

	internal AkReflectionPathInfo(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkReflectionPathInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkReflectionPathInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkReflectionPathInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public static int GetSizeOf()
	{
		return AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_GetSizeOf();
	}

	public Vector3 GetPathPoint(uint idx)
	{
		return AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_GetPathPoint(swigCPtr, idx);
	}

	public AkAcousticSurface GetAcousticSurface(uint idx)
	{
		return new AkAcousticSurface(AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_GetAcousticSurface(swigCPtr, idx), cMemoryOwn: false);
	}

	public float GetDiffraction(uint idx)
	{
		return AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_GetDiffraction(swigCPtr, idx);
	}

	public void Clone(AkReflectionPathInfo other)
	{
		AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_Clone(swigCPtr, getCPtr(other));
	}

	public AkReflectionPathInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkReflectionPathInfo(), cMemoryOwn: true)
	{
	}
}
