using System;

namespace Epic.OnlineServices.Lobby
{
	public sealed class LobbyInterface : Handle
	{
		public const int AddnotifyjoinlobbyacceptedApiLatest = 1;

		public const int AddnotifylobbyinviteacceptedApiLatest = 1;

		public const int AddnotifylobbyinvitereceivedApiLatest = 1;

		public const int AddnotifylobbymemberstatusreceivedApiLatest = 1;

		public const int AddnotifylobbymemberupdatereceivedApiLatest = 1;

		public const int AddnotifylobbyupdatereceivedApiLatest = 1;

		public const int AttributeApiLatest = 1;

		public const int AttributedataApiLatest = 1;

		public const int CopylobbydetailshandleApiLatest = 1;

		public const int CopylobbydetailshandlebyinviteidApiLatest = 1;

		public const int CopylobbydetailshandlebyuieventidApiLatest = 1;

		public const int CreatelobbyApiLatest = 5;

		public const int CreatelobbysearchApiLatest = 1;

		public const int DestroylobbyApiLatest = 1;

		public const int GetinvitecountApiLatest = 1;

		public const int GetinviteidbyindexApiLatest = 1;

		public const int InviteidMaxLength = 64;

		public const int JoinlobbyApiLatest = 2;

		public const int KickmemberApiLatest = 1;

		public const int LeavelobbyApiLatest = 1;

		public const int MaxLobbies = 16;

		public const int MaxLobbyMembers = 64;

		public const int MaxSearchResults = 200;

		public const int PromotememberApiLatest = 1;

		public const int QueryinvitesApiLatest = 1;

		public const int RejectinviteApiLatest = 1;

		public const string SearchBucketId = "bucket";

		public const string SearchMincurrentmembers = "mincurrentmembers";

		public const string SearchMinslotsavailable = "minslotsavailable";

		public const int SendinviteApiLatest = 1;

		public const int UpdatelobbyApiLatest = 1;

		public const int UpdatelobbymodificationApiLatest = 1;

		public LobbyInterface()
		{
		}

