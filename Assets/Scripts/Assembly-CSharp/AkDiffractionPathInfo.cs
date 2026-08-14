using System;
using UnityEngine;

public class AkDiffractionPathInfo : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public const uint kMaxNodes = 8u;

	public AkTransform virtualPos
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_virtualPos_get(swigCPtr);
			if (!(intPtr == IntPtr.Zero))
			{
				return new AkTransform(intPtr, cMemoryOwn: false);
			}
			return null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_virtualPos_set(swigCPtr, AkTransform.getCPtr(value));
		}
	}

	public uint nodeCount
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_nodeCount_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_nodeCount_set(swigCPtr, value);
		}
	}

	public float diffraction
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_diffraction_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_diffraction_set(swigCPtr, value);
		}
	}

	public float totLength
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_totLength_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_totLength_set(swigCPtr, value);
		}
	}

	public float obstructionValue
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_obstructionValue_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_obstructionValue_set(swigCPtr, value);
		}
	}

	internal AkDiffractionPathInfo(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkDiffractionPathInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkDiffractionPathInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkDiffractionPathInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public static int GetSizeOf()
	{
		return AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_GetSizeOf();
	}

	public Vector3 GetNodes(uint idx)
	{
		return AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_GetNodes(swigCPtr, idx);
	}

	public float GetAngles(uint idx)
	{
		return AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_GetAngles(swigCPtr, idx);
	}

	public ulong GetPortals(uint idx)
	{
		return AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_GetPortals(swigCPtr, idx);
	}

	public ulong GetRooms(uint idx)
	{
		return AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_GetRooms(swigCPtr, idx);
	}

	public void Clone(AkDiffractionPathInfo other)
	{
		AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_Clone(swigCPtr, getCPtr(other));
	}

	public AkDiffractionPathInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkDiffractionPathInfo(), cMemoryOwn: true)
	{
	}
}
