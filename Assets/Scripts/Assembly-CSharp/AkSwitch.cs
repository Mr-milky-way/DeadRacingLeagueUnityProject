using System;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("Wwise/AkSwitch")]
[DefaultExecutionOrder(-10)]
public class AkSwitch : AkDragDropTriggerHandler
{
	public Switch data = new Switch();

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("valueID")]
	private int valueIdInternal;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("groupID")]
	private int groupIdInternal;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("valueGuid")]
	private byte[] valueGuidInternal;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("groupGuid")]
	private byte[] groupGuidInternal;

	protected override BaseType WwiseType => data;

	[Obsolete("This functionality is deprecated as of Wwise v2018.1.6 and will be removed in a future release.")]
	public int valueID
	{
		get
		{
			if (data != null)
			{
				return (int)data.Id;
			}
			return 0;
		}
	}

	[Obsolete("This functionality is deprecated as of Wwise v2018.1.6 and will be removed in a future release.")]
	public int groupID
	{
		get
		{
			if (data != null)
			{
				return (int)data.GroupId;
			}
			return 0;
		}
	}

	[Obsolete("This functionality is deprecated as of Wwise v2018.1.6 and will be removed in a future release.")]
	public byte[] valueGuid
	{
		get
		{
			if (data == null)
			{
				return null;
			}
			WwiseObjectReference objectReference = data.ObjectReference;
			if ((bool)objectReference)
			{
				return objectReference.Guid.ToByteArray();
			}
			return null;
		}
	}

	[Obsolete("This functionality is deprecated as of Wwise v2018.1.6 and will be removed in a future release.")]
	public byte[] groupGuid
	{
		get
		{
			if (data == null)
			{
				return null;
			}
			WwiseObjectReference groupWwiseObjectReference = data.GroupWwiseObjectReference;
			if ((bool)groupWwiseObjectReference)
			{
				return groupWwiseObjectReference.Guid.ToByteArray();
			}
			return null;
		}
	}

	public override void HandleEvent(GameObject in_gameObject)
	{
		data.SetValue((useOtherObject && in_gameObject != null) ? in_gameObject : base.gameObject);
	}
}
