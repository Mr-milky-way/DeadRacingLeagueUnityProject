using System;

namespace Epic.OnlineServices.Connect
{
	public sealed class ConnectInterface : Handle
	{
		public const int AddnotifyauthexpirationApiLatest = 1;

		public const int AddnotifyloginstatuschangedApiLatest = 1;

		public const int CopyproductuserexternalaccountbyaccountidApiLatest = 1;

		public const int CopyproductuserexternalaccountbyaccounttypeApiLatest = 1;

		public const int CopyproductuserexternalaccountbyindexApiLatest = 1;

		public const int CopyproductuserinfoApiLatest = 1;

		public const int CreatedeviceidApiLatest = 1;

		public const int CreatedeviceidDevicemodelMaxLength = 64;

		public const int CreateuserApiLatest = 1;

		public const int CredentialsApiLatest = 1;

		public const int DeletedeviceidApiLatest = 1;

		public const int ExternalAccountIdMaxLength = 256;

		public const int ExternalaccountinfoApiLatest = 1;

		public const int GetexternalaccountmappingApiLatest = 1;

		public const int GetexternalaccountmappingsApiLatest = 1;

		public const int GetproductuserexternalaccountcountApiLatest = 1;

		public const int GetproductuseridmappingApiLatest = 1;

		public const int LinkaccountApiLatest = 1;

		public const int LoginApiLatest = 2;

		public const int OnauthexpirationcallbackApiLatest = 1;

		public const int QueryexternalaccountmappingsApiLatest = 1;

		public const int QueryexternalaccountmappingsMaxAccountIds = 128;

		public const int QueryproductuseridmappingsApiLatest = 2;

		public const int TimeUndefined = -1;

		public const int TransferdeviceidaccountApiLatest = 1;

		public const int UnlinkaccountApiLatest = 1;

		public const int UserlogininfoApiLatest = 1;

		public const int UserlogininfoDisplaynameMaxLength = 32;

		public ConnectInterface()
		{
		}

