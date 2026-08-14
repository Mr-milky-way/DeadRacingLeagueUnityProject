using System;
using UnityEngine;

public abstract class WwiseObjectReference : ScriptableObject
{
	[AkShowOnly]
	[SerializeField]
	private string objectName = string.Empty;

	[AkShowOnly]
	[SerializeField]
	private uint id;

	[AkShowOnly]
	[SerializeField]
	private string guid = string.Empty;

	public Guid Guid
	{
		get
		{
			if (!string.IsNullOrEmpty(guid))
			{
				return new Guid(guid);
			}
			return Guid.Empty;
		}
	}

	public string ObjectName => objectName;

	public virtual string DisplayName => ObjectName;

	public uint Id => id;

	public abstract WwiseObjectType WwiseObjectType { get; }
}
