using System;

public class AkAcousticSurface : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint textureID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_textureID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_textureID_set(swigCPtr, value);
		}
	}

	public float occlusion
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_occlusion_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_occlusion_set(swigCPtr, value);
		}
	}

	public string strName
	{
		get
		{
			return AkSoundEngine.StringFromIntPtrString(AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_strName_get(swigCPtr));
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_strName_set(swigCPtr, value);
		}
	}

	internal AkAcousticSurface(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkAcousticSurface obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkAcousticSurface()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkAcousticSurface(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkAcousticSurface()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkAcousticSurface(), cMemoryOwn: true)
	{
	}

	public void Clear()
	{
		AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_Clear(swigCPtr);
	}

	public void DeleteName()
	{
		AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_DeleteName(swigCPtr);
	}

	public static int GetSizeOf()
	{
		return AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_GetSizeOf();
	}

	public void Clone(AkAcousticSurface other)
	{
		AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_Clone(swigCPtr, getCPtr(other));
	}
}
