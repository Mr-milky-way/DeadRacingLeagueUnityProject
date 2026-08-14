using System;

public class AkOutputSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint audioDeviceShareset
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkOutputSettings_audioDeviceShareset_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkOutputSettings_audioDeviceShareset_set(swigCPtr, value);
		}
	}

	public uint idDevice
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkOutputSettings_idDevice_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkOutputSettings_idDevice_set(swigCPtr, value);
		}
	}

	public AkPanningRule ePanningRule
	{
		get
		{
			return (AkPanningRule)AkSoundEnginePINVOKE.CSharp_AkOutputSettings_ePanningRule_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkOutputSettings_ePanningRule_set(swigCPtr, (int)value);
		}
	}

	public AkChannelConfig channelConfig
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkOutputSettings_channelConfig_get(swigCPtr);
			if (!(intPtr == IntPtr.Zero))
			{
				return new AkChannelConfig(intPtr, cMemoryOwn: false);
			}
			return null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkOutputSettings_channelConfig_set(swigCPtr, AkChannelConfig.getCPtr(value));
		}
	}

	internal AkOutputSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkOutputSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkOutputSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkOutputSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkOutputSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkOutputSettings__SWIG_0(), cMemoryOwn: true)
	{
	}

	public AkOutputSettings(string in_szDeviceShareSet, uint in_idDevice, AkChannelConfig in_channelConfig, AkPanningRule in_ePanning)
		: this(AkSoundEnginePINVOKE.CSharp_new_AkOutputSettings__SWIG_1(in_szDeviceShareSet, in_idDevice, AkChannelConfig.getCPtr(in_channelConfig), (int)in_ePanning), cMemoryOwn: true)
	{
	}

	public AkOutputSettings(string in_szDeviceShareSet, uint in_idDevice, AkChannelConfig in_channelConfig)
		: this(AkSoundEnginePINVOKE.CSharp_new_AkOutputSettings__SWIG_2(in_szDeviceShareSet, in_idDevice, AkChannelConfig.getCPtr(in_channelConfig)), cMemoryOwn: true)
	{
	}

	public AkOutputSettings(string in_szDeviceShareSet, uint in_idDevice)
		: this(AkSoundEnginePINVOKE.CSharp_new_AkOutputSettings__SWIG_3(in_szDeviceShareSet, in_idDevice), cMemoryOwn: true)
	{
	}

	public AkOutputSettings(string in_szDeviceShareSet)
		: this(AkSoundEnginePINVOKE.CSharp_new_AkOutputSettings__SWIG_4(in_szDeviceShareSet), cMemoryOwn: true)
	{
	}
}
