using System;
using UnityEngine;

public class AkTransform : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	internal AkTransform(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkTransform obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkTransform()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkTransform(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public Vector3 Position()
	{
		return AkSoundEnginePINVOKE.CSharp_AkTransform_Position(swigCPtr);
	}

	public Vector3 OrientationFront()
	{
		return AkSoundEnginePINVOKE.CSharp_AkTransform_OrientationFront(swigCPtr);
	}

	public Vector3 OrientationTop()
	{
		return AkSoundEnginePINVOKE.CSharp_AkTransform_OrientationTop(swigCPtr);
	}

	public void Set(Vector3 in_position, Vector3 in_orientationFront, Vector3 in_orientationTop)
	{
		AkSoundEnginePINVOKE.CSharp_AkTransform_Set__SWIG_0(swigCPtr, in_position, in_orientationFront, in_orientationTop);
	}

	public void Set(float in_positionX, float in_positionY, float in_positionZ, float in_orientFrontX, float in_orientFrontY, float in_orientFrontZ, float in_orientTopX, float in_orientTopY, float in_orientTopZ)
	{
		AkSoundEnginePINVOKE.CSharp_AkTransform_Set__SWIG_1(swigCPtr, in_positionX, in_positionY, in_positionZ, in_orientFrontX, in_orientFrontY, in_orientFrontZ, in_orientTopX, in_orientTopY, in_orientTopZ);
	}

	public void SetPosition(Vector3 in_position)
	{
		AkSoundEnginePINVOKE.CSharp_AkTransform_SetPosition__SWIG_0(swigCPtr, in_position);
	}

	public void SetPosition(float in_x, float in_y, float in_z)
	{
		AkSoundEnginePINVOKE.CSharp_AkTransform_SetPosition__SWIG_1(swigCPtr, in_x, in_y, in_z);
	}

	public void SetOrientation(Vector3 in_orientationFront, Vector3 in_orientationTop)
	{
		AkSoundEnginePINVOKE.CSharp_AkTransform_SetOrientation__SWIG_0(swigCPtr, in_orientationFront, in_orientationTop);
	}

	public void SetOrientation(float in_orientFrontX, float in_orientFrontY, float in_orientFrontZ, float in_orientTopX, float in_orientTopY, float in_orientTopZ)
	{
		AkSoundEnginePINVOKE.CSharp_AkTransform_SetOrientation__SWIG_1(swigCPtr, in_orientFrontX, in_orientFrontY, in_orientFrontZ, in_orientTopX, in_orientTopY, in_orientTopZ);
	}

	public AkTransform()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkTransform(), cMemoryOwn: true)
	{
	}
}
