using System;

public class AkAudioInterruptionCallbackInfo : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public bool bEnterInterruption => AkSoundEnginePINVOKE.CSharp_AkAudioInterruptionCallbackInfo_bEnterInterruption_get(swigCPtr);

	internal AkAudioInterruptionCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkAudioInterruptionCallbackInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkAudioInterruptionCallbackInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkAudioInterruptionCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkAudioInterruptionCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkAudioInterruptionCallbackInfo(), cMemoryOwn: true)
	{
	}
}
