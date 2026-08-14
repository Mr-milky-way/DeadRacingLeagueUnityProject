using System;

public class AkChannelEmitter : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkTransform position
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkChannelEmitter_position_get(swigCPtr);
			if (!(intPtr == IntPtr.Zero))
			{
				return new AkTransform(intPtr, cMemoryOwn: false);
			}
			return null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkChannelEmitter_position_set(swigCPtr, AkTransform.getCPtr(value));
		}
	}

	public uint uInputChannels
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkChannelEmitter_uInputChannels_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkChannelEmitter_uInputChannels_set(swigCPtr, value);
		}
	}

	internal AkChannelEmitter(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkChannelEmitter obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkChannelEmitter()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkChannelEmitter(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
