using System;

namespace Epic.OnlineServices.Ecom
{
	public sealed class Transaction : Handle
	{
		public const int TransactionCopyentitlementbyindexApiLatest = 1;

		public const int TransactionGetentitlementscountApiLatest = 1;

		public Transaction()
		{
		}

		public Transaction(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopyEntitlementByIndex(TransactionCopyEntitlementByIndexOptions options, out Entitlement outEntitlement)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<TransactionCopyEntitlementByIndexOptionsInternal, TransactionCopyEntitlementByIndexOptions>(ref target, options);
			IntPtr outEntitlement2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_Transaction_CopyEntitlementByIndex(base.InnerHandle, target, ref outEntitlement2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<EntitlementInternal, Entitlement>(outEntitlement2, out outEntitlement))
			{
				Bindings.EOS_Ecom_Entitlement_Release(outEntitlement2);
			}
			return result;
		}

		public uint GetEntitlementsCount(TransactionGetEntitlementsCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<TransactionGetEntitlementsCountOptionsInternal, TransactionGetEntitlementsCountOptions>(ref target, options);
			uint result = Bindings.EOS_Ecom_Transaction_GetEntitlementsCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result GetTransactionId(out string outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			int inOutBufferLength = 65;
			Helper.TryMarshalAllocate(ref target, inOutBufferLength, out var _);
			Result result = Bindings.EOS_Ecom_Transaction_GetTransactionId(base.InnerHandle, target, ref inOutBufferLength);
			Helper.TryMarshalGet(target, out outBuffer);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void Release()
		{
			Bindings.EOS_Ecom_Transaction_Release(base.InnerHandle);
		}
	}
}
