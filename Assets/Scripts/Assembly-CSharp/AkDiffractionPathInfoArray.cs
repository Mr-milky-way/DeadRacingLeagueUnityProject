using System;

public class AkDiffractionPathInfoArray : AkBaseArray<AkDiffractionPathInfo>
{
	protected override int StructureSize => AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_GetSizeOf();

	public AkDiffractionPathInfoArray(int count)
		: base(count)
	{
	}

	protected override AkDiffractionPathInfo CreateNewReferenceFromIntPtr(IntPtr address)
	{
		return new AkDiffractionPathInfo(address, cMemoryOwn: false);
	}

	protected override void CloneIntoReferenceFromIntPtr(IntPtr address, AkDiffractionPathInfo other)
	{
		AkSoundEnginePINVOKE.CSharp_AkDiffractionPathInfo_Clone(address, AkDiffractionPathInfo.getCPtr(other));
	}
}
