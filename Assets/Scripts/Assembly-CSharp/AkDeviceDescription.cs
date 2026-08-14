using System;

public class AkDeviceDescription : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint idDevice
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_idDevice_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_idDevice_set(swigCPtr, value);
		}
	}

	public string deviceName
	{
		get
		{
			return AkSoundEngine.StringFromIntPtrOSString(AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_deviceName_get(swigCPtr));
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_deviceName_set(swigCPtr, value);
		}
	}

	public AkAudioDeviceState deviceStateMask
	{
		get
		{
			return (AkAudioDeviceState)AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_deviceStateMask_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_deviceStateMask_set(swigCPtr, (int)value);
		}
	}

	public bool isDefaultDevice
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_isDefaultDevice_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_isDefaultDevice_set(swigCPtr, value);
		}
	}

	internal AkDeviceDescription(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkDeviceDescription obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkDeviceDescription()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkDeviceDescription(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public void Clear()
	{
		AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_Clear(swigCPtr);
	}

	public static int GetSizeOf()
	{
		return AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_GetSizeOf();
	}

	public void Clone(AkDeviceDescription other)
	{
		AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_Clone(swigCPtr, getCPtr(other));
	}

	public AkDeviceDescription()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkDeviceDescription(), cMemoryOwn: true)
	{
	}
}
