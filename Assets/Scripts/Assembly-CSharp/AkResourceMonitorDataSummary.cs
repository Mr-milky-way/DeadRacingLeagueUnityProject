using System;

public class AkResourceMonitorDataSummary : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public float totalCPU
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkResourceMonitorDataSummary_totalCPU_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkResourceMonitorDataSummary_totalCPU_set(swigCPtr, value);
		}
	}

	public float pluginCPU
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkResourceMonitorDataSummary_pluginCPU_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkResourceMonitorDataSummary_pluginCPU_set(swigCPtr, value);
		}
	}

	public uint physicalVoices
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkResourceMonitorDataSummary_physicalVoices_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkResourceMonitorDataSummary_physicalVoices_set(swigCPtr, value);
		}
	}

	public uint virtualVoices
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkResourceMonitorDataSummary_virtualVoices_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkResourceMonitorDataSummary_virtualVoices_set(swigCPtr, value);
		}
	}

	public uint totalVoices
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkResourceMonitorDataSummary_totalVoices_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkResourceMonitorDataSummary_totalVoices_set(swigCPtr, value);
		}
	}

	public uint nbActiveEvents
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkResourceMonitorDataSummary_nbActiveEvents_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkResourceMonitorDataSummary_nbActiveEvents_set(swigCPtr, value);
		}
	}

	internal AkResourceMonitorDataSummary(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkResourceMonitorDataSummary obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkResourceMonitorDataSummary()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkResourceMonitorDataSummary(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkResourceMonitorDataSummary()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkResourceMonitorDataSummary(), cMemoryOwn: true)
	{
	}
}