		public ConnectInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public ulong AddNotifyAuthExpiration(AddNotifyAuthExpirationOptions options, object clientData, OnAuthExpirationCallback notification)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyAuthExpirationOptionsInternal, AddNotifyAuthExpirationOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnAuthExpirationCallbackInternal onAuthExpirationCallbackInternal = OnAuthExpirationCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notification, onAuthExpirationCallbackInternal);
			ulong num = Bindings.EOS_Connect_AddNotifyAuthExpiration(base.InnerHandle, target, clientDataAddress, onAuthExpirationCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyLoginStatusChanged(AddNotifyLoginStatusChangedOptions options, object clientData, OnLoginStatusChangedCallback notification)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyLoginStatusChangedOptionsInternal, AddNotifyLoginStatusChangedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLoginStatusChangedCallbackInternal onLoginStatusChangedCallbackInternal = OnLoginStatusChangedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notification, onLoginStatusChangedCallbackInternal);
			ulong num = Bindings.EOS_Connect_AddNotifyLoginStatusChanged(base.InnerHandle, target, clientDataAddress, onLoginStatusChangedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public Result CopyProductUserExternalAccountByAccountId(CopyProductUserExternalAccountByAccountIdOptions options, out ExternalAccountInfo outExternalAccountInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyProductUserExternalAccountByAccountIdOptionsInternal, CopyProductUserExternalAccountByAccountIdOptions>(ref target, options);
			IntPtr outExternalAccountInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_Connect_CopyProductUserExternalAccountByAccountId(base.InnerHandle, target, ref outExternalAccountInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<ExternalAccountInfoInternal, ExternalAccountInfo>(outExternalAccountInfo2, out outExternalAccountInfo))
			{
				Bindings.EOS_Connect_ExternalAccountInfo_Release(outExternalAccountInfo2);
			}
			return result;
		}

		public Result CopyProductUserExternalAccountByAccountType(CopyProductUserExternalAccountByAccountTypeOptions options, out ExternalAccountInfo outExternalAccountInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyProductUserExternalAccountByAccountTypeOptionsInternal, CopyProductUserExternalAccountByAccountTypeOptions>(ref target, options);
			IntPtr outExternalAccountInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_Connect_CopyProductUserExternalAccountByAccountType(base.InnerHandle, target, ref outExternalAccountInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<ExternalAccountInfoInternal, ExternalAccountInfo>(outExternalAccountInfo2, out outExternalAccountInfo))
			{
				Bindings.EOS_Connect_ExternalAccountInfo_Release(outExternalAccountInfo2);
			}
			return result;
		}

		public Result CopyProductUserExternalAccountByIndex(CopyProductUserExternalAccountByIndexOptions options, out ExternalAccountInfo outExternalAccountInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyProductUserExternalAccountByIndexOptionsInternal, CopyProductUserExternalAccountByIndexOptions>(ref target, options);
			IntPtr outExternalAccountInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_Connect_CopyProductUserExternalAccountByIndex(base.InnerHandle, target, ref outExternalAccountInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<ExternalAccountInfoInternal, ExternalAccountInfo>(outExternalAccountInfo2, out outExternalAccountInfo))
			{
				Bindings.EOS_Connect_ExternalAccountInfo_Release(outExternalAccountInfo2);
			}
			return result;
		}

		public Result CopyProductUserInfo(CopyProductUserInfoOptions options, out ExternalAccountInfo outExternalAccountInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyProductUserInfoOptionsInternal, CopyProductUserInfoOptions>(ref target, options);
			IntPtr outExternalAccountInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_Connect_CopyProductUserInfo(base.InnerHandle, target, ref outExternalAccountInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<ExternalAccountInfoInternal, ExternalAccountInfo>(outExternalAccountInfo2, out outExternalAccountInfo))
			{
				Bindings.EOS_Connect_ExternalAccountInfo_Release(outExternalAccountInfo2);
			}
			return result;
		}

		public void CreateDeviceId(CreateDeviceIdOptions options, object clientData, OnCreateDeviceIdCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CreateDeviceIdOptionsInternal, CreateDeviceIdOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnCreateDeviceIdCallbackInternal onCreateDeviceIdCallbackInternal = OnCreateDeviceIdCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onCreateDeviceIdCallbackInternal);
			Bindings.EOS_Connect_CreateDeviceId(base.InnerHandle, target, clientDataAddress, onCreateDeviceIdCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void CreateUser(CreateUserOptions options, object clientData, OnCreateUserCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CreateUserOptionsInternal, CreateUserOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnCreateUserCallbackInternal onCreateUserCallbackInternal = OnCreateUserCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onCreateUserCallbackInternal);
			Bindings.EOS_Connect_CreateUser(base.InnerHandle, target, clientDataAddress, onCreateUserCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void DeleteDeviceId(DeleteDeviceIdOptions options, object clientData, OnDeleteDeviceIdCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<DeleteDeviceIdOptionsInternal, DeleteDeviceIdOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnDeleteDeviceIdCallbackInternal onDeleteDeviceIdCallbackInternal = OnDeleteDeviceIdCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onDeleteDeviceIdCallbackInternal);
			Bindings.EOS_Connect_DeleteDeviceId(base.InnerHandle, target, clientDataAddress, onDeleteDeviceIdCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public ProductUserId GetExternalAccountMapping(GetExternalAccountMappingsOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetExternalAccountMappingsOptionsInternal, GetExternalAccountMappingsOptions>(ref target, options);
			IntPtr source = Bindings.EOS_Connect_GetExternalAccountMapping(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out ProductUserId target2);
			return target2;
		}

		public ProductUserId GetLoggedInUserByIndex(int index)
		{
			Helper.TryMarshalGet(Bindings.EOS_Connect_GetLoggedInUserByIndex(base.InnerHandle, index), out ProductUserId target);
			return target;
		}

		public int GetLoggedInUsersCount()
		{
			return Bindings.EOS_Connect_GetLoggedInUsersCount(base.InnerHandle);
		}

		public LoginStatus GetLoginStatus(ProductUserId localUserId)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet(ref target, localUserId);
			return Bindings.EOS_Connect_GetLoginStatus(base.InnerHandle, target);
		}

		public uint GetProductUserExternalAccountCount(GetProductUserExternalAccountCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetProductUserExternalAccountCountOptionsInternal, GetProductUserExternalAccountCountOptions>(ref target, options);
			uint result = Bindings.EOS_Connect_GetProductUserExternalAccountCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result GetProductUserIdMapping(GetProductUserIdMappingOptions options, out string outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetProductUserIdMappingOptionsInternal, GetProductUserIdMappingOptions>(ref target, options);
			IntPtr target2 = IntPtr.Zero;
			int inOutBufferLength = 257;
			Helper.TryMarshalAllocate(ref target2, inOutBufferLength, out var _);
			Result result = Bindings.EOS_Connect_GetProductUserIdMapping(base.InnerHandle, target, target2, ref inOutBufferLength);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(target2, out outBuffer);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public void LinkAccount(LinkAccountOptions options, object clientData, OnLinkAccountCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LinkAccountOptionsInternal, LinkAccountOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLinkAccountCallbackInternal onLinkAccountCallbackInternal = OnLinkAccountCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onLinkAccountCallbackInternal);
			Bindings.EOS_Connect_LinkAccount(base.InnerHandle, target, clientDataAddress, onLinkAccountCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void Login(LoginOptions options, object clientData, OnLoginCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LoginOptionsInternal, LoginOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLoginCallbackInternal onLoginCallbackInternal = OnLoginCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onLoginCallbackInternal);
			Bindings.EOS_Connect_Login(base.InnerHandle, target, clientDataAddress, onLoginCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryExternalAccountMappings(QueryExternalAccountMappingsOptions options, object clientData, OnQueryExternalAccountMappingsCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryExternalAccountMappingsOptionsInternal, QueryExternalAccountMappingsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryExternalAccountMappingsCallbackInternal onQueryExternalAccountMappingsCallbackInternal = OnQueryExternalAccountMappingsCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryExternalAccountMappingsCallbackInternal);
			Bindings.EOS_Connect_QueryExternalAccountMappings(base.InnerHandle, target, clientDataAddress, onQueryExternalAccountMappingsCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryProductUserIdMappings(QueryProductUserIdMappingsOptions options, object clientData, OnQueryProductUserIdMappingsCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryProductUserIdMappingsOptionsInternal, QueryProductUserIdMappingsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryProductUserIdMappingsCallbackInternal onQueryProductUserIdMappingsCallbackInternal = OnQueryProductUserIdMappingsCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryProductUserIdMappingsCallbackInternal);
			Bindings.EOS_Connect_QueryProductUserIdMappings(base.InnerHandle, target, clientDataAddress, onQueryProductUserIdMappingsCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RemoveNotifyAuthExpiration(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Connect_RemoveNotifyAuthExpiration(base.InnerHandle, inId);
		}

		public void RemoveNotifyLoginStatusChanged(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Connect_RemoveNotifyLoginStatusChanged(base.InnerHandle, inId);
		}

		public void TransferDeviceIdAccount(TransferDeviceIdAccountOptions options, object clientData, OnTransferDeviceIdAccountCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<TransferDeviceIdAccountOptionsInternal, TransferDeviceIdAccountOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnTransferDeviceIdAccountCallbackInternal onTransferDeviceIdAccountCallbackInternal = OnTransferDeviceIdAccountCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onTransferDeviceIdAccountCallbackInternal);
			Bindings.EOS_Connect_TransferDeviceIdAccount(base.InnerHandle, target, clientDataAddress, onTransferDeviceIdAccountCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void UnlinkAccount(UnlinkAccountOptions options, object clientData, OnUnlinkAccountCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UnlinkAccountOptionsInternal, UnlinkAccountOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnUnlinkAccountCallbackInternal onUnlinkAccountCallbackInternal = OnUnlinkAccountCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onUnlinkAccountCallbackInternal);
			Bindings.EOS_Connect_UnlinkAccount(base.InnerHandle, target, clientDataAddress, onUnlinkAccountCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnAuthExpirationCallbackInternal))]
		internal static void OnAuthExpirationCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnAuthExpirationCallback, AuthExpirationCallbackInfoInternal, AuthExpirationCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnCreateDeviceIdCallbackInternal))]
		internal static void OnCreateDeviceIdCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnCreateDeviceIdCallback, CreateDeviceIdCallbackInfoInternal, CreateDeviceIdCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnCreateUserCallbackInternal))]
		internal static void OnCreateUserCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnCreateUserCallback, CreateUserCallbackInfoInternal, CreateUserCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnDeleteDeviceIdCallbackInternal))]
		internal static void OnDeleteDeviceIdCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnDeleteDeviceIdCallback, DeleteDeviceIdCallbackInfoInternal, DeleteDeviceIdCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnLinkAccountCallbackInternal))]
		internal static void OnLinkAccountCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnLinkAccountCallback, LinkAccountCallbackInfoInternal, LinkAccountCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnLoginCallbackInternal))]
		internal static void OnLoginCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnLoginCallback, LoginCallbackInfoInternal, LoginCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnLoginStatusChangedCallbackInternal))]
		internal static void OnLoginStatusChangedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnLoginStatusChangedCallback, LoginStatusChangedCallbackInfoInternal, LoginStatusChangedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryExternalAccountMappingsCallbackInternal))]
		internal static void OnQueryExternalAccountMappingsCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryExternalAccountMappingsCallback, QueryExternalAccountMappingsCallbackInfoInternal, QueryExternalAccountMappingsCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryProductUserIdMappingsCallbackInternal))]
		internal static void OnQueryProductUserIdMappingsCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryProductUserIdMappingsCallback, QueryProductUserIdMappingsCallbackInfoInternal, QueryProductUserIdMappingsCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnTransferDeviceIdAccountCallbackInternal))]
		internal static void OnTransferDeviceIdAccountCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnTransferDeviceIdAccountCallback, TransferDeviceIdAccountCallbackInfoInternal, TransferDeviceIdAccountCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnUnlinkAccountCallbackInternal))]
		internal static void OnUnlinkAccountCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnUnlinkAccountCallback, UnlinkAccountCallbackInfoInternal, UnlinkAccountCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
