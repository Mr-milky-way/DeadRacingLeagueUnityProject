using System;

public class AkCommunicationSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint uPoolSize
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkCommunicationSettings_uPoolSize_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkCommunicationSettings_uPoolSize_set(swigCPtr, value);
		}
	}

	public ushort uDiscoveryBroadcastPort
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkCommunicationSettings_uDiscoveryBroadcastPort_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkCommunicationSettings_uDiscoveryBroadcastPort_set(swigCPtr, value);
		}
	}

	public ushort uCommandPort
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkCommunicationSettings_uCommandPort_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkCommunicationSettings_uCommandPort_set(swigCPtr, value);
		}
	}

	public ushort uNotificationPort
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkCommunicationSettings_uNotificationPort_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkCommunicationSettings_uNotificationPort_set(swigCPtr, value);
		}
	}

	public bool bInitSystemLib
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkCommunicationSettings_bInitSystemLib_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkCommunicationSettings_bInitSystemLib_set(swigCPtr, value);
		}
	}

	public string szAppNetworkName
	{
		get
		{
			return AkSoundEngine.StringFromIntPtrString(AkSoundEnginePINVOKE.CSharp_AkCommunicationSettings_szAppNetworkName_get(swigCPtr));
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkCommunicationSettings_szAppNetworkName_set(swigCPtr, value);
		}
	}

	internal AkCommunicationSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkCommunicationSettings obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkCommunicationSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkCommunicationSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkCommunicationSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkCommunicationSettings(), cMemoryOwn: true)
	{
	}
}
