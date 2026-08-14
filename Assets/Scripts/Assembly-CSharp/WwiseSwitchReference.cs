using UnityEngine;

public class WwiseSwitchReference : WwiseGroupValueObjectReference
{
	[AkShowOnly]
	[SerializeField]
	private WwiseSwitchGroupReference WwiseSwitchGroupReference;

	public override WwiseObjectType WwiseObjectType => WwiseObjectType.Switch;

	public override WwiseObjectReference GroupObjectReference
	{
		get
		{
			return WwiseSwitchGroupReference;
		}
		set
		{
			WwiseSwitchGroupReference = value as WwiseSwitchGroupReference;
		}
	}

	public override WwiseObjectType GroupWwiseObjectType => WwiseObjectType.SwitchGroup;
}
