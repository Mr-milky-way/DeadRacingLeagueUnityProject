public abstract class WwiseGroupValueObjectReference : WwiseObjectReference
{
	public abstract WwiseObjectReference GroupObjectReference { get; set; }

	public abstract WwiseObjectType GroupWwiseObjectType { get; }

	public override string DisplayName
	{
		get
		{
			WwiseObjectReference groupObjectReference = GroupObjectReference;
			if (!groupObjectReference)
			{
				return base.ObjectName;
			}
			return groupObjectReference.ObjectName + " / " + base.ObjectName;
		}
	}
}
