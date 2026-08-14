using System;

namespace Epic.OnlineServices.Ecom
{
	public sealed class EcomInterface : Handle
	{
		public const int CatalogitemApiLatest = 1;

		public const int CatalogitemEntitlementendtimestampUndefined = -1;

		public const int CatalogofferApiLatest = 3;

		public const int CatalogofferExpirationtimestampUndefined = -1;

		public const int CatalogreleaseApiLatest = 1;

		public const int CheckoutApiLatest = 1;

		public const int CheckoutMaxEntries = 10;

		public const int CheckoutentryApiLatest = 1;

		public const int CopyentitlementbyidApiLatest = 2;

		public const int CopyentitlementbyindexApiLatest = 1;

		public const int CopyentitlementbynameandindexApiLatest = 1;

		public const int CopyitembyidApiLatest = 1;

		public const int CopyitemimageinfobyindexApiLatest = 1;

		public const int CopyitemreleasebyindexApiLatest = 1;

		public const int CopyofferbyidApiLatest = 2;

		public const int CopyofferbyindexApiLatest = 2;

		public const int CopyofferimageinfobyindexApiLatest = 1;

		public const int CopyofferitembyindexApiLatest = 1;

		public const int CopytransactionbyidApiLatest = 1;

		public const int CopytransactionbyindexApiLatest = 1;

		public const int EntitlementApiLatest = 2;

		public const int EntitlementEndtimestampUndefined = -1;

		public const int GetentitlementsbynamecountApiLatest = 1;

		public const int GetentitlementscountApiLatest = 1;

		public const int GetitemimageinfocountApiLatest = 1;

		public const int GetitemreleasecountApiLatest = 1;

		public const int GetoffercountApiLatest = 1;

		public const int GetofferimageinfocountApiLatest = 1;

		public const int GetofferitemcountApiLatest = 1;

		public const int GettransactioncountApiLatest = 1;

		public const int ItemownershipApiLatest = 1;

		public const int KeyimageinfoApiLatest = 1;

		public const int QueryentitlementsApiLatest = 2;

		public const int QueryentitlementsMaxEntitlementIds = 32;

		public const int QueryoffersApiLatest = 1;

		public const int QueryownershipApiLatest = 2;

		public const int QueryownershipMaxCatalogIds = 32;

		public const int QueryownershiptokenApiLatest = 2;

		public const int QueryownershiptokenMaxCatalogitemIds = 32;

		public const int RedeementitlementsApiLatest = 1;

		public const int RedeementitlementsMaxIds = 32;

		public const int TransactionidMaximumLength = 64;

		public EcomInterface()
		{
		}

		public EcomInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public void Checkout(CheckoutOptions options, object clientData, OnCheckoutCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CheckoutOptionsInternal, CheckoutOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnCheckoutCallbackInternal onCheckoutCallbackInternal = OnCheckoutCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onCheckoutCallbackInternal);
			Bindings.EOS_Ecom_Checkout(base.InnerHandle, target, clientDataAddress, onCheckoutCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public Result CopyEntitlementById(CopyEntitlementByIdOptions options, out Entitlement outEntitlement)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyEntitlementByIdOptionsInternal, CopyEntitlementByIdOptions>(ref target, options);
			IntPtr outEntitlement2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_CopyEntitlementById(base.InnerHandle, target, ref outEntitlement2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<EntitlementInternal, Entitlement>(outEntitlement2, out outEntitlement))
			{
				Bindings.EOS_Ecom_Entitlement_Release(outEntitlement2);
			}
			return result;
		}

		public Result CopyEntitlementByIndex(CopyEntitlementByIndexOptions options, out Entitlement outEntitlement)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyEntitlementByIndexOptionsInternal, CopyEntitlementByIndexOptions>(ref target, options);
			IntPtr outEntitlement2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_CopyEntitlementByIndex(base.InnerHandle, target, ref outEntitlement2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<EntitlementInternal, Entitlement>(outEntitlement2, out outEntitlement))
			{
				Bindings.EOS_Ecom_Entitlement_Release(outEntitlement2);
			}
			return result;
		}

