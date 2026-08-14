using System;
using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("Wwise/AkBank")]
[ExecuteInEditMode]
[DefaultExecutionOrder(-75)]
public class AkBank : AkTriggerHandler
{
	public Bank data = new Bank();

	public bool decodeBank;

	public bool loadAsynchronous;

	public bool saveDecodedBank;

	public List<int> unloadTriggerList = new List<int> { -358577003 };

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("bankName")]
	private string bankNameInternal;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("valueGuid")]
	private byte[] valueGuidInternal;

	[Obsolete("This functionality is deprecated as of Wwise v2018.1.6 and will be removed in a future release.")]
	public string bankName
	{
		get
		{
			if (data != null)
			{
				return data.Name;
			}
			return string.Empty;
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

	protected override void Awake()
	{
		base.Awake();
		RegisterTriggers(unloadTriggerList, UnloadBank);
	}

	protected override void Start()
	{
		base.Start();
		if (unloadTriggerList.Contains(1281810935))
		{
			UnloadBank(null);
		}
	}

	public override void HandleEvent(GameObject in_gameObject)
	{
		float t0 = Time.realtimeSinceStartup;
		int ms = 0;
		string bank_name = (base.gameObject ? base.gameObject.name : "<null>");
		if (!loadAsynchronous)
		{
			data.Load(decodeBank, saveDecodedBank);
			ms = (int)((Time.realtimeSinceStartup - t0) * 1000f);
			Debug.Log($"AkBank> {bank_name} Load Sync Complete in {ms}ms");
		}
		else
		{
			data.LoadAsync(delegate(uint p_bank_id, IntPtr p_bank_ptr, AKRESULT p_result, object p_cookie)
			{
				ms = (int)((Time.realtimeSinceStartup - t0) * 1000f);
				Debug.Log($"AkBank> {bank_name} Load Async Complete in {ms}ms / {p_result}");
			});
		}
	}

	public void UnloadBank(GameObject in_gameObject)
	{
		data.Unload();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UnregisterTriggers(unloadTriggerList, UnloadBank);
	}
}
