using System;

public class AkEventCallbackInfo : AkCallbackInfo
{
	private IntPtr swigCPtr;

	public uint playingID => AkSoundEnginePINVOKE.CSharp_AkEventCallbackInfo_playingID_get(swigCPtr);

	public uint eventID => AkSoundEnginePINVOKE.CSharp_AkEventCallbackInfo_eventID_get(swigCPtr);

	internal AkEventCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkEventCallbackInfo_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkEventCallbackInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkEventCallbackInfo_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkEventCallbackInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkEventCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public AkEventCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkEventCallbackInfo(), cMemoryOwn: true)
	{
	}
}
