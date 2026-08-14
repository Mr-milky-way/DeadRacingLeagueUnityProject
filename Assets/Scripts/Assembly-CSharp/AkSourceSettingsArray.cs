using System;

public class AkSourceSettingsArray : AkBaseArray<AkSourceSettings>
{
	protected override int StructureSize => AkSoundEnginePINVOKE.CSharp_AkSourceSettings_GetSizeOf();

	public AkSourceSettingsArray(int count)
		: base(count)
	{
	}

	protected override void DefaultConstructAtIntPtr(IntPtr address)
	{
		AkSoundEnginePINVOKE.CSharp_AkSourceSettings_Clear(address);
	}

	protected override AkSourceSettings CreateNewReferenceFromIntPtr(IntPtr address)
	{
		return new AkSourceSettings(address, cMemoryOwn: false);
	}

	protected override void CloneIntoReferenceFromIntPtr(IntPtr address, AkSourceSettings other)
	{
		AkSoundEnginePINVOKE.CSharp_AkSourceSettings_Clone(address, AkSourceSettings.getCPtr(other));
	}
}
