using System;

public class AkTaskContext : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint uIdxThread
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkTaskContext_uIdxThread_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkTaskContext_uIdxThread_set(swigCPtr, value);
		}
	}

	internal AkTaskContext(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkTaskContext obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkTaskContext()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkTaskContext(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkTaskContext()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkTaskContext(), cMemoryOwn: true)
	{
	}
}
