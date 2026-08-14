using System;

public class AkExternalSourceInfoArray : AkBaseArray<AkExternalSourceInfo>
{
	protected override int StructureSize => AkSoundEnginePINVOKE.CSharp_AkExternalSourceInfo_GetSizeOf();

	public AkExternalSourceInfoArray(int count)
		: base(count)
	{
	}

	protected override void DefaultConstructAtIntPtr(IntPtr address)
	{
		AkSoundEnginePINVOKE.CSharp_AkExternalSourceInfo_Clear(address);
	}

	protected override void ReleaseAllocatedMemoryFromReferenceAtIntPtr(IntPtr address)
	{
		AkSoundEnginePINVOKE.CSharp_AkExternalSourceInfo_szFile_set(address, null);
	}

	protected override AkExternalSourceInfo CreateNewReferenceFromIntPtr(IntPtr address)
	{
		return new AkExternalSourceInfo(address, cMemoryOwn: false);
	}

	protected override void CloneIntoReferenceFromIntPtr(IntPtr address, AkExternalSourceInfo other)
	{
		AkSoundEnginePINVOKE.CSharp_AkExternalSourceInfo_Clone(address, AkExternalSourceInfo.getCPtr(other));
	}
}
