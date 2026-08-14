using System;

public class AkReflectionPathInfoArray : AkBaseArray<AkReflectionPathInfo>
{
	protected override int StructureSize => AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_GetSizeOf();

	public AkReflectionPathInfoArray(int count)
		: base(count)
	{
	}

	protected override AkReflectionPathInfo CreateNewReferenceFromIntPtr(IntPtr address)
	{
		return new AkReflectionPathInfo(address, cMemoryOwn: false);
	}

	protected override void CloneIntoReferenceFromIntPtr(IntPtr address, AkReflectionPathInfo other)
	{
		AkSoundEnginePINVOKE.CSharp_AkReflectionPathInfo_Clone(address, AkReflectionPathInfo.getCPtr(other));
	}
}
