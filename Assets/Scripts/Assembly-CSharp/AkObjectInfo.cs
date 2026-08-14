using System;

public class AkObjectInfo : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint objID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkObjectInfo_objID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkObjectInfo_objID_set(swigCPtr, value);
		}
	}

	public uint parentID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkObjectInfo_parentID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkObjectInfo_parentID_set(swigCPtr, value);
		}
	}

	public int iDepth
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkObjectInfo_iDepth_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkObjectInfo_iDepth_set(swigCPtr, value);
		}
	}

	internal AkObjectInfo(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkObjectInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkObjectInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkObjectInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public void Clear()
	{
		AkSoundEnginePINVOKE.CSharp_AkObjectInfo_Clear(swigCPtr);
	}

	public static int GetSizeOf()
	{
		return AkSoundEnginePINVOKE.CSharp_AkObjectInfo_GetSizeOf();
	}

	public void Clone(AkObjectInfo other)
	{
		AkSoundEnginePINVOKE.CSharp_AkObjectInfo_Clone(swigCPtr, getCPtr(other));
	}

	public AkObjectInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkObjectInfo(), cMemoryOwn: true)
	{
	}
}
