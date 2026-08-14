using UnityEngine;

public class WwiseStateReference : WwiseGroupValueObjectReference
{
	[AkShowOnly]
	[SerializeField]
	private WwiseStateGroupReference WwiseStateGroupReference;

	public override WwiseObjectType WwiseObjectType => WwiseObjectType.State;

	public override WwiseObjectReference GroupObjectReference
	{
		get
		{
			return WwiseStateGroupReference;
		}
		set
		{
			WwiseStateGroupReference = value as WwiseStateGroupReference;
		}
	}

	public override WwiseObjectType GroupWwiseObjectType => WwiseObjectType.StateGroup;
}
