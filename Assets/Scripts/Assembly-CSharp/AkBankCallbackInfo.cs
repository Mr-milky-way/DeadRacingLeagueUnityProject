using System;

public class AkBankCallbackInfo : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint bankID => AkSoundEnginePINVOKE.CSharp_AkBankCallbackInfo_bankID_get(swigCPtr);

	public IntPtr inMemoryBankPtr => AkSoundEnginePINVOKE.CSharp_AkBankCallbackInfo_inMemoryBankPtr_get(swigCPtr);

	public AKRESULT loadResult => (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkBankCallbackInfo_loadResult_get(swigCPtr);

	internal AkBankCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkBankCallbackInfo obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkBankCallbackInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkBankCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkBankCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkBankCallbackInfo(), cMemoryOwn: true)
	{
	}
}