		public Result CopyEntitlementByNameAndIndex(CopyEntitlementByNameAndIndexOptions options, out Entitlement outEntitlement)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyEntitlementByNameAndIndexOptionsInternal, CopyEntitlementByNameAndIndexOptions>(ref target, options);
			IntPtr outEntitlement2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_CopyEntitlementByNameAndIndex(base.InnerHandle, target, ref outEntitlement2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<EntitlementInternal, Entitlement>(outEntitlement2, out outEntitlement))
			{
				Bindings.EOS_Ecom_Entitlement_Release(outEntitlement2);
			}
			return result;
		}

		public Result CopyItemById(CopyItemByIdOptions options, out CatalogItem outItem)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyItemByIdOptionsInternal, CopyItemByIdOptions>(ref target, options);
			IntPtr outItem2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_CopyItemById(base.InnerHandle, target, ref outItem2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<CatalogItemInternal, CatalogItem>(outItem2, out outItem))
			{
				Bindings.EOS_Ecom_CatalogItem_Release(outItem2);
			}
			return result;
		}

		public Result CopyItemImageInfoByIndex(CopyItemImageInfoByIndexOptions options, out KeyImageInfo outImageInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyItemImageInfoByIndexOptionsInternal, CopyItemImageInfoByIndexOptions>(ref target, options);
			IntPtr outImageInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_CopyItemImageInfoByIndex(base.InnerHandle, target, ref outImageInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<KeyImageInfoInternal, KeyImageInfo>(outImageInfo2, out outImageInfo))
			{
				Bindings.EOS_Ecom_KeyImageInfo_Release(outImageInfo2);
			}
			return result;
		}

		public Result CopyItemReleaseByIndex(CopyItemReleaseByIndexOptions options, out CatalogRelease outRelease)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyItemReleaseByIndexOptionsInternal, CopyItemReleaseByIndexOptions>(ref target, options);
			IntPtr outRelease2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_CopyItemReleaseByIndex(base.InnerHandle, target, ref outRelease2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<CatalogReleaseInternal, CatalogRelease>(outRelease2, out outRelease))
			{
				Bindings.EOS_Ecom_CatalogRelease_Release(outRelease2);
			}
			return result;
		}

		public Result CopyOfferById(CopyOfferByIdOptions options, out CatalogOffer outOffer)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyOfferByIdOptionsInternal, CopyOfferByIdOptions>(ref target, options);
			IntPtr outOffer2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_CopyOfferById(base.InnerHandle, target, ref outOffer2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<CatalogOfferInternal, CatalogOffer>(outOffer2, out outOffer))
			{
				Bindings.EOS_Ecom_CatalogOffer_Release(outOffer2);
			}
			return result;
		}

		public Result CopyOfferByIndex(CopyOfferByIndexOptions options, out CatalogOffer outOffer)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyOfferByIndexOptionsInternal, CopyOfferByIndexOptions>(ref target, options);
			IntPtr outOffer2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_CopyOfferByIndex(base.InnerHandle, target, ref outOffer2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<CatalogOfferInternal, CatalogOffer>(outOffer2, out outOffer))
			{
				Bindings.EOS_Ecom_CatalogOffer_Release(outOffer2);
			}
			return result;
		}

		public Result CopyOfferImageInfoByIndex(CopyOfferImageInfoByIndexOptions options, out KeyImageInfo outImageInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyOfferImageInfoByIndexOptionsInternal, CopyOfferImageInfoByIndexOptions>(ref target, options);
			IntPtr outImageInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_CopyOfferImageInfoByIndex(base.InnerHandle, target, ref outImageInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<KeyImageInfoInternal, KeyImageInfo>(outImageInfo2, out outImageInfo))
			{
				Bindings.EOS_Ecom_KeyImageInfo_Release(outImageInfo2);
			}
			return result;
		}

		public Result CopyOfferItemByIndex(CopyOfferItemByIndexOptions options, out CatalogItem outItem)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyOfferItemByIndexOptionsInternal, CopyOfferItemByIndexOptions>(ref target, options);
			IntPtr outItem2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_CopyOfferItemByIndex(base.InnerHandle, target, ref outItem2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<CatalogItemInternal, CatalogItem>(outItem2, out outItem))
			{
				Bindings.EOS_Ecom_CatalogItem_Release(outItem2);
			}
			return result;
		}

		public Result CopyTransactionById(CopyTransactionByIdOptions options, out Transaction outTransaction)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyTransactionByIdOptionsInternal, CopyTransactionByIdOptions>(ref target, options);
			IntPtr outTransaction2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_CopyTransactionById(base.InnerHandle, target, ref outTransaction2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outTransaction2, out outTransaction);
			return result;
		}

		public Result CopyTransactionByIndex(CopyTransactionByIndexOptions options, out Transaction outTransaction)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyTransactionByIndexOptionsInternal, CopyTransactionByIndexOptions>(ref target, options);
			IntPtr outTransaction2 = IntPtr.Zero;
			Result result = Bindings.EOS_Ecom_CopyTransactionByIndex(base.InnerHandle, target, ref outTransaction2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outTransaction2, out outTransaction);
			return result;
		}

		public uint GetEntitlementsByNameCount(GetEntitlementsByNameCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetEntitlementsByNameCountOptionsInternal, GetEntitlementsByNameCountOptions>(ref target, options);
			uint result = Bindings.EOS_Ecom_GetEntitlementsByNameCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public uint GetEntitlementsCount(GetEntitlementsCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetEntitlementsCountOptionsInternal, GetEntitlementsCountOptions>(ref target, options);
			uint result = Bindings.EOS_Ecom_GetEntitlementsCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public uint GetItemImageInfoCount(GetItemImageInfoCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetItemImageInfoCountOptionsInternal, GetItemImageInfoCountOptions>(ref target, options);
			uint result = Bindings.EOS_Ecom_GetItemImageInfoCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public uint GetItemReleaseCount(GetItemReleaseCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetItemReleaseCountOptionsInternal, GetItemReleaseCountOptions>(ref target, options);
			uint result = Bindings.EOS_Ecom_GetItemReleaseCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public uint GetOfferCount(GetOfferCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetOfferCountOptionsInternal, GetOfferCountOptions>(ref target, options);
			uint result = Bindings.EOS_Ecom_GetOfferCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public uint GetOfferImageInfoCount(GetOfferImageInfoCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetOfferImageInfoCountOptionsInternal, GetOfferImageInfoCountOptions>(ref target, options);
			uint result = Bindings.EOS_Ecom_GetOfferImageInfoCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public uint GetOfferItemCount(GetOfferItemCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetOfferItemCountOptionsInternal, GetOfferItemCountOptions>(ref target, options);
			uint result = Bindings.EOS_Ecom_GetOfferItemCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public uint GetTransactionCount(GetTransactionCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetTransactionCountOptionsInternal, GetTransactionCountOptions>(ref target, options);
			uint result = Bindings.EOS_Ecom_GetTransactionCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void QueryEntitlements(QueryEntitlementsOptions options, object clientData, OnQueryEntitlementsCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryEntitlementsOptionsInternal, QueryEntitlementsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryEntitlementsCallbackInternal onQueryEntitlementsCallbackInternal = OnQueryEntitlementsCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryEntitlementsCallbackInternal);
			Bindings.EOS_Ecom_QueryEntitlements(base.InnerHandle, target, clientDataAddress, onQueryEntitlementsCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryOffers(QueryOffersOptions options, object clientData, OnQueryOffersCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryOffersOptionsInternal, QueryOffersOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryOffersCallbackInternal onQueryOffersCallbackInternal = OnQueryOffersCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryOffersCallbackInternal);
			Bindings.EOS_Ecom_QueryOffers(base.InnerHandle, target, clientDataAddress, onQueryOffersCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryOwnership(QueryOwnershipOptions options, object clientData, OnQueryOwnershipCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryOwnershipOptionsInternal, QueryOwnershipOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryOwnershipCallbackInternal onQueryOwnershipCallbackInternal = OnQueryOwnershipCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryOwnershipCallbackInternal);
			Bindings.EOS_Ecom_QueryOwnership(base.InnerHandle, target, clientDataAddress, onQueryOwnershipCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryOwnershipToken(QueryOwnershipTokenOptions options, object clientData, OnQueryOwnershipTokenCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryOwnershipTokenOptionsInternal, QueryOwnershipTokenOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryOwnershipTokenCallbackInternal onQueryOwnershipTokenCallbackInternal = OnQueryOwnershipTokenCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryOwnershipTokenCallbackInternal);
			Bindings.EOS_Ecom_QueryOwnershipToken(base.InnerHandle, target, clientDataAddress, onQueryOwnershipTokenCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RedeemEntitlements(RedeemEntitlementsOptions options, object clientData, OnRedeemEntitlementsCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<RedeemEntitlementsOptionsInternal, RedeemEntitlementsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnRedeemEntitlementsCallbackInternal onRedeemEntitlementsCallbackInternal = OnRedeemEntitlementsCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onRedeemEntitlementsCallbackInternal);
			Bindings.EOS_Ecom_RedeemEntitlements(base.InnerHandle, target, clientDataAddress, onRedeemEntitlementsCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnCheckoutCallbackInternal))]
		internal static void OnCheckoutCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnCheckoutCallback, CheckoutCallbackInfoInternal, CheckoutCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryEntitlementsCallbackInternal))]
		internal static void OnQueryEntitlementsCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryEntitlementsCallback, QueryEntitlementsCallbackInfoInternal, QueryEntitlementsCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryOffersCallbackInternal))]
		internal static void OnQueryOffersCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryOffersCallback, QueryOffersCallbackInfoInternal, QueryOffersCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryOwnershipCallbackInternal))]
		internal static void OnQueryOwnershipCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryOwnershipCallback, QueryOwnershipCallbackInfoInternal, QueryOwnershipCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryOwnershipTokenCallbackInternal))]
		internal static void OnQueryOwnershipTokenCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryOwnershipTokenCallback, QueryOwnershipTokenCallbackInfoInternal, QueryOwnershipTokenCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnRedeemEntitlementsCallbackInternal))]
		internal static void OnRedeemEntitlementsCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnRedeemEntitlementsCallback, RedeemEntitlementsCallbackInfoInternal, RedeemEntitlementsCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
