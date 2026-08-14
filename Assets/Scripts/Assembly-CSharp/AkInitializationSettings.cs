using System;

public class AkInitializationSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkStreamMgrSettings streamMgrSettings
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_streamMgrSettings_get(swigCPtr);
			if (!(intPtr == IntPtr.Zero))
			{
				return new AkStreamMgrSettings(intPtr, cMemoryOwn: false);
			}
			return null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_streamMgrSettings_set(swigCPtr, AkStreamMgrSettings.getCPtr(value));
		}
	}

	public AkDeviceSettings deviceSettings
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_deviceSettings_get(swigCPtr);
			if (!(intPtr == IntPtr.Zero))
			{
				return new AkDeviceSettings(intPtr, cMemoryOwn: false);
			}
			return null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_deviceSettings_set(swigCPtr, AkDeviceSettings.getCPtr(value));
		}
	}

	public AkInitSettings initSettings
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_initSettings_get(swigCPtr);
			if (!(intPtr == IntPtr.Zero))
			{
				return new AkInitSettings(intPtr, cMemoryOwn: false);
			}
			return null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_initSettings_set(swigCPtr, AkInitSettings.getCPtr(value));
		}
	}

	public AkPlatformInitSettings platformSettings
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_platformSettings_get(swigCPtr);
			if (!(intPtr == IntPtr.Zero))
			{
				return new AkPlatformInitSettings(intPtr, cMemoryOwn: false);
			}
			return null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_platformSettings_set(swigCPtr, AkPlatformInitSettings.getCPtr(value));
		}
	}

	public AkMusicSettings musicSettings
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_musicSettings_get(swigCPtr);
			if (!(intPtr == IntPtr.Zero))
			{
				return new AkMusicSettings(intPtr, cMemoryOwn: false);
			}
			return null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_musicSettings_set(swigCPtr, AkMusicSettings.getCPtr(value));
		}
	}

	public AkUnityPlatformSpecificSettings unityPlatformSpecificSettings
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_unityPlatformSpecificSettings_get(swigCPtr);
			if (!(intPtr == IntPtr.Zero))
			{
				return new AkUnityPlatformSpecificSettings(intPtr, cMemoryOwn: false);
			}
			return null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_unityPlatformSpecificSettings_set(swigCPtr, AkUnityPlatformSpecificSettings.getCPtr(value));
		}
	}

	public bool useAsyncOpen
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_useAsyncOpen_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkInitializationSettings_useAsyncOpen_set(swigCPtr, value);
		}
	}

	internal AkInitializationSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkInitializationSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkInitializationSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkInitializationSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkInitializationSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkInitializationSettings(), cMemoryOwn: true)
	{
	}
}
