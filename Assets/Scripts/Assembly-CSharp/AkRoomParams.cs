using System;
using UnityEngine;

public class AkRoomParams : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public Vector3 Up
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_Up_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_Up_set(swigCPtr, value);
		}
	}

	public Vector3 Front
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_Front_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_Front_set(swigCPtr, value);
		}
	}

	public uint ReverbAuxBus
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_ReverbAuxBus_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_ReverbAuxBus_set(swigCPtr, value);
		}
	}

	public float ReverbLevel
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_ReverbLevel_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_ReverbLevel_set(swigCPtr, value);
		}
	}

	public float WallOcclusion
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_WallOcclusion_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_WallOcclusion_set(swigCPtr, value);
		}
	}

	public float RoomGameObj_AuxSendLevelToSelf
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_RoomGameObj_AuxSendLevelToSelf_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_RoomGameObj_AuxSendLevelToSelf_set(swigCPtr, value);
		}
	}

	public bool RoomGameObj_KeepRegistered
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_RoomGameObj_KeepRegistered_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_RoomGameObj_KeepRegistered_set(swigCPtr, value);
		}
	}

	internal AkRoomParams(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkRoomParams obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkRoomParams()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkRoomParams(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkRoomParams()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkRoomParams(), cMemoryOwn: true)
	{
	}
}
