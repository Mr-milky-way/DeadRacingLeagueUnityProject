using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class AkMemBankLoader : MonoBehaviour
{
	private const int WaitMs = 50;

	private const long AK_BANK_PLATFORM_DATA_ALIGNMENT = 16L;

	private const long AK_BANK_PLATFORM_DATA_ALIGNMENT_MASK = 15L;

	public string bankName = "";

	public bool isLocalizedBank;

	private string m_bankPath;

	[HideInInspector]
	public uint ms_bankID;

	private IntPtr ms_pInMemoryBankPtr = IntPtr.Zero;

	private GCHandle ms_pinnedArray;

	private UnityWebRequest ms_www;

	private void Start()
	{
		if (isLocalizedBank)
		{
			LoadLocalizedBank(bankName);
		}
		else
		{
			LoadNonLocalizedBank(bankName);
		}
	}

	public void LoadNonLocalizedBank(string in_bankFilename)
	{
		string in_bankPath = "file://" + Path.Combine(AkBasePathGetter.SoundBankBasePath, in_bankFilename);
		DoLoadBank(in_bankPath);
	}

	public void LoadLocalizedBank(string in_bankFilename)
	{
		string in_bankPath = "file://" + Path.Combine(Path.Combine(AkBasePathGetter.SoundBankBasePath, AkSoundEngine.GetCurrentLanguage()), in_bankFilename);
		DoLoadBank(in_bankPath);
	}

	private uint AllocateAlignedBuffer(byte[] data)
	{
		uint result = 0u;
		try
		{
			ms_pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
			ms_pInMemoryBankPtr = ms_pinnedArray.AddrOfPinnedObject();
			result = (uint)data.Length;
			if ((ms_pInMemoryBankPtr.ToInt64() & 0xF) != 0L)
			{
				byte[] array = new byte[(long)data.Length + 16L];
				GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				IntPtr intPtr = gCHandle.AddrOfPinnedObject();
				int destinationIndex = 0;
				if ((intPtr.ToInt64() & 0xF) != 0L)
				{
					long num = (intPtr.ToInt64() + 15) & -16;
					destinationIndex = (int)(num - intPtr.ToInt64());
					intPtr = new IntPtr(num);
				}
				Array.Copy(data, 0, array, destinationIndex, data.Length);
				ms_pInMemoryBankPtr = intPtr;
				ms_pinnedArray.Free();
				ms_pinnedArray = gCHandle;
			}
		}
		catch
		{
		}
		return result;
	}

	private IEnumerator LoadFile()
	{
		ms_www = UnityWebRequest.Get(m_bankPath);
		yield return ms_www.SendWebRequest();
		uint in_uInMemoryBankSize = AllocateAlignedBuffer(ms_www.downloadHandler.data);
		AKRESULT aKRESULT = AkSoundEngine.LoadBankMemoryView(ms_pInMemoryBankPtr, in_uInMemoryBankSize, out ms_bankID);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			Debug.LogError("WwiseUnity: AkMemBankLoader: bank loading failed with result " + aKRESULT);
		}
	}

	private void DoLoadBank(string in_bankPath)
	{
		m_bankPath = in_bankPath;
		StartCoroutine(LoadFile());
	}

	private void OnDestroy()
	{
		if (ms_pInMemoryBankPtr != IntPtr.Zero && AkSoundEngine.UnloadBank(ms_bankID, ms_pInMemoryBankPtr) == AKRESULT.AK_Success)
		{
			ms_pinnedArray.Free();
		}
	}
}
