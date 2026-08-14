using System;

namespace Epic.OnlineServices.KWS
{
	public sealed class KWSInterface : Handle
	{
		public const int AddnotifypermissionsupdatereceivedApiLatest = 1;

		public const int CopypermissionbyindexApiLatest = 1;

		public const int CreateuserApiLatest = 1;

		public const int GetpermissionbykeyApiLatest = 1;

		public const int GetpermissionscountApiLatest = 1;

		public const int MaxPermissionLength = 32;

		public const int MaxPermissions = 16;

		public const int PermissionstatusApiLatest = 1;

		public const int QueryagegateApiLatest = 1;

		public const int QuerypermissionsApiLatest = 1;

		public const int RequestpermissionsApiLatest = 1;

		public const int UpdateparentemailApiLatest = 1;

		public KWSInterface()
		{
		}

		public KWSInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public ulong AddNotifyPermissionsUpdateReceived(AddNotifyPermissionsUpdateReceivedOptions options, object clientData, OnPermissionsUpdateReceivedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyPermissionsUpdateReceivedOptionsInternal, AddNotifyPermissionsUpdateReceivedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnPermissionsUpdateReceivedCallbackInternal onPermissionsUpdateReceivedCallbackInternal = OnPermissionsUpdateReceivedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onPermissionsUpdateReceivedCallbackInternal);
			ulong num = Bindings.EOS_KWS_AddNotifyPermissionsUpdateReceived(base.InnerHandle, target, clientDataAddress, onPermissionsUpdateReceivedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public Result CopyPermissionByIndex(CopyPermissionByIndexOptions options, out PermissionStatus outPermission)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyPermissionByIndexOptionsInternal, CopyPermissionByIndexOptions>(ref target, options);
			IntPtr outPermission2 = IntPtr.Zero;
			Result result = Bindings.EOS_KWS_CopyPermissionByIndex(base.InnerHandle, target, ref outPermission2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<PermissionStatusInternal, PermissionStatus>(outPermission2, out outPermission))
			{
				Bindings.EOS_KWS_PermissionStatus_Release(outPermission2);
			}
			return result;
		}

		public void CreateUser(CreateUserOptions options, object clientData, OnCreateUserCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CreateUserOptionsInternal, CreateUserOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnCreateUserCallbackInternal onCreateUserCallbackInternal = OnCreateUserCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onCreateUserCallbackInternal);
			Bindings.EOS_KWS_CreateUser(base.InnerHandle, target, clientDataAddress, onCreateUserCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public Result GetPermissionByKey(GetPermissionByKeyOptions options, out KWSPermissionStatus outPermission)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetPermissionByKeyOptionsInternal, GetPermissionByKeyOptions>(ref target, options);
			outPermission = Helper.GetDefault<KWSPermissionStatus>();
			Result result = Bindings.EOS_KWS_GetPermissionByKey(base.InnerHandle, target, ref outPermission);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public int GetPermissionsCount(GetPermissionsCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetPermissionsCountOptionsInternal, GetPermissionsCountOptions>(ref target, options);
			int result = Bindings.EOS_KWS_GetPermissionsCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void QueryAgeGate(QueryAgeGateOptions options, object clientData, OnQueryAgeGateCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryAgeGateOptionsInternal, QueryAgeGateOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryAgeGateCallbackInternal onQueryAgeGateCallbackInternal = OnQueryAgeGateCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryAgeGateCallbackInternal);
			Bindings.EOS_KWS_QueryAgeGate(base.InnerHandle, target, clientDataAddress, onQueryAgeGateCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryPermissions(QueryPermissionsOptions options, object clientData, OnQueryPermissionsCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryPermissionsOptionsInternal, QueryPermissionsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryPermissionsCallbackInternal onQueryPermissionsCallbackInternal = OnQueryPermissionsCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryPermissionsCallbackInternal);
			Bindings.EOS_KWS_QueryPermissions(base.InnerHandle, target, clientDataAddress, onQueryPermissionsCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RemoveNotifyPermissionsUpdateReceived(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_KWS_RemoveNotifyPermissionsUpdateReceived(base.InnerHandle, inId);
		}

		public void RequestPermissions(RequestPermissionsOptions options, object clientData, OnRequestPermissionsCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<RequestPermissionsOptionsInternal, RequestPermissionsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnRequestPermissionsCallbackInternal onRequestPermissionsCallbackInternal = OnRequestPermissionsCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onRequestPermissionsCallbackInternal);
			Bindings.EOS_KWS_RequestPermissions(base.InnerHandle, target, clientDataAddress, onRequestPermissionsCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void UpdateParentEmail(UpdateParentEmailOptions options, object clientData, OnUpdateParentEmailCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UpdateParentEmailOptionsInternal, UpdateParentEmailOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnUpdateParentEmailCallbackInternal onUpdateParentEmailCallbackInternal = OnUpdateParentEmailCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onUpdateParentEmailCallbackInternal);
			Bindings.EOS_KWS_UpdateParentEmail(base.InnerHandle, target, clientDataAddress, onUpdateParentEmailCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnCreateUserCallbackInternal))]
		internal static void OnCreateUserCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnCreateUserCallback, CreateUserCallbackInfoInternal, CreateUserCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnPermissionsUpdateReceivedCallbackInternal))]
		internal static void OnPermissionsUpdateReceivedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnPermissionsUpdateReceivedCallback, PermissionsUpdateReceivedCallbackInfoInternal, PermissionsUpdateReceivedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryAgeGateCallbackInternal))]
		internal static void OnQueryAgeGateCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryAgeGateCallback, QueryAgeGateCallbackInfoInternal, QueryAgeGateCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryPermissionsCallbackInternal))]
		internal static void OnQueryPermissionsCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryPermissionsCallback, QueryPermissionsCallbackInfoInternal, QueryPermissionsCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnRequestPermissionsCallbackInternal))]
		internal static void OnRequestPermissionsCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnRequestPermissionsCallback, RequestPermissionsCallbackInfoInternal, RequestPermissionsCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnUpdateParentEmailCallbackInternal))]
		internal static void OnUpdateParentEmailCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnUpdateParentEmailCallback, UpdateParentEmailCallbackInfoInternal, UpdateParentEmailCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
