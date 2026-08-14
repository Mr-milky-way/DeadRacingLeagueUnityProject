using System;

namespace Epic.OnlineServices.Sessions
{
	public sealed class SessionsInterface : Handle
	{
		public const int AddnotifyjoinsessionacceptedApiLatest = 1;

		public const int AddnotifysessioninviteacceptedApiLatest = 1;

		public const int AddnotifysessioninvitereceivedApiLatest = 1;

		public const int AttributedataApiLatest = 1;

		public const int CopyactivesessionhandleApiLatest = 1;

		public const int CopysessionhandlebyinviteidApiLatest = 1;

		public const int CopysessionhandlebyuieventidApiLatest = 1;

		public const int CopysessionhandleforpresenceApiLatest = 1;

		public const int CreatesessionmodificationApiLatest = 3;

		public const int CreatesessionsearchApiLatest = 1;

		public const int DestroysessionApiLatest = 1;

		public const int DumpsessionstateApiLatest = 1;

		public const int EndsessionApiLatest = 1;

		public const int GetinvitecountApiLatest = 1;

		public const int GetinviteidbyindexApiLatest = 1;

		public const int InviteidMaxLength = 64;

		public const int IsuserinsessionApiLatest = 1;

		public const int JoinsessionApiLatest = 2;

		public const int MaxSearchResults = 200;

		public const int Maxregisteredplayers = 1000;

		public const int QueryinvitesApiLatest = 1;

		public const int RegisterplayersApiLatest = 1;

		public const int RejectinviteApiLatest = 1;

		public const string SearchBucketId = "bucket";

		public const string SearchEmptyServersOnly = "emptyonly";

		public const string SearchMinslotsavailable = "minslotsavailable";

		public const string SearchNonemptyServersOnly = "nonemptyonly";

		public const int SendinviteApiLatest = 1;

		public const int SessionattributeApiLatest = 1;

		public const int SessionattributedataApiLatest = 1;

		public const int StartsessionApiLatest = 1;

		public const int UnregisterplayersApiLatest = 1;

		public const int UpdatesessionApiLatest = 1;

		public const int UpdatesessionmodificationApiLatest = 1;

		public SessionsInterface()
		{
		}

