using System;

public class AkSourceSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint sourceID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSourceSettings_sourceID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSourceSettings_sourceID_set(swigCPtr, value);
		}
	}

	public IntPtr pMediaMemory
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSourceSettings_pMediaMemory_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSourceSettings_pMediaMemory_set(swigCPtr, value);
		}
	}

	public uint uMediaSize
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSourceSettings_uMediaSize_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSourceSettings_uMediaSize_set(swigCPtr, value);
		}
	}

	internal AkSourceSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkSourceSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkSourceSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkSourceSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public void Clear()
	{
		AkSoundEnginePINVOKE.CSharp_AkSourceSettings_Clear(swigCPtr);
	}

	public static int GetSizeOf()
	{
		return AkSoundEnginePINVOKE.CSharp_AkSourceSettings_GetSizeOf();
	}

	public void Clone(AkSourceSettings other)
	{
		AkSoundEnginePINVOKE.CSharp_AkSourceSettings_Clone(swigCPtr, getCPtr(other));
	}

	public AkSourceSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkSourceSettings(), cMemoryOwn: true)
	{
	}
}
