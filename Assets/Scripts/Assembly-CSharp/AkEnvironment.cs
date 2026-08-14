using System;
using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("Wwise/AkEnvironment")]
[RequireComponent(typeof(Collider))]
[ExecuteInEditMode]
public class AkEnvironment : MonoBehaviour
{
	public class AkEnvironment_CompareByPriority : IComparer<AkEnvironment>
	{
		public virtual int Compare(AkEnvironment a, AkEnvironment b)
		{
			int num = a.priority.CompareTo(b.priority);
			if (num != 0 || !(a != b))
			{
				return num;
			}
			return 1;
		}
	}

	public class AkEnvironment_CompareBySelectionAlgorithm : AkEnvironment_CompareByPriority
	{
		public override int Compare(AkEnvironment a, AkEnvironment b)
		{
			if (a.isDefault)
			{
				if (!b.isDefault)
				{
					return 1;
				}
				return base.Compare(a, b);
			}
			if (b.isDefault)
			{
				return -1;
			}
			if (a.excludeOthers)
			{
				if (!b.excludeOthers)
				{
					return -1;
				}
				return base.Compare(a, b);
			}
			if (!b.excludeOthers)
			{
				return base.Compare(a, b);
			}
			return 1;
		}
	}

	public const int MAX_NB_ENVIRONMENTS = 4;

	public static AkEnvironment_CompareByPriority s_compareByPriority = new AkEnvironment_CompareByPriority();

	public static AkEnvironment_CompareBySelectionAlgorithm s_compareBySelectionAlgorithm = new AkEnvironment_CompareBySelectionAlgorithm();

	public bool excludeOthers;

	public bool isDefault;

	public AuxBus data = new AuxBus();

	public int priority;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("m_auxBusID")]
	private int auxBusIdInternal;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("valueGuid")]
	private byte[] valueGuidInternal;

	public Collider Collider { get; private set; }

	[Obsolete("This functionality is deprecated as of Wwise v2018.1.2 and will be removed in a future release.")]
	public int m_auxBusID
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

	public void Awake()
	{
		Collider = GetComponent<Collider>();
	}

	[Obsolete("This functionality is deprecated as of Wwise v2018.1.2 and will be removed in a future release.")]
	public uint GetAuxBusID()
	{
		return data.Id;
	}

	[Obsolete("This functionality is deprecated as of Wwise v2018.1.6 and will be removed in a future release.")]
	public Collider GetCollider()
	{
		return Collider;
	}
}
