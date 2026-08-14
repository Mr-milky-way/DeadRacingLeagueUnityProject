using System;
using Epic.OnlineServices.AntiCheatCommon;

namespace Epic.OnlineServices.AntiCheatClient
{
	public sealed class AntiCheatClientInterface : Handle
	{
		public const int AddexternalintegritycatalogApiLatest = 1;

		public const int AddnotifymessagetopeerApiLatest = 1;

		public const int AddnotifymessagetoserverApiLatest = 1;

		public const int AddnotifypeeractionrequiredApiLatest = 1;

		public const int AddnotifypeerauthstatuschangedApiLatest = 1;

		public const int BeginsessionApiLatest = 3;

		public const int EndsessionApiLatest = 1;

		public const int GetprotectmessageoutputlengthApiLatest = 1;

		public const int PeerSelf = -1;

		public const int PollstatusApiLatest = 1;

		public const int ProtectmessageApiLatest = 1;

		public const int ReceivemessagefrompeerApiLatest = 1;

		public const int ReceivemessagefromserverApiLatest = 1;

		public const int RegisterpeerApiLatest = 1;

		public const int UnprotectmessageApiLatest = 1;

		public const int UnregisterpeerApiLatest = 1;

		public AntiCheatClientInterface()
		{
		}

		public AntiCheatClientInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result AddExternalIntegrityCatalog(AddExternalIntegrityCatalogOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddExternalIntegrityCatalogOptionsInternal, AddExternalIntegrityCatalogOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatClient_AddExternalIntegrityCatalog(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public ulong AddNotifyMessageToPeer(AddNotifyMessageToPeerOptions options, object clientData, OnMessageToPeerCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyMessageToPeerOptionsInternal, AddNotifyMessageToPeerOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnMessageToPeerCallbackInternal onMessageToPeerCallbackInternal = OnMessageToPeerCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onMessageToPeerCallbackInternal);
			ulong num = Bindings.EOS_AntiCheatClient_AddNotifyMessageToPeer(base.InnerHandle, target, clientDataAddress, onMessageToPeerCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyMessageToServer(AddNotifyMessageToServerOptions options, object clientData, OnMessageToServerCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyMessageToServerOptionsInternal, AddNotifyMessageToServerOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnMessageToServerCallbackInternal onMessageToServerCallbackInternal = OnMessageToServerCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onMessageToServerCallbackInternal);
			ulong num = Bindings.EOS_AntiCheatClient_AddNotifyMessageToServer(base.InnerHandle, target, clientDataAddress, onMessageToServerCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyPeerActionRequired(AddNotifyPeerActionRequiredOptions options, object clientData, OnPeerActionRequiredCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyPeerActionRequiredOptionsInternal, AddNotifyPeerActionRequiredOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnPeerActionRequiredCallbackInternal onPeerActionRequiredCallbackInternal = OnPeerActionRequiredCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onPeerActionRequiredCallbackInternal);
			ulong num = Bindings.EOS_AntiCheatClient_AddNotifyPeerActionRequired(base.InnerHandle, target, clientDataAddress, onPeerActionRequiredCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyPeerAuthStatusChanged(AddNotifyPeerAuthStatusChangedOptions options, object clientData, OnPeerAuthStatusChangedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyPeerAuthStatusChangedOptionsInternal, AddNotifyPeerAuthStatusChangedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnPeerAuthStatusChangedCallbackInternal onPeerAuthStatusChangedCallbackInternal = OnPeerAuthStatusChangedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onPeerAuthStatusChangedCallbackInternal);
			ulong num = Bindings.EOS_AntiCheatClient_AddNotifyPeerAuthStatusChanged(base.InnerHandle, target, clientDataAddress, onPeerAuthStatusChangedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public Result BeginSession(BeginSessionOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<BeginSessionOptionsInternal, BeginSessionOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatClient_BeginSession(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result EndSession(EndSessionOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<EndSessionOptionsInternal, EndSessionOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatClient_EndSession(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result GetProtectMessageOutputLength(GetProtectMessageOutputLengthOptions options, out uint outBufferLengthBytes)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetProtectMessageOutputLengthOptionsInternal, GetProtectMessageOutputLengthOptions>(ref target, options);
			outBufferLengthBytes = Helper.GetDefault<uint>();
			Result result = Bindings.EOS_AntiCheatClient_GetProtectMessageOutputLength(base.InnerHandle, target, ref outBufferLengthBytes);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result PollStatus(PollStatusOptions options, AntiCheatClientViolationType violationType, out string outMessage)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<PollStatusOptionsInternal, PollStatusOptions>(ref target, options);
			IntPtr target2 = IntPtr.Zero;
			uint outMessageLength = options.OutMessageLength;
			Helper.TryMarshalAllocate(ref target2, outMessageLength, out var _);
			Result result = Bindings.EOS_AntiCheatClient_PollStatus(base.InnerHandle, target, violationType, target2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(target2, out outMessage);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public Result ProtectMessage(ProtectMessageOptions options, out byte[] outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<ProtectMessageOptionsInternal, ProtectMessageOptions>(ref target, options);
			IntPtr target2 = IntPtr.Zero;
			uint outBufferLengthBytes = options.OutBufferSizeBytes;
			Helper.TryMarshalAllocate(ref target2, outBufferLengthBytes, out var _);
			Result result = Bindings.EOS_AntiCheatClient_ProtectMessage(base.InnerHandle, target, target2, ref outBufferLengthBytes);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(target2, out outBuffer, outBufferLengthBytes);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public Result ReceiveMessageFromPeer(ReceiveMessageFromPeerOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<ReceiveMessageFromPeerOptionsInternal, ReceiveMessageFromPeerOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatClient_ReceiveMessageFromPeer(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result ReceiveMessageFromServer(ReceiveMessageFromServerOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<ReceiveMessageFromServerOptionsInternal, ReceiveMessageFromServerOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatClient_ReceiveMessageFromServer(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result RegisterPeer(RegisterPeerOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<RegisterPeerOptionsInternal, RegisterPeerOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatClient_RegisterPeer(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void RemoveNotifyMessageToPeer(ulong notificationId)
		{
			Helper.TryRemoveCallbackByNotificationId(notificationId);
			Bindings.EOS_AntiCheatClient_RemoveNotifyMessageToPeer(base.InnerHandle, notificationId);
		}

		public void RemoveNotifyMessageToServer(ulong notificationId)
		{
			Helper.TryRemoveCallbackByNotificationId(notificationId);
			Bindings.EOS_AntiCheatClient_RemoveNotifyMessageToServer(base.InnerHandle, notificationId);
		}

		public void RemoveNotifyPeerActionRequired(ulong notificationId)
		{
			Helper.TryRemoveCallbackByNotificationId(notificationId);
			Bindings.EOS_AntiCheatClient_RemoveNotifyPeerActionRequired(base.InnerHandle, notificationId);
		}

		public void RemoveNotifyPeerAuthStatusChanged(ulong notificationId)
		{
			Helper.TryRemoveCallbackByNotificationId(notificationId);
			Bindings.EOS_AntiCheatClient_RemoveNotifyPeerAuthStatusChanged(base.InnerHandle, notificationId);
		}

		public Result UnprotectMessage(UnprotectMessageOptions options, out byte[] outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UnprotectMessageOptionsInternal, UnprotectMessageOptions>(ref target, options);
			IntPtr target2 = IntPtr.Zero;
			uint outBufferLengthBytes = options.OutBufferSizeBytes;
			Helper.TryMarshalAllocate(ref target2, outBufferLengthBytes, out var _);
			Result result = Bindings.EOS_AntiCheatClient_UnprotectMessage(base.InnerHandle, target, target2, ref outBufferLengthBytes);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(target2, out outBuffer, outBufferLengthBytes);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public Result UnregisterPeer(UnregisterPeerOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UnregisterPeerOptionsInternal, UnregisterPeerOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatClient_UnregisterPeer(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		[MonoPInvokeCallback(typeof(OnMessageToPeerCallbackInternal))]
		internal static void OnMessageToPeerCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnMessageToPeerCallback, OnMessageToClientCallbackInfoInternal, OnMessageToClientCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnMessageToServerCallbackInternal))]
		internal static void OnMessageToServerCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnMessageToServerCallback, OnMessageToServerCallbackInfoInternal, OnMessageToServerCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnPeerActionRequiredCallbackInternal))]
		internal static void OnPeerActionRequiredCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnPeerActionRequiredCallback, OnClientActionRequiredCallbackInfoInternal, OnClientActionRequiredCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnPeerAuthStatusChangedCallbackInternal))]
		internal static void OnPeerAuthStatusChangedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnPeerAuthStatusChangedCallback, OnClientAuthStatusChangedCallbackInfoInternal, OnClientAuthStatusChangedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
