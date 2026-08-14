using AK.Wwise;
using UnityEngine;

[ExecuteInEditMode]
public abstract class AkDragDropTriggerHandler : AkTriggerHandler
{
	protected abstract BaseType WwiseType { get; }

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
