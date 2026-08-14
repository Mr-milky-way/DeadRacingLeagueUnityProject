using System;

public class AkMusicPlaylistCallbackInfo : AkEventCallbackInfo
{
	private IntPtr swigCPtr;

	public uint playlistID => AkSoundEnginePINVOKE.CSharp_AkMusicPlaylistCallbackInfo_playlistID_get(swigCPtr);

	public uint uNumPlaylistItems => AkSoundEnginePINVOKE.CSharp_AkMusicPlaylistCallbackInfo_uNumPlaylistItems_get(swigCPtr);

	public uint uPlaylistSelection => AkSoundEnginePINVOKE.CSharp_AkMusicPlaylistCallbackInfo_uPlaylistSelection_get(swigCPtr);

	public uint uPlaylistItemDone => AkSoundEnginePINVOKE.CSharp_AkMusicPlaylistCallbackInfo_uPlaylistItemDone_get(swigCPtr);

	internal AkMusicPlaylistCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkMusicPlaylistCallbackInfo_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkMusicPlaylistCallbackInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkMusicPlaylistCallbackInfo_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkMusicPlaylistCallbackInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkMusicPlaylistCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public AkMusicPlaylistCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkMusicPlaylistCallbackInfo(), cMemoryOwn: true)
	{
	}
}
