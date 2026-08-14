using System;

namespace Epic.OnlineServices.UserInfo
{
	public sealed class UserInfoInterface : Handle
	{
		public const int CopyexternaluserinfobyaccountidApiLatest = 1;

		public const int CopyexternaluserinfobyaccounttypeApiLatest = 1;

		public const int CopyexternaluserinfobyindexApiLatest = 1;

		public const int CopyuserinfoApiLatest = 2;

		public const int ExternaluserinfoApiLatest = 1;

		public const int GetexternaluserinfocountApiLatest = 1;

		public const int MaxDisplaynameCharacters = 16;

		public const int MaxDisplaynameUtf8Length = 64;

		public const int QueryuserinfoApiLatest = 1;

		public const int QueryuserinfobydisplaynameApiLatest = 1;

		public const int QueryuserinfobyexternalaccountApiLatest = 1;

		public UserInfoInterface()
		{
		}

		public UserInfoInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopyExternalUserInfoByAccountId(CopyExternalUserInfoByAccountIdOptions options, out ExternalUserInfo outExternalUserInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyExternalUserInfoByAccountIdOptionsInternal, CopyExternalUserInfoByAccountIdOptions>(ref target, options);
			IntPtr outExternalUserInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_UserInfo_CopyExternalUserInfoByAccountId(base.InnerHandle, target, ref outExternalUserInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<ExternalUserInfoInternal, ExternalUserInfo>(outExternalUserInfo2, out outExternalUserInfo))
			{
				Bindings.EOS_UserInfo_ExternalUserInfo_Release(outExternalUserInfo2);
			}
			return result;
		}

		public Result CopyExternalUserInfoByAccountType(CopyExternalUserInfoByAccountTypeOptions options, out ExternalUserInfo outExternalUserInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyExternalUserInfoByAccountTypeOptionsInternal, CopyExternalUserInfoByAccountTypeOptions>(ref target, options);
			IntPtr outExternalUserInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_UserInfo_CopyExternalUserInfoByAccountType(base.InnerHandle, target, ref outExternalUserInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<ExternalUserInfoInternal, ExternalUserInfo>(outExternalUserInfo2, out outExternalUserInfo))
			{
				Bindings.EOS_UserInfo_ExternalUserInfo_Release(outExternalUserInfo2);
			}
			return result;
		}

		public Result CopyExternalUserInfoByIndex(CopyExternalUserInfoByIndexOptions options, out ExternalUserInfo outExternalUserInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyExternalUserInfoByIndexOptionsInternal, CopyExternalUserInfoByIndexOptions>(ref target, options);
			IntPtr outExternalUserInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_UserInfo_CopyExternalUserInfoByIndex(base.InnerHandle, target, ref outExternalUserInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<ExternalUserInfoInternal, ExternalUserInfo>(outExternalUserInfo2, out outExternalUserInfo))
			{
				Bindings.EOS_UserInfo_ExternalUserInfo_Release(outExternalUserInfo2);
			}
			return result;
		}

		public Result CopyUserInfo(CopyUserInfoOptions options, out UserInfoData outUserInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyUserInfoOptionsInternal, CopyUserInfoOptions>(ref target, options);
			IntPtr outUserInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_UserInfo_CopyUserInfo(base.InnerHandle, target, ref outUserInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<UserInfoDataInternal, UserInfoData>(outUserInfo2, out outUserInfo))
			{
				Bindings.EOS_UserInfo_Release(outUserInfo2);
			}
			return result;
		}

		public uint GetExternalUserInfoCount(GetExternalUserInfoCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetExternalUserInfoCountOptionsInternal, GetExternalUserInfoCountOptions>(ref target, options);
			uint result = Bindings.EOS_UserInfo_GetExternalUserInfoCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void QueryUserInfo(QueryUserInfoOptions options, object clientData, OnQueryUserInfoCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryUserInfoOptionsInternal, QueryUserInfoOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryUserInfoCallbackInternal onQueryUserInfoCallbackInternal = OnQueryUserInfoCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryUserInfoCallbackInternal);
			Bindings.EOS_UserInfo_QueryUserInfo(base.InnerHandle, target, clientDataAddress, onQueryUserInfoCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryUserInfoByDisplayName(QueryUserInfoByDisplayNameOptions options, object clientData, OnQueryUserInfoByDisplayNameCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryUserInfoByDisplayNameOptionsInternal, QueryUserInfoByDisplayNameOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryUserInfoByDisplayNameCallbackInternal onQueryUserInfoByDisplayNameCallbackInternal = OnQueryUserInfoByDisplayNameCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryUserInfoByDisplayNameCallbackInternal);
			Bindings.EOS_UserInfo_QueryUserInfoByDisplayName(base.InnerHandle, target, clientDataAddress, onQueryUserInfoByDisplayNameCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryUserInfoByExternalAccount(QueryUserInfoByExternalAccountOptions options, object clientData, OnQueryUserInfoByExternalAccountCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryUserInfoByExternalAccountOptionsInternal, QueryUserInfoByExternalAccountOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryUserInfoByExternalAccountCallbackInternal onQueryUserInfoByExternalAccountCallbackInternal = OnQueryUserInfoByExternalAccountCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryUserInfoByExternalAccountCallbackInternal);
			Bindings.EOS_UserInfo_QueryUserInfoByExternalAccount(base.InnerHandle, target, clientDataAddress, onQueryUserInfoByExternalAccountCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnQueryUserInfoByDisplayNameCallbackInternal))]
		internal static void OnQueryUserInfoByDisplayNameCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryUserInfoByDisplayNameCallback, QueryUserInfoByDisplayNameCallbackInfoInternal, QueryUserInfoByDisplayNameCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryUserInfoByExternalAccountCallbackInternal))]
		internal static void OnQueryUserInfoByExternalAccountCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryUserInfoByExternalAccountCallback, QueryUserInfoByExternalAccountCallbackInfoInternal, QueryUserInfoByExternalAccountCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryUserInfoCallbackInternal))]
		internal static void OnQueryUserInfoCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryUserInfoCallback, QueryUserInfoCallbackInfoInternal, QueryUserInfoCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
