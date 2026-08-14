using System;

public class AkObjectInfoArray : AkBaseArray<AkObjectInfo>
{
	protected override int StructureSize => AkSoundEnginePINVOKE.CSharp_AkObjectInfo_GetSizeOf();

	public AkObjectInfoArray(int count)
		: base(count)
	{
	}

	protected override void DefaultConstructAtIntPtr(IntPtr address)
	{
		AkSoundEnginePINVOKE.CSharp_AkObjectInfo_Clear(address);
	}

	protected override AkObjectInfo CreateNewReferenceFromIntPtr(IntPtr address)
	{
		return new AkObjectInfo(address, cMemoryOwn: false);
	}

	protected override void CloneIntoReferenceFromIntPtr(IntPtr address, AkObjectInfo other)
	{
		AkSoundEnginePINVOKE.CSharp_AkObjectInfo_Clone(address, AkObjectInfo.getCPtr(other));
	}
}
