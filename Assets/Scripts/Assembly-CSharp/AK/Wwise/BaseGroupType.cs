using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AK.Wwise
{
	[Serializable]
	public abstract class BaseGroupType : BaseType
	{
		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("groupID")]
		private int groupIdInternal;

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("groupGuid")]
		private byte[] groupGuidInternal;

		public WwiseObjectReference GroupWwiseObjectReference
		{
			get
			{
				WwiseGroupValueObjectReference wwiseGroupValueObjectReference = ObjectReference as WwiseGroupValueObjectReference;
				if (!wwiseGroupValueObjectReference)
				{
					return null;
				}
				return wwiseGroupValueObjectReference.GroupObjectReference;
			}
		}

		public abstract WwiseObjectType WwiseObjectGroupType { get; }

		public uint GroupId
		{
			get
			{
				if (!GroupWwiseObjectReference)
				{
					return 0u;
				}
				return GroupWwiseObjectReference.Id;
			}
		}

		[Obsolete("This functionality is deprecated as of Wwise v2018.1.2 and will be removed in a future release.")]
		public int groupID => (int)GroupId;

		[Obsolete("This functionality is deprecated as of Wwise v2018.1.6 and will be removed in a future release.")]
		public byte[] groupGuid
		{
			get
			{
				WwiseObjectReference objectReference = ObjectReference;
				if ((bool)objectReference)
				{
					return objectReference.Guid.ToByteArray();
				}
				return null;
			}
		}

		public override bool IsValid()
		{
			if (base.IsValid())
			{
				return GroupWwiseObjectReference != null;
			}
			return false;
		}
	}
}
