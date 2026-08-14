using System;

namespace Epic.OnlineServices.Presence
{
	public sealed class PresenceInterface : Handle
	{
		public const int AddnotifyjoingameacceptedApiLatest = 2;

		public const int AddnotifyonpresencechangedApiLatest = 1;

		public const int CopypresenceApiLatest = 2;

		public const int CreatepresencemodificationApiLatest = 1;

		public const int DataMaxKeyLength = 64;

		public const int DataMaxKeys = 32;

		public const int DataMaxValueLength = 255;

		public const int DatarecordApiLatest = 1;

		public const int DeletedataApiLatest = 1;

		public const int GetjoininfoApiLatest = 1;

		public const int HaspresenceApiLatest = 1;

		public const int InfoApiLatest = 2;

		public const int QuerypresenceApiLatest = 1;

		public const int RichTextMaxValueLength = 255;

		public const int SetdataApiLatest = 1;

		public const int SetpresenceApiLatest = 1;

		public const int SetrawrichtextApiLatest = 1;

		public const int SetstatusApiLatest = 1;

		public PresenceInterface()
		{
		}

		public PresenceInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public ulong AddNotifyJoinGameAccepted(AddNotifyJoinGameAcceptedOptions options, object clientData, OnJoinGameAcceptedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyJoinGameAcceptedOptionsInternal, AddNotifyJoinGameAcceptedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnJoinGameAcceptedCallbackInternal onJoinGameAcceptedCallbackInternal = OnJoinGameAcceptedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onJoinGameAcceptedCallbackInternal);
			ulong num = Bindings.EOS_Presence_AddNotifyJoinGameAccepted(base.InnerHandle, target, clientDataAddress, onJoinGameAcceptedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyOnPresenceChanged(AddNotifyOnPresenceChangedOptions options, object clientData, OnPresenceChangedCallback notificationHandler)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyOnPresenceChangedOptionsInternal, AddNotifyOnPresenceChangedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnPresenceChangedCallbackInternal onPresenceChangedCallbackInternal = OnPresenceChangedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationHandler, onPresenceChangedCallbackInternal);
			ulong num = Bindings.EOS_Presence_AddNotifyOnPresenceChanged(base.InnerHandle, target, clientDataAddress, onPresenceChangedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public Result CopyPresence(CopyPresenceOptions options, out Info outPresence)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyPresenceOptionsInternal, CopyPresenceOptions>(ref target, options);
			IntPtr outPresence2 = IntPtr.Zero;
			Result result = Bindings.EOS_Presence_CopyPresence(base.InnerHandle, target, ref outPresence2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<InfoInternal, Info>(outPresence2, out outPresence))
			{
				Bindings.EOS_Presence_Info_Release(outPresence2);
			}
			return result;
		}

		public Result CreatePresenceModification(CreatePresenceModificationOptions options, out PresenceModification outPresenceModificationHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CreatePresenceModificationOptionsInternal, CreatePresenceModificationOptions>(ref target, options);
			IntPtr outPresenceModificationHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_Presence_CreatePresenceModification(base.InnerHandle, target, ref outPresenceModificationHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outPresenceModificationHandle2, out outPresenceModificationHandle);
			return result;
		}

		public Result GetJoinInfo(GetJoinInfoOptions options, out string outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetJoinInfoOptionsInternal, GetJoinInfoOptions>(ref target, options);
			IntPtr target2 = IntPtr.Zero;
			int inOutBufferLength = 256;
			Helper.TryMarshalAllocate(ref target2, inOutBufferLength, out var _);
			Result result = Bindings.EOS_Presence_GetJoinInfo(base.InnerHandle, target, target2, ref inOutBufferLength);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(target2, out outBuffer);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public bool HasPresence(HasPresenceOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<HasPresenceOptionsInternal, HasPresenceOptions>(ref target, options);
			int source = Bindings.EOS_Presence_HasPresence(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out var target2);
			return target2;
		}

		public void QueryPresence(QueryPresenceOptions options, object clientData, OnQueryPresenceCompleteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryPresenceOptionsInternal, QueryPresenceOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryPresenceCompleteCallbackInternal onQueryPresenceCompleteCallbackInternal = OnQueryPresenceCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryPresenceCompleteCallbackInternal);
			Bindings.EOS_Presence_QueryPresence(base.InnerHandle, target, clientDataAddress, onQueryPresenceCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RemoveNotifyJoinGameAccepted(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Presence_RemoveNotifyJoinGameAccepted(base.InnerHandle, inId);
		}

		public void RemoveNotifyOnPresenceChanged(ulong notificationId)
		{
			Helper.TryRemoveCallbackByNotificationId(notificationId);
			Bindings.EOS_Presence_RemoveNotifyOnPresenceChanged(base.InnerHandle, notificationId);
		}

		public void SetPresence(SetPresenceOptions options, object clientData, SetPresenceCompleteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SetPresenceOptionsInternal, SetPresenceOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			SetPresenceCompleteCallbackInternal setPresenceCompleteCallbackInternal = SetPresenceCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, setPresenceCompleteCallbackInternal);
			Bindings.EOS_Presence_SetPresence(base.InnerHandle, target, clientDataAddress, setPresenceCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnJoinGameAcceptedCallbackInternal))]
		internal static void OnJoinGameAcceptedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnJoinGameAcceptedCallback, JoinGameAcceptedCallbackInfoInternal, JoinGameAcceptedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnPresenceChangedCallbackInternal))]
		internal static void OnPresenceChangedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnPresenceChangedCallback, PresenceChangedCallbackInfoInternal, PresenceChangedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryPresenceCompleteCallbackInternal))]
		internal static void OnQueryPresenceCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryPresenceCompleteCallback, QueryPresenceCallbackInfoInternal, QueryPresenceCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(SetPresenceCompleteCallbackInternal))]
		internal static void SetPresenceCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<SetPresenceCompleteCallback, SetPresenceCallbackInfoInternal, SetPresenceCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
