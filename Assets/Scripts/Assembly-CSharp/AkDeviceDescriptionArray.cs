using System;

public class AkDeviceDescriptionArray : AkBaseArray<AkDeviceDescription>
{
	protected override int StructureSize => AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_GetSizeOf();

	public AkDeviceDescriptionArray(int count)
		: base(count)
	{
	}

	protected override void DefaultConstructAtIntPtr(IntPtr address)
	{
		AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_Clear(address);
	}

	protected override AkDeviceDescription CreateNewReferenceFromIntPtr(IntPtr address)
	{
		return new AkDeviceDescription(address, cMemoryOwn: false);
	}

	protected override void CloneIntoReferenceFromIntPtr(IntPtr address, AkDeviceDescription other)
	{
		AkSoundEnginePINVOKE.CSharp_AkDeviceDescription_Clone(address, AkDeviceDescription.getCPtr(other));
	}
}
