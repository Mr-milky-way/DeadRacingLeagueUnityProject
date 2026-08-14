using System;

public class AkTriangle : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public ushort point0
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkTriangle_point0_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkTriangle_point0_set(swigCPtr, value);
		}
	}

	public ushort point1
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkTriangle_point1_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkTriangle_point1_set(swigCPtr, value);
		}
	}

	public ushort point2
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkTriangle_point2_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkTriangle_point2_set(swigCPtr, value);
		}
	}

	public ushort surface
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkTriangle_surface_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkTriangle_surface_set(swigCPtr, value);
		}
	}

	internal AkTriangle(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkTriangle obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkTriangle()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkTriangle(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkTriangle()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkTriangle__SWIG_0(), cMemoryOwn: true)
	{
	}

	public AkTriangle(ushort in_pt0, ushort in_pt1, ushort in_pt2, ushort in_surfaceInfo)
		: this(AkSoundEnginePINVOKE.CSharp_new_AkTriangle__SWIG_1(in_pt0, in_pt1, in_pt2, in_surfaceInfo), cMemoryOwn: true)
	{
	}

	public void Clear()
	{
		AkSoundEnginePINVOKE.CSharp_AkTriangle_Clear(swigCPtr);
	}

	public static int GetSizeOf()
	{
		return AkSoundEnginePINVOKE.CSharp_AkTriangle_GetSizeOf();
	}

	public void Clone(AkTriangle other)
	{
		AkSoundEnginePINVOKE.CSharp_AkTriangle_Clone(swigCPtr, getCPtr(other));
	}
}
