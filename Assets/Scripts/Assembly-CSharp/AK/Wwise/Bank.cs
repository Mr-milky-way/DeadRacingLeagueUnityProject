using System;

namespace AK.Wwise
{
	[Serializable]
	public class Bank : BaseType
	{
		public WwiseBankReference WwiseObjectReference;

		public override WwiseObjectReference ObjectReference
		{
			get
			{
				return WwiseObjectReference;
			}
			set
			{
				WwiseObjectReference = value as WwiseBankReference;
			}
		}

		public override WwiseObjectType WwiseObjectType => WwiseObjectType.Soundbank;

		public void Load(bool decodeBank = false, bool saveDecodedBank = false)
		{
			if (IsValid())
			{
				AkBankManager.LoadBank(Name, decodeBank, saveDecodedBank);
			}
		}

		public void LoadAsync(AkCallbackManager.BankCallback callback = null)
		{
			if (IsValid())
			{
				AkBankManager.LoadBankAsync(Name, callback);
			}
		}

		public void Unload()
		{
			if (IsValid())
			{
				AkBankManager.UnloadBank(Name);
			}
		}
	}
}
