using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AK.Wwise
{
	[Serializable]
	public abstract class BaseType
	{
		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("ID")]
		private int idInternal;

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("valueGuid")]
		private byte[] valueGuidInternal;

		public abstract WwiseObjectReference ObjectReference { get; set; }

		public abstract WwiseObjectType WwiseObjectType { get; }

		public virtual string Name
		{
			get
			{
				if (!IsValid())
				{
					return string.Empty;
				}
				return ObjectReference.DisplayName;
			}
		}

		public uint Id
		{
			get
			{
				if (!IsValid())
				{
					return 0u;
				}
				return ObjectReference.Id;
			}
		}

		public static uint InvalidId => 0u;

		[Obsolete("This functionality is deprecated as of Wwise v2018.1.2 and will be removed in a future release.")]
		public int ID => (int)Id;

		[Obsolete("This functionality is deprecated as of Wwise v2018.1.6 and will be removed in a future release.")]
		public byte[] valueGuid
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

		public virtual bool IsValid()
		{
			return ObjectReference != null;
		}

		public bool Validate()
		{
			if (IsValid())
			{
				return true;
			}
			Debug.LogWarning("Wwise ID has not been resolved. Consider picking a new " + GetType().Name + ".");
			return false;
		}

		protected void Verify(AKRESULT result)
		{
		}

		public override string ToString()
		{
			if (!IsValid())
			{
				return "Empty " + GetType().Name;
			}
			return ObjectReference.ObjectName;
		}
	}
}
