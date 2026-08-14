using System;

namespace Epic.OnlineServices.UI
{
	public sealed class UIInterface : Handle
	{
		public const int AcknowledgecorrelationidApiLatest = 1;

		public const int AcknowledgeeventidApiLatest = 1;

		public const int AddnotifydisplaysettingsupdatedApiLatest = 1;

		public const int EventidInvalid = 0;

		public const int GetfriendsvisibleApiLatest = 1;

		public const int GettogglefriendskeyApiLatest = 1;

		public const int HidefriendsApiLatest = 1;

		public const int PrepresentApiLatest = 1;

		public const int ReportkeyeventApiLatest = 1;

		public const int SetdisplaypreferenceApiLatest = 1;

		public const int SettogglefriendskeyApiLatest = 1;

		public const int ShowfriendsApiLatest = 1;

		public UIInterface()
		{
		}

		public UIInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result AcknowledgeEventId(AcknowledgeEventIdOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AcknowledgeEventIdOptionsInternal, AcknowledgeEventIdOptions>(ref target, options);
			Result result = Bindings.EOS_UI_AcknowledgeEventId(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public ulong AddNotifyDisplaySettingsUpdated(AddNotifyDisplaySettingsUpdatedOptions options, object clientData, OnDisplaySettingsUpdatedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyDisplaySettingsUpdatedOptionsInternal, AddNotifyDisplaySettingsUpdatedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnDisplaySettingsUpdatedCallbackInternal onDisplaySettingsUpdatedCallbackInternal = OnDisplaySettingsUpdatedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onDisplaySettingsUpdatedCallbackInternal);
			ulong num = Bindings.EOS_UI_AddNotifyDisplaySettingsUpdated(base.InnerHandle, target, clientDataAddress, onDisplaySettingsUpdatedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public bool GetFriendsVisible(GetFriendsVisibleOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetFriendsVisibleOptionsInternal, GetFriendsVisibleOptions>(ref target, options);
			int source = Bindings.EOS_UI_GetFriendsVisible(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out var target2);
			return target2;
		}

		public NotificationLocation GetNotificationLocationPreference()
		{
			return Bindings.EOS_UI_GetNotificationLocationPreference(base.InnerHandle);
		}

		public KeyCombination GetToggleFriendsKey(GetToggleFriendsKeyOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetToggleFriendsKeyOptionsInternal, GetToggleFriendsKeyOptions>(ref target, options);
			KeyCombination result = Bindings.EOS_UI_GetToggleFriendsKey(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void HideFriends(HideFriendsOptions options, object clientData, OnHideFriendsCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<HideFriendsOptionsInternal, HideFriendsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnHideFriendsCallbackInternal onHideFriendsCallbackInternal = OnHideFriendsCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onHideFriendsCallbackInternal);
			Bindings.EOS_UI_HideFriends(base.InnerHandle, target, clientDataAddress, onHideFriendsCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public bool IsValidKeyCombination(KeyCombination keyCombination)
		{
			Helper.TryMarshalGet(Bindings.EOS_UI_IsValidKeyCombination(base.InnerHandle, keyCombination), out var target);
			return target;
		}

		public void RemoveNotifyDisplaySettingsUpdated(ulong id)
		{
			Helper.TryRemoveCallbackByNotificationId(id);
			Bindings.EOS_UI_RemoveNotifyDisplaySettingsUpdated(base.InnerHandle, id);
		}

		public Result SetDisplayPreference(SetDisplayPreferenceOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SetDisplayPreferenceOptionsInternal, SetDisplayPreferenceOptions>(ref target, options);
			Result result = Bindings.EOS_UI_SetDisplayPreference(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetToggleFriendsKey(SetToggleFriendsKeyOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SetToggleFriendsKeyOptionsInternal, SetToggleFriendsKeyOptions>(ref target, options);
			Result result = Bindings.EOS_UI_SetToggleFriendsKey(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void ShowFriends(ShowFriendsOptions options, object clientData, OnShowFriendsCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<ShowFriendsOptionsInternal, ShowFriendsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnShowFriendsCallbackInternal onShowFriendsCallbackInternal = OnShowFriendsCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onShowFriendsCallbackInternal);
			Bindings.EOS_UI_ShowFriends(base.InnerHandle, target, clientDataAddress, onShowFriendsCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnDisplaySettingsUpdatedCallbackInternal))]
		internal static void OnDisplaySettingsUpdatedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnDisplaySettingsUpdatedCallback, OnDisplaySettingsUpdatedCallbackInfoInternal, OnDisplaySettingsUpdatedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnHideFriendsCallbackInternal))]
		internal static void OnHideFriendsCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnHideFriendsCallback, HideFriendsCallbackInfoInternal, HideFriendsCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnShowFriendsCallbackInternal))]
		internal static void OnShowFriendsCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnShowFriendsCallback, ShowFriendsCallbackInfoInternal, ShowFriendsCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
