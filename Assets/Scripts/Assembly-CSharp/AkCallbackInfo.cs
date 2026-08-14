using System;

public class AkCallbackInfo : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public IntPtr pCookie => AkSoundEnginePINVOKE.CSharp_AkCallbackInfo_pCookie_get(swigCPtr);

	public ulong gameObjID => AkSoundEnginePINVOKE.CSharp_AkCallbackInfo_gameObjID_get(swigCPtr);

	internal AkCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkCallbackInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkCallbackInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkCallbackInfo(), cMemoryOwn: true)
	{
	}
}
