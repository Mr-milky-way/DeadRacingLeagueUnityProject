using System;

public class AkMusicSyncCallbackInfo : AkCallbackInfo
{
	private IntPtr swigCPtr;

	public uint playingID => AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_playingID_get(swigCPtr);

	public int segmentInfo_iCurrentPosition => AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_iCurrentPosition_get(swigCPtr);

	public int segmentInfo_iPreEntryDuration => AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_iPreEntryDuration_get(swigCPtr);

	public int segmentInfo_iActiveDuration => AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_iActiveDuration_get(swigCPtr);

	public int segmentInfo_iPostExitDuration => AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_iPostExitDuration_get(swigCPtr);

	public int segmentInfo_iRemainingLookAheadTime => AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_iRemainingLookAheadTime_get(swigCPtr);

	public float segmentInfo_fBeatDuration => AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_fBeatDuration_get(swigCPtr);

	public float segmentInfo_fBarDuration => AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_fBarDuration_get(swigCPtr);

	public float segmentInfo_fGridDuration => AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_fGridDuration_get(swigCPtr);

	public float segmentInfo_fGridOffset => AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_fGridOffset_get(swigCPtr);

	public AkCallbackType musicSyncType => (AkCallbackType)AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_musicSyncType_get(swigCPtr);

	public string userCueName => AkSoundEngine.StringFromIntPtrString(AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_userCueName_get(swigCPtr));

	internal AkMusicSyncCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkMusicSyncCallbackInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkMusicSyncCallbackInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkMusicSyncCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public AkMusicSyncCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkMusicSyncCallbackInfo(), cMemoryOwn: true)
	{
	}
}
