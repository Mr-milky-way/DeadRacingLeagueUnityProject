using System;

public class AkObstructionOcclusionValuesArray : AkBaseArray<AkObstructionOcclusionValues>
{
	protected override int StructureSize => AkSoundEnginePINVOKE.CSharp_AkObstructionOcclusionValues_GetSizeOf();

	public AkObstructionOcclusionValuesArray(int count)
		: base(count)
	{
	}

	protected override void DefaultConstructAtIntPtr(IntPtr address)
	{
		AkSoundEnginePINVOKE.CSharp_AkObstructionOcclusionValues_Clear(address);
	}

	protected override AkObstructionOcclusionValues CreateNewReferenceFromIntPtr(IntPtr address)
	{
		return new AkObstructionOcclusionValues(address, cMemoryOwn: false);
	}

	protected override void CloneIntoReferenceFromIntPtr(IntPtr address, AkObstructionOcclusionValues other)
	{
		AkSoundEnginePINVOKE.CSharp_AkObstructionOcclusionValues_Clone(address, AkObstructionOcclusionValues.getCPtr(other));
	}
}