		public SessionsInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public ulong AddNotifyJoinSessionAccepted(AddNotifyJoinSessionAcceptedOptions options, object clientData, OnJoinSessionAcceptedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyJoinSessionAcceptedOptionsInternal, AddNotifyJoinSessionAcceptedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnJoinSessionAcceptedCallbackInternal onJoinSessionAcceptedCallbackInternal = OnJoinSessionAcceptedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onJoinSessionAcceptedCallbackInternal);
			ulong num = Bindings.EOS_Sessions_AddNotifyJoinSessionAccepted(base.InnerHandle, target, clientDataAddress, onJoinSessionAcceptedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifySessionInviteAccepted(AddNotifySessionInviteAcceptedOptions options, object clientData, OnSessionInviteAcceptedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifySessionInviteAcceptedOptionsInternal, AddNotifySessionInviteAcceptedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnSessionInviteAcceptedCallbackInternal onSessionInviteAcceptedCallbackInternal = OnSessionInviteAcceptedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onSessionInviteAcceptedCallbackInternal);
			ulong num = Bindings.EOS_Sessions_AddNotifySessionInviteAccepted(base.InnerHandle, target, clientDataAddress, onSessionInviteAcceptedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifySessionInviteReceived(AddNotifySessionInviteReceivedOptions options, object clientData, OnSessionInviteReceivedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifySessionInviteReceivedOptionsInternal, AddNotifySessionInviteReceivedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnSessionInviteReceivedCallbackInternal onSessionInviteReceivedCallbackInternal = OnSessionInviteReceivedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onSessionInviteReceivedCallbackInternal);
			ulong num = Bindings.EOS_Sessions_AddNotifySessionInviteReceived(base.InnerHandle, target, clientDataAddress, onSessionInviteReceivedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public Result CopyActiveSessionHandle(CopyActiveSessionHandleOptions options, out ActiveSession outSessionHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyActiveSessionHandleOptionsInternal, CopyActiveSessionHandleOptions>(ref target, options);
			IntPtr outSessionHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Sessions_CopyActiveSessionHandle(base.InnerHandle, target, ref outSessionHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outSessionHandle2, out outSessionHandle);
			return result;
		}

		public Result CopySessionHandleByInviteId(CopySessionHandleByInviteIdOptions options, out SessionDetails outSessionHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopySessionHandleByInviteIdOptionsInternal, CopySessionHandleByInviteIdOptions>(ref target, options);
			IntPtr outSessionHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Sessions_CopySessionHandleByInviteId(base.InnerHandle, target, ref outSessionHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outSessionHandle2, out outSessionHandle);
			return result;
		}

		public Result CopySessionHandleByUiEventId(CopySessionHandleByUiEventIdOptions options, out SessionDetails outSessionHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopySessionHandleByUiEventIdOptionsInternal, CopySessionHandleByUiEventIdOptions>(ref target, options);
			IntPtr outSessionHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Sessions_CopySessionHandleByUiEventId(base.InnerHandle, target, ref outSessionHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outSessionHandle2, out outSessionHandle);
			return result;
		}

		public Result CopySessionHandleForPresence(CopySessionHandleForPresenceOptions options, out SessionDetails outSessionHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopySessionHandleForPresenceOptionsInternal, CopySessionHandleForPresenceOptions>(ref target, options);
			IntPtr outSessionHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Sessions_CopySessionHandleForPresence(base.InnerHandle, target, ref outSessionHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outSessionHandle2, out outSessionHandle);
			return result;
		}

		public Result CreateSessionModification(CreateSessionModificationOptions options, out SessionModification outSessionModificationHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CreateSessionModificationOptionsInternal, CreateSessionModificationOptions>(ref target, options);
			IntPtr outSessionModificationHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Sessions_CreateSessionModification(base.InnerHandle, target, ref outSessionModificationHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outSessionModificationHandle2, out outSessionModificationHandle);
			return result;
		}

		public Result CreateSessionSearch(CreateSessionSearchOptions options, out SessionSearch outSessionSearchHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CreateSessionSearchOptionsInternal, CreateSessionSearchOptions>(ref target, options);
			IntPtr outSessionSearchHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Sessions_CreateSessionSearch(base.InnerHandle, target, ref outSessionSearchHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outSessionSearchHandle2, out outSessionSearchHandle);
			return result;
		}

		public void DestroySession(DestroySessionOptions options, object clientData, OnDestroySessionCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<DestroySessionOptionsInternal, DestroySessionOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnDestroySessionCallbackInternal onDestroySessionCallbackInternal = OnDestroySessionCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onDestroySessionCallbackInternal);
			Bindings.EOS_Sessions_DestroySession(base.InnerHandle, target, clientDataAddress, onDestroySessionCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public Result DumpSessionState(DumpSessionStateOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<DumpSessionStateOptionsInternal, DumpSessionStateOptions>(ref target, options);
			Result result = Bindings.EOS_Sessions_DumpSessionState(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void EndSession(EndSessionOptions options, object clientData, OnEndSessionCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<EndSessionOptionsInternal, EndSessionOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnEndSessionCallbackInternal onEndSessionCallbackInternal = OnEndSessionCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onEndSessionCallbackInternal);
			Bindings.EOS_Sessions_EndSession(base.InnerHandle, target, clientDataAddress, onEndSessionCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public uint GetInviteCount(GetInviteCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetInviteCountOptionsInternal, GetInviteCountOptions>(ref target, options);
			uint result = Bindings.EOS_Sessions_GetInviteCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result GetInviteIdByIndex(GetInviteIdByIndexOptions options, out string outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetInviteIdByIndexOptionsInternal, GetInviteIdByIndexOptions>(ref target, options);
			IntPtr target2 = IntPtr.Zero;
			int inOutBufferLength = 65;
			Helper.TryMarshalAllocate(ref target2, inOutBufferLength, out var _);
			Result result = Bindings.EOS_Sessions_GetInviteIdByIndex(base.InnerHandle, target, target2, ref inOutBufferLength);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(target2, out outBuffer);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public Result IsUserInSession(IsUserInSessionOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<IsUserInSessionOptionsInternal, IsUserInSessionOptions>(ref target, options);
			Result result = Bindings.EOS_Sessions_IsUserInSession(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void JoinSession(JoinSessionOptions options, object clientData, OnJoinSessionCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<JoinSessionOptionsInternal, JoinSessionOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnJoinSessionCallbackInternal onJoinSessionCallbackInternal = OnJoinSessionCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onJoinSessionCallbackInternal);
			Bindings.EOS_Sessions_JoinSession(base.InnerHandle, target, clientDataAddress, onJoinSessionCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryInvites(QueryInvitesOptions options, object clientData, OnQueryInvitesCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryInvitesOptionsInternal, QueryInvitesOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryInvitesCallbackInternal onQueryInvitesCallbackInternal = OnQueryInvitesCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryInvitesCallbackInternal);
			Bindings.EOS_Sessions_QueryInvites(base.InnerHandle, target, clientDataAddress, onQueryInvitesCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RegisterPlayers(RegisterPlayersOptions options, object clientData, OnRegisterPlayersCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<RegisterPlayersOptionsInternal, RegisterPlayersOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnRegisterPlayersCallbackInternal onRegisterPlayersCallbackInternal = OnRegisterPlayersCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onRegisterPlayersCallbackInternal);
			Bindings.EOS_Sessions_RegisterPlayers(base.InnerHandle, target, clientDataAddress, onRegisterPlayersCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RejectInvite(RejectInviteOptions options, object clientData, OnRejectInviteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<RejectInviteOptionsInternal, RejectInviteOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnRejectInviteCallbackInternal onRejectInviteCallbackInternal = OnRejectInviteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onRejectInviteCallbackInternal);
			Bindings.EOS_Sessions_RejectInvite(base.InnerHandle, target, clientDataAddress, onRejectInviteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RemoveNotifyJoinSessionAccepted(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Sessions_RemoveNotifyJoinSessionAccepted(base.InnerHandle, inId);
		}

		public void RemoveNotifySessionInviteAccepted(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Sessions_RemoveNotifySessionInviteAccepted(base.InnerHandle, inId);
		}

		public void RemoveNotifySessionInviteReceived(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Sessions_RemoveNotifySessionInviteReceived(base.InnerHandle, inId);
		}

		public void SendInvite(SendInviteOptions options, object clientData, OnSendInviteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SendInviteOptionsInternal, SendInviteOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnSendInviteCallbackInternal onSendInviteCallbackInternal = OnSendInviteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onSendInviteCallbackInternal);
			Bindings.EOS_Sessions_SendInvite(base.InnerHandle, target, clientDataAddress, onSendInviteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void StartSession(StartSessionOptions options, object clientData, OnStartSessionCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<StartSessionOptionsInternal, StartSessionOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnStartSessionCallbackInternal onStartSessionCallbackInternal = OnStartSessionCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onStartSessionCallbackInternal);
			Bindings.EOS_Sessions_StartSession(base.InnerHandle, target, clientDataAddress, onStartSessionCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void UnregisterPlayers(UnregisterPlayersOptions options, object clientData, OnUnregisterPlayersCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UnregisterPlayersOptionsInternal, UnregisterPlayersOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnUnregisterPlayersCallbackInternal onUnregisterPlayersCallbackInternal = OnUnregisterPlayersCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onUnregisterPlayersCallbackInternal);
			Bindings.EOS_Sessions_UnregisterPlayers(base.InnerHandle, target, clientDataAddress, onUnregisterPlayersCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void UpdateSession(UpdateSessionOptions options, object clientData, OnUpdateSessionCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UpdateSessionOptionsInternal, UpdateSessionOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnUpdateSessionCallbackInternal onUpdateSessionCallbackInternal = OnUpdateSessionCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onUpdateSessionCallbackInternal);
			Bindings.EOS_Sessions_UpdateSession(base.InnerHandle, target, clientDataAddress, onUpdateSessionCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public Result UpdateSessionModification(UpdateSessionModificationOptions options, out SessionModification outSessionModificationHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UpdateSessionModificationOptionsInternal, UpdateSessionModificationOptions>(ref target, options);
			IntPtr outSessionModificationHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Sessions_UpdateSessionModification(base.InnerHandle, target, ref outSessionModificationHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outSessionModificationHandle2, out outSessionModificationHandle);
			return result;
		}

		[MonoPInvokeCallback(typeof(OnDestroySessionCallbackInternal))]
		internal static void OnDestroySessionCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnDestroySessionCallback, DestroySessionCallbackInfoInternal, DestroySessionCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnEndSessionCallbackInternal))]
		internal static void OnEndSessionCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnEndSessionCallback, EndSessionCallbackInfoInternal, EndSessionCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnJoinSessionAcceptedCallbackInternal))]
		internal static void OnJoinSessionAcceptedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnJoinSessionAcceptedCallback, JoinSessionAcceptedCallbackInfoInternal, JoinSessionAcceptedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnJoinSessionCallbackInternal))]
		internal static void OnJoinSessionCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnJoinSessionCallback, JoinSessionCallbackInfoInternal, JoinSessionCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryInvitesCallbackInternal))]
		internal static void OnQueryInvitesCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryInvitesCallback, QueryInvitesCallbackInfoInternal, QueryInvitesCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnRegisterPlayersCallbackInternal))]
		internal static void OnRegisterPlayersCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnRegisterPlayersCallback, RegisterPlayersCallbackInfoInternal, RegisterPlayersCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnRejectInviteCallbackInternal))]
		internal static void OnRejectInviteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnRejectInviteCallback, RejectInviteCallbackInfoInternal, RejectInviteCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnSendInviteCallbackInternal))]
		internal static void OnSendInviteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnSendInviteCallback, SendInviteCallbackInfoInternal, SendInviteCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnSessionInviteAcceptedCallbackInternal))]
		internal static void OnSessionInviteAcceptedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnSessionInviteAcceptedCallback, SessionInviteAcceptedCallbackInfoInternal, SessionInviteAcceptedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnSessionInviteReceivedCallbackInternal))]
		internal static void OnSessionInviteReceivedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnSessionInviteReceivedCallback, SessionInviteReceivedCallbackInfoInternal, SessionInviteReceivedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnStartSessionCallbackInternal))]
		internal static void OnStartSessionCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnStartSessionCallback, StartSessionCallbackInfoInternal, StartSessionCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnUnregisterPlayersCallbackInternal))]
		internal static void OnUnregisterPlayersCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnUnregisterPlayersCallback, UnregisterPlayersCallbackInfoInternal, UnregisterPlayersCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnUpdateSessionCallbackInternal))]
		internal static void OnUpdateSessionCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnUpdateSessionCallback, UpdateSessionCallbackInfoInternal, UpdateSessionCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
