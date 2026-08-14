using System;

public class AkDurationCallbackInfo : AkEventCallbackInfo
{
	private IntPtr swigCPtr;

	public float fDuration => AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_fDuration_get(swigCPtr);

	public float fEstimatedDuration => AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_fEstimatedDuration_get(swigCPtr);

	public uint audioNodeID => AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_audioNodeID_get(swigCPtr);

	public uint mediaID => AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_mediaID_get(swigCPtr);

	public bool bStreaming => AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_bStreaming_get(swigCPtr);

	internal AkDurationCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkDurationCallbackInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkDurationCallbackInfo()
	{
		Dispose();
	}

	public override void Dispose()
	{
		lock (this)
		{
			if (swigCPtr != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					AkSoundEnginePINVOKE.CSharp_delete_AkDurationCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public AkDurationCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkDurationCallbackInfo(), cMemoryOwn: true)
	{
	}
}
