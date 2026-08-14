using System;

public class AkTriangleArray : AkBaseArray<AkTriangle>
{
	protected override int StructureSize => AkSoundEnginePINVOKE.CSharp_AkTriangle_GetSizeOf();

	public AkTriangleArray(int count)
		: base(count)
	{
	}

	protected override void DefaultConstructAtIntPtr(IntPtr address)
	{
		AkSoundEnginePINVOKE.CSharp_AkTriangle_Clear(address);
	}

	protected override AkTriangle CreateNewReferenceFromIntPtr(IntPtr address)
	{
		return new AkTriangle(address, cMemoryOwn: false);
	}

	protected override void CloneIntoReferenceFromIntPtr(IntPtr address, AkTriangle other)
	{
		AkSoundEnginePINVOKE.CSharp_AkTriangle_Clone(address, AkTriangle.getCPtr(other));
	}
}
