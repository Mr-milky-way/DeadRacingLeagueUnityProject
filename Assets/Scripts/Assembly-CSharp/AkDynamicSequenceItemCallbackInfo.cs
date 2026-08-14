using System;

public class AkDynamicSequenceItemCallbackInfo : AkCallbackInfo
{
	private IntPtr swigCPtr;

	public uint playingID => AkSoundEnginePINVOKE.CSharp_AkDynamicSequenceItemCallbackInfo_playingID_get(swigCPtr);

	public uint audioNodeID => AkSoundEnginePINVOKE.CSharp_AkDynamicSequenceItemCallbackInfo_audioNodeID_get(swigCPtr);

	public IntPtr pCustomInfo => AkSoundEnginePINVOKE.CSharp_AkDynamicSequenceItemCallbackInfo_pCustomInfo_get(swigCPtr);

	internal AkDynamicSequenceItemCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkDynamicSequenceItemCallbackInfo_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkDynamicSequenceItemCallbackInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkDynamicSequenceItemCallbackInfo_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkDynamicSequenceItemCallbackInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkDynamicSequenceItemCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public AkDynamicSequenceItemCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkDynamicSequenceItemCallbackInfo(), cMemoryOwn: true)
	{
	}
}
