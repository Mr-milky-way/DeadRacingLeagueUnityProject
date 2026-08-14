using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class AkBankManager
{
	private class BankHandle
	{
		protected readonly string bankName;

		protected uint m_BankID;

		public int RefCount { get; private set; }

		public BankHandle(string name)
		{
			bankName = name;
		}

		public virtual AKRESULT DoLoadBank()
		{
			return AkSoundEngine.LoadBank(bankName, out m_BankID);
		}

		public void LoadBank()
		{
			if (RefCount == 0 && !BanksToUnload.Remove(this))
			{
				AKRESULT result = DoLoadBank();
				LogLoadResult(result);
			}
			IncRef();
		}

		public virtual void UnloadBank()
		{
			AkSoundEngine.UnloadBank(m_BankID, IntPtr.Zero, null, null);
			lock (m_BankHandles)
			{
				m_BankHandles.Remove(bankName);
			}
		}

		public void IncRef()
		{
			if (RefCount == 0)
			{
				BanksToUnload.Remove(this);
			}
			RefCount++;
		}

		public void DecRef()
		{
			RefCount--;
			if (RefCount == 0)
			{
				BanksToUnload.Add(this);
			}
		}

		protected void LogLoadResult(AKRESULT result)
		{
			if (result != AKRESULT.AK_Success && AkSoundEngine.IsInitialized())
			{
				Debug.LogWarning("WwiseUnity: Bank " + bankName + " failed to load (" + result.ToString() + ")");
			}
		}
	}

	private class AsyncBankHandle : BankHandle
	{
		private readonly AkCallbackManager.BankCallback bankCallback;

		public AsyncBankHandle(string name, AkCallbackManager.BankCallback callback)
			: base(name)
		{
			bankCallback = callback;
		}

		private static void GlobalBankCallback(uint in_bankID, IntPtr in_pInMemoryBankPtr, AKRESULT in_eLoadResult, object in_Cookie)
		{
			AsyncBankHandle asyncBankHandle = (AsyncBankHandle)in_Cookie;
			AkCallbackManager.BankCallback bankCallback = asyncBankHandle.bankCallback;
			if (in_eLoadResult != AKRESULT.AK_Success)
			{
				asyncBankHandle.LogLoadResult(in_eLoadResult);
				if (in_eLoadResult != AKRESULT.AK_BankAlreadyLoaded)
				{
					lock (m_BankHandles)
					{
						m_BankHandles.Remove(asyncBankHandle.bankName);
					}
				}
			}
			bankCallback?.Invoke(in_bankID, in_pInMemoryBankPtr, in_eLoadResult, null);
		}

		public override AKRESULT DoLoadBank()
		{
			return AkSoundEngine.LoadBank(bankName, GlobalBankCallback, this, out m_BankID);
		}
	}

	private class DecodableBankHandle : BankHandle
	{
		private readonly bool decodeBank = true;

		private readonly string decodedBankPath;

		private readonly bool saveDecodedBank;

		public DecodableBankHandle(string name, bool save)
			: base(name)
		{
			saveDecodedBank = save;
			string path = bankName + ".bnk";
			string currentLanguage = AkSoundEngine.GetCurrentLanguage();
			string decodedBankFullPath = AkBasePathGetter.DecodedBankFullPath;
			decodedBankPath = Path.Combine(decodedBankFullPath, currentLanguage);
			string path2 = Path.Combine(decodedBankPath, path);
			bool flag = File.Exists(path2);
			if (!flag)
			{
				decodedBankPath = decodedBankFullPath;
				path2 = Path.Combine(decodedBankPath, path);
				flag = File.Exists(path2);
			}
			if (flag)
			{
				try
				{
					DateTime lastWriteTime = File.GetLastWriteTime(path2);
					DateTime lastWriteTime2 = File.GetLastWriteTime(Path.Combine(AkBasePathGetter.SoundBankBasePath, path));
					decodeBank = lastWriteTime <= lastWriteTime2;
				}
				catch
				{
				}
			}
		}

		public override AKRESULT DoLoadBank()
		{
			if (decodeBank)
			{
				return AkSoundEngine.LoadAndDecodeBank(bankName, saveDecodedBank, out m_BankID);
			}
			if (string.IsNullOrEmpty(decodedBankPath))
			{
				return AkSoundEngine.LoadBank(bankName, out m_BankID);
			}
			AKRESULT aKRESULT = AkSoundEngine.SetBasePath(decodedBankPath);
			if (aKRESULT == AKRESULT.AK_Success)
			{
				aKRESULT = AkSoundEngine.LoadBank(bankName, out m_BankID);
				AkSoundEngine.SetBasePath(AkBasePathGetter.SoundBankBasePath);
			}
			return aKRESULT;
		}

		public override void UnloadBank()
		{
			if (decodeBank && !saveDecodedBank)
			{
				AkSoundEngine.PrepareBank(AkPreparationType.Preparation_Unload, m_BankID);
			}
			else
			{
				base.UnloadBank();
			}
		}
	}

	private static readonly Dictionary<string, BankHandle> m_BankHandles = new Dictionary<string, BankHandle>();

	private static readonly List<BankHandle> BanksToUnload = new List<BankHandle>();

	internal static void DoUnloadBanks()
	{
		int count = BanksToUnload.Count;
		for (int i = 0; i < count; i++)
		{
			BanksToUnload[i].UnloadBank();
		}
		BanksToUnload.Clear();
	}

	internal static void Reset()
	{
		m_BankHandles.Clear();
		BanksToUnload.Clear();
	}

	public static void LoadInitBank(bool doReset = true)
	{
		if (doReset)
		{
			Reset();
		}
		uint out_bankID;
		AKRESULT aKRESULT = AkSoundEngine.LoadBank("Init.bnk", out out_bankID);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			Debug.LogError("WwiseUnity: Failed load Init.bnk with result: " + aKRESULT);
		}
	}

	public static void UnloadInitBank()
	{
		AkSoundEngine.UnloadBank("Init.bnk", IntPtr.Zero);
	}

	public static void LoadBank(string name, bool decodeBank, bool saveDecodedBank)
	{
		BankHandle value = null;
		lock (m_BankHandles)
		{
			if (m_BankHandles.TryGetValue(name, out value))
			{
				value.IncRef();
				return;
			}
			value = (decodeBank ? new DecodableBankHandle(name, saveDecodedBank) : new BankHandle(name));
			m_BankHandles.Add(name, value);
		}
		value.LoadBank();
	}

	public static void LoadBankAsync(string name, AkCallbackManager.BankCallback callback = null)
	{
		BankHandle value = null;
		lock (m_BankHandles)
		{
			if (m_BankHandles.TryGetValue(name, out value))
			{
				value.IncRef();
				return;
			}
			value = new AsyncBankHandle(name, callback);
			m_BankHandles.Add(name, value);
		}
		value.LoadBank();
	}

	public static void UnloadBank(string name)
	{
		lock (m_BankHandles)
		{
			BankHandle value = null;
			if (m_BankHandles.TryGetValue(name, out value))
			{
				value.DecRef();
			}
		}
	}
}
