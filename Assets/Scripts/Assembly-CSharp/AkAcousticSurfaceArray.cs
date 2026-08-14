using System;

public class AkAcousticSurfaceArray : AkBaseArray<AkAcousticSurface>
{
	protected override int StructureSize => AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_GetSizeOf();

	public AkAcousticSurfaceArray(int count)
		: base(count)
	{
	}

	protected override void DefaultConstructAtIntPtr(IntPtr address)
	{
		AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_Clear(address);
	}

	protected override AkAcousticSurface CreateNewReferenceFromIntPtr(IntPtr address)
	{
		return new AkAcousticSurface(address, cMemoryOwn: false);
	}

	protected override void CloneIntoReferenceFromIntPtr(IntPtr address, AkAcousticSurface other)
	{
		AkSoundEnginePINVOKE.CSharp_AkAcousticSurface_Clone(address, AkAcousticSurface.getCPtr(other));
	}
}
