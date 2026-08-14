using System;

namespace Epic.OnlineServices.Friends
{
	public sealed class FriendsInterface : Handle
	{
		public const int AcceptinviteApiLatest = 1;

		public const int AddnotifyfriendsupdateApiLatest = 1;

		public const int GetfriendatindexApiLatest = 1;

		public const int GetfriendscountApiLatest = 1;

		public const int GetstatusApiLatest = 1;

		public const int QueryfriendsApiLatest = 1;

		public const int RejectinviteApiLatest = 1;

		public const int SendinviteApiLatest = 1;

		public FriendsInterface()
		{
		}

		public FriendsInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public void AcceptInvite(AcceptInviteOptions options, object clientData, OnAcceptInviteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AcceptInviteOptionsInternal, AcceptInviteOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnAcceptInviteCallbackInternal onAcceptInviteCallbackInternal = OnAcceptInviteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onAcceptInviteCallbackInternal);
			Bindings.EOS_Friends_AcceptInvite(base.InnerHandle, target, clientDataAddress, onAcceptInviteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public ulong AddNotifyFriendsUpdate(AddNotifyFriendsUpdateOptions options, object clientData, OnFriendsUpdateCallback friendsUpdateHandler)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyFriendsUpdateOptionsInternal, AddNotifyFriendsUpdateOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnFriendsUpdateCallbackInternal onFriendsUpdateCallbackInternal = OnFriendsUpdateCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, friendsUpdateHandler, onFriendsUpdateCallbackInternal);
			ulong num = Bindings.EOS_Friends_AddNotifyFriendsUpdate(base.InnerHandle, target, clientDataAddress, onFriendsUpdateCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public EpicAccountId GetFriendAtIndex(GetFriendAtIndexOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetFriendAtIndexOptionsInternal, GetFriendAtIndexOptions>(ref target, options);
			IntPtr source = Bindings.EOS_Friends_GetFriendAtIndex(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out EpicAccountId target2);
			return target2;
		}

		public int GetFriendsCount(GetFriendsCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetFriendsCountOptionsInternal, GetFriendsCountOptions>(ref target, options);
			int result = Bindings.EOS_Friends_GetFriendsCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public FriendsStatus GetStatus(GetStatusOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetStatusOptionsInternal, GetStatusOptions>(ref target, options);
			FriendsStatus result = Bindings.EOS_Friends_GetStatus(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void QueryFriends(QueryFriendsOptions options, object clientData, OnQueryFriendsCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryFriendsOptionsInternal, QueryFriendsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryFriendsCallbackInternal onQueryFriendsCallbackInternal = OnQueryFriendsCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryFriendsCallbackInternal);
			Bindings.EOS_Friends_QueryFriends(base.InnerHandle, target, clientDataAddress, onQueryFriendsCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RejectInvite(RejectInviteOptions options, object clientData, OnRejectInviteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<RejectInviteOptionsInternal, RejectInviteOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnRejectInviteCallbackInternal onRejectInviteCallbackInternal = OnRejectInviteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onRejectInviteCallbackInternal);
			Bindings.EOS_Friends_RejectInvite(base.InnerHandle, target, clientDataAddress, onRejectInviteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RemoveNotifyFriendsUpdate(ulong notificationId)
		{
			Helper.TryRemoveCallbackByNotificationId(notificationId);
			Bindings.EOS_Friends_RemoveNotifyFriendsUpdate(base.InnerHandle, notificationId);
		}

		public void SendInvite(SendInviteOptions options, object clientData, OnSendInviteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SendInviteOptionsInternal, SendInviteOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnSendInviteCallbackInternal onSendInviteCallbackInternal = OnSendInviteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onSendInviteCallbackInternal);
			Bindings.EOS_Friends_SendInvite(base.InnerHandle, target, clientDataAddress, onSendInviteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnAcceptInviteCallbackInternal))]
		internal static void OnAcceptInviteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnAcceptInviteCallback, AcceptInviteCallbackInfoInternal, AcceptInviteCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnFriendsUpdateCallbackInternal))]
		internal static void OnFriendsUpdateCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnFriendsUpdateCallback, OnFriendsUpdateInfoInternal, OnFriendsUpdateInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryFriendsCallbackInternal))]
		internal static void OnQueryFriendsCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryFriendsCallback, QueryFriendsCallbackInfoInternal, QueryFriendsCallbackInfo>(data, out var callback, out var callbackInfo))
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
	}
}