		public LobbyInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public ulong AddNotifyJoinLobbyAccepted(AddNotifyJoinLobbyAcceptedOptions options, object clientData, OnJoinLobbyAcceptedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyJoinLobbyAcceptedOptionsInternal, AddNotifyJoinLobbyAcceptedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnJoinLobbyAcceptedCallbackInternal onJoinLobbyAcceptedCallbackInternal = OnJoinLobbyAcceptedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onJoinLobbyAcceptedCallbackInternal);
			ulong num = Bindings.EOS_Lobby_AddNotifyJoinLobbyAccepted(base.InnerHandle, target, clientDataAddress, onJoinLobbyAcceptedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyLobbyInviteAccepted(AddNotifyLobbyInviteAcceptedOptions options, object clientData, OnLobbyInviteAcceptedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyLobbyInviteAcceptedOptionsInternal, AddNotifyLobbyInviteAcceptedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLobbyInviteAcceptedCallbackInternal onLobbyInviteAcceptedCallbackInternal = OnLobbyInviteAcceptedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onLobbyInviteAcceptedCallbackInternal);
			ulong num = Bindings.EOS_Lobby_AddNotifyLobbyInviteAccepted(base.InnerHandle, target, clientDataAddress, onLobbyInviteAcceptedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyLobbyInviteReceived(AddNotifyLobbyInviteReceivedOptions options, object clientData, OnLobbyInviteReceivedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyLobbyInviteReceivedOptionsInternal, AddNotifyLobbyInviteReceivedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLobbyInviteReceivedCallbackInternal onLobbyInviteReceivedCallbackInternal = OnLobbyInviteReceivedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onLobbyInviteReceivedCallbackInternal);
			ulong num = Bindings.EOS_Lobby_AddNotifyLobbyInviteReceived(base.InnerHandle, target, clientDataAddress, onLobbyInviteReceivedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyLobbyMemberStatusReceived(AddNotifyLobbyMemberStatusReceivedOptions options, object clientData, OnLobbyMemberStatusReceivedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyLobbyMemberStatusReceivedOptionsInternal, AddNotifyLobbyMemberStatusReceivedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLobbyMemberStatusReceivedCallbackInternal onLobbyMemberStatusReceivedCallbackInternal = OnLobbyMemberStatusReceivedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onLobbyMemberStatusReceivedCallbackInternal);
			ulong num = Bindings.EOS_Lobby_AddNotifyLobbyMemberStatusReceived(base.InnerHandle, target, clientDataAddress, onLobbyMemberStatusReceivedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyLobbyMemberUpdateReceived(AddNotifyLobbyMemberUpdateReceivedOptions options, object clientData, OnLobbyMemberUpdateReceivedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyLobbyMemberUpdateReceivedOptionsInternal, AddNotifyLobbyMemberUpdateReceivedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLobbyMemberUpdateReceivedCallbackInternal onLobbyMemberUpdateReceivedCallbackInternal = OnLobbyMemberUpdateReceivedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onLobbyMemberUpdateReceivedCallbackInternal);
			ulong num = Bindings.EOS_Lobby_AddNotifyLobbyMemberUpdateReceived(base.InnerHandle, target, clientDataAddress, onLobbyMemberUpdateReceivedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyLobbyUpdateReceived(AddNotifyLobbyUpdateReceivedOptions options, object clientData, OnLobbyUpdateReceivedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyLobbyUpdateReceivedOptionsInternal, AddNotifyLobbyUpdateReceivedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLobbyUpdateReceivedCallbackInternal onLobbyUpdateReceivedCallbackInternal = OnLobbyUpdateReceivedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onLobbyUpdateReceivedCallbackInternal);
			ulong num = Bindings.EOS_Lobby_AddNotifyLobbyUpdateReceived(base.InnerHandle, target, clientDataAddress, onLobbyUpdateReceivedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public Result CopyLobbyDetailsHandle(CopyLobbyDetailsHandleOptions options, out LobbyDetails outLobbyDetailsHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyLobbyDetailsHandleOptionsInternal, CopyLobbyDetailsHandleOptions>(ref target, options);
			IntPtr outLobbyDetailsHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Lobby_CopyLobbyDetailsHandle(base.InnerHandle, target, ref outLobbyDetailsHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outLobbyDetailsHandle2, out outLobbyDetailsHandle);
			return result;
		}

		public Result CopyLobbyDetailsHandleByInviteId(CopyLobbyDetailsHandleByInviteIdOptions options, out LobbyDetails outLobbyDetailsHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyLobbyDetailsHandleByInviteIdOptionsInternal, CopyLobbyDetailsHandleByInviteIdOptions>(ref target, options);
			IntPtr outLobbyDetailsHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Lobby_CopyLobbyDetailsHandleByInviteId(base.InnerHandle, target, ref outLobbyDetailsHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outLobbyDetailsHandle2, out outLobbyDetailsHandle);
			return result;
		}

		public Result CopyLobbyDetailsHandleByUiEventId(CopyLobbyDetailsHandleByUiEventIdOptions options, out LobbyDetails outLobbyDetailsHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyLobbyDetailsHandleByUiEventIdOptionsInternal, CopyLobbyDetailsHandleByUiEventIdOptions>(ref target, options);
			IntPtr outLobbyDetailsHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Lobby_CopyLobbyDetailsHandleByUiEventId(base.InnerHandle, target, ref outLobbyDetailsHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outLobbyDetailsHandle2, out outLobbyDetailsHandle);
			return result;
		}

		public void CreateLobby(CreateLobbyOptions options, object clientData, OnCreateLobbyCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CreateLobbyOptionsInternal, CreateLobbyOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnCreateLobbyCallbackInternal onCreateLobbyCallbackInternal = OnCreateLobbyCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onCreateLobbyCallbackInternal);
			Bindings.EOS_Lobby_CreateLobby(base.InnerHandle, target, clientDataAddress, onCreateLobbyCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public Result CreateLobbySearch(CreateLobbySearchOptions options, out LobbySearch outLobbySearchHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CreateLobbySearchOptionsInternal, CreateLobbySearchOptions>(ref target, options);
			IntPtr outLobbySearchHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Lobby_CreateLobbySearch(base.InnerHandle, target, ref outLobbySearchHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outLobbySearchHandle2, out outLobbySearchHandle);
			return result;
		}

		public void DestroyLobby(DestroyLobbyOptions options, object clientData, OnDestroyLobbyCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<DestroyLobbyOptionsInternal, DestroyLobbyOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnDestroyLobbyCallbackInternal onDestroyLobbyCallbackInternal = OnDestroyLobbyCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onDestroyLobbyCallbackInternal);
			Bindings.EOS_Lobby_DestroyLobby(base.InnerHandle, target, clientDataAddress, onDestroyLobbyCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public uint GetInviteCount(GetInviteCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetInviteCountOptionsInternal, GetInviteCountOptions>(ref target, options);
			uint result = Bindings.EOS_Lobby_GetInviteCount(base.InnerHandle, target);
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
			Result result = Bindings.EOS_Lobby_GetInviteIdByIndex(base.InnerHandle, target, target2, ref inOutBufferLength);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(target2, out outBuffer);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public void JoinLobby(JoinLobbyOptions options, object clientData, OnJoinLobbyCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<JoinLobbyOptionsInternal, JoinLobbyOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnJoinLobbyCallbackInternal onJoinLobbyCallbackInternal = OnJoinLobbyCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onJoinLobbyCallbackInternal);
			Bindings.EOS_Lobby_JoinLobby(base.InnerHandle, target, clientDataAddress, onJoinLobbyCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void KickMember(KickMemberOptions options, object clientData, OnKickMemberCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<KickMemberOptionsInternal, KickMemberOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnKickMemberCallbackInternal onKickMemberCallbackInternal = OnKickMemberCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onKickMemberCallbackInternal);
			Bindings.EOS_Lobby_KickMember(base.InnerHandle, target, clientDataAddress, onKickMemberCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void LeaveLobby(LeaveLobbyOptions options, object clientData, OnLeaveLobbyCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LeaveLobbyOptionsInternal, LeaveLobbyOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLeaveLobbyCallbackInternal onLeaveLobbyCallbackInternal = OnLeaveLobbyCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onLeaveLobbyCallbackInternal);
			Bindings.EOS_Lobby_LeaveLobby(base.InnerHandle, target, clientDataAddress, onLeaveLobbyCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void PromoteMember(PromoteMemberOptions options, object clientData, OnPromoteMemberCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<PromoteMemberOptionsInternal, PromoteMemberOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnPromoteMemberCallbackInternal onPromoteMemberCallbackInternal = OnPromoteMemberCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onPromoteMemberCallbackInternal);
			Bindings.EOS_Lobby_PromoteMember(base.InnerHandle, target, clientDataAddress, onPromoteMemberCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryInvites(QueryInvitesOptions options, object clientData, OnQueryInvitesCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryInvitesOptionsInternal, QueryInvitesOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryInvitesCallbackInternal onQueryInvitesCallbackInternal = OnQueryInvitesCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryInvitesCallbackInternal);
			Bindings.EOS_Lobby_QueryInvites(base.InnerHandle, target, clientDataAddress, onQueryInvitesCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RejectInvite(RejectInviteOptions options, object clientData, OnRejectInviteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<RejectInviteOptionsInternal, RejectInviteOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnRejectInviteCallbackInternal onRejectInviteCallbackInternal = OnRejectInviteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onRejectInviteCallbackInternal);
			Bindings.EOS_Lobby_RejectInvite(base.InnerHandle, target, clientDataAddress, onRejectInviteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RemoveNotifyJoinLobbyAccepted(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Lobby_RemoveNotifyJoinLobbyAccepted(base.InnerHandle, inId);
		}

		public void RemoveNotifyLobbyInviteAccepted(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Lobby_RemoveNotifyLobbyInviteAccepted(base.InnerHandle, inId);
		}

		public void RemoveNotifyLobbyInviteReceived(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Lobby_RemoveNotifyLobbyInviteReceived(base.InnerHandle, inId);
		}

		public void RemoveNotifyLobbyMemberStatusReceived(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Lobby_RemoveNotifyLobbyMemberStatusReceived(base.InnerHandle, inId);
		}

		public void RemoveNotifyLobbyMemberUpdateReceived(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Lobby_RemoveNotifyLobbyMemberUpdateReceived(base.InnerHandle, inId);
		}

		public void RemoveNotifyLobbyUpdateReceived(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Lobby_RemoveNotifyLobbyUpdateReceived(base.InnerHandle, inId);
		}

		public void SendInvite(SendInviteOptions options, object clientData, OnSendInviteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SendInviteOptionsInternal, SendInviteOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnSendInviteCallbackInternal onSendInviteCallbackInternal = OnSendInviteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onSendInviteCallbackInternal);
			Bindings.EOS_Lobby_SendInvite(base.InnerHandle, target, clientDataAddress, onSendInviteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void UpdateLobby(UpdateLobbyOptions options, object clientData, OnUpdateLobbyCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UpdateLobbyOptionsInternal, UpdateLobbyOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnUpdateLobbyCallbackInternal onUpdateLobbyCallbackInternal = OnUpdateLobbyCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onUpdateLobbyCallbackInternal);
			Bindings.EOS_Lobby_UpdateLobby(base.InnerHandle, target, clientDataAddress, onUpdateLobbyCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public Result UpdateLobbyModification(UpdateLobbyModificationOptions options, out LobbyModification outLobbyModificationHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UpdateLobbyModificationOptionsInternal, UpdateLobbyModificationOptions>(ref target, options);
			IntPtr outLobbyModificationHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Lobby_UpdateLobbyModification(base.InnerHandle, target, ref outLobbyModificationHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outLobbyModificationHandle2, out outLobbyModificationHandle);
			return result;
		}

		[MonoPInvokeCallback(typeof(OnCreateLobbyCallbackInternal))]
		internal static void OnCreateLobbyCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnCreateLobbyCallback, CreateLobbyCallbackInfoInternal, CreateLobbyCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnDestroyLobbyCallbackInternal))]
		internal static void OnDestroyLobbyCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnDestroyLobbyCallback, DestroyLobbyCallbackInfoInternal, DestroyLobbyCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnJoinLobbyAcceptedCallbackInternal))]
		internal static void OnJoinLobbyAcceptedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnJoinLobbyAcceptedCallback, JoinLobbyAcceptedCallbackInfoInternal, JoinLobbyAcceptedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnJoinLobbyCallbackInternal))]
		internal static void OnJoinLobbyCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnJoinLobbyCallback, JoinLobbyCallbackInfoInternal, JoinLobbyCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnKickMemberCallbackInternal))]
		internal static void OnKickMemberCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnKickMemberCallback, KickMemberCallbackInfoInternal, KickMemberCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnLeaveLobbyCallbackInternal))]
		internal static void OnLeaveLobbyCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnLeaveLobbyCallback, LeaveLobbyCallbackInfoInternal, LeaveLobbyCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnLobbyInviteAcceptedCallbackInternal))]
		internal static void OnLobbyInviteAcceptedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnLobbyInviteAcceptedCallback, LobbyInviteAcceptedCallbackInfoInternal, LobbyInviteAcceptedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnLobbyInviteReceivedCallbackInternal))]
		internal static void OnLobbyInviteReceivedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnLobbyInviteReceivedCallback, LobbyInviteReceivedCallbackInfoInternal, LobbyInviteReceivedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnLobbyMemberStatusReceivedCallbackInternal))]
		internal static void OnLobbyMemberStatusReceivedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnLobbyMemberStatusReceivedCallback, LobbyMemberStatusReceivedCallbackInfoInternal, LobbyMemberStatusReceivedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnLobbyMemberUpdateReceivedCallbackInternal))]
		internal static void OnLobbyMemberUpdateReceivedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnLobbyMemberUpdateReceivedCallback, LobbyMemberUpdateReceivedCallbackInfoInternal, LobbyMemberUpdateReceivedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnLobbyUpdateReceivedCallbackInternal))]
		internal static void OnLobbyUpdateReceivedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnLobbyUpdateReceivedCallback, LobbyUpdateReceivedCallbackInfoInternal, LobbyUpdateReceivedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnPromoteMemberCallbackInternal))]
		internal static void OnPromoteMemberCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnPromoteMemberCallback, PromoteMemberCallbackInfoInternal, PromoteMemberCallbackInfo>(data, out var callback, out var callbackInfo))
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

		[MonoPInvokeCallback(typeof(OnUpdateLobbyCallbackInternal))]
		internal static void OnUpdateLobbyCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnUpdateLobbyCallback, UpdateLobbyCallbackInfoInternal, UpdateLobbyCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
