using System;
using Epic.OnlineServices.AntiCheatCommon;

namespace Epic.OnlineServices.AntiCheatServer
{
	public sealed class AntiCheatServerInterface : Handle
	{
		public const int AddnotifyclientactionrequiredApiLatest = 1;

		public const int AddnotifyclientauthstatuschangedApiLatest = 1;

		public const int AddnotifymessagetoclientApiLatest = 1;

		public const int BeginsessionApiLatest = 3;

		public const int BeginsessionMaxRegistertimeout = 120;

		public const int BeginsessionMinRegistertimeout = 10;

		public const int EndsessionApiLatest = 1;

		public const int GetprotectmessageoutputlengthApiLatest = 1;

		public const int ProtectmessageApiLatest = 1;

		public const int ReceivemessagefromclientApiLatest = 1;

		public const int RegisterclientApiLatest = 1;

		public const int SetclientnetworkstateApiLatest = 1;

		public const int UnprotectmessageApiLatest = 1;

		public const int UnregisterclientApiLatest = 1;

		public AntiCheatServerInterface()
		{
		}

		public AntiCheatServerInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public ulong AddNotifyClientActionRequired(AddNotifyClientActionRequiredOptions options, object clientData, OnClientActionRequiredCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyClientActionRequiredOptionsInternal, AddNotifyClientActionRequiredOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnClientActionRequiredCallbackInternal onClientActionRequiredCallbackInternal = OnClientActionRequiredCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onClientActionRequiredCallbackInternal);
			ulong num = Bindings.EOS_AntiCheatServer_AddNotifyClientActionRequired(base.InnerHandle, target, clientDataAddress, onClientActionRequiredCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyClientAuthStatusChanged(AddNotifyClientAuthStatusChangedOptions options, object clientData, OnClientAuthStatusChangedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyClientAuthStatusChangedOptionsInternal, AddNotifyClientAuthStatusChangedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnClientAuthStatusChangedCallbackInternal onClientAuthStatusChangedCallbackInternal = OnClientAuthStatusChangedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onClientAuthStatusChangedCallbackInternal);
			ulong num = Bindings.EOS_AntiCheatServer_AddNotifyClientAuthStatusChanged(base.InnerHandle, target, clientDataAddress, onClientAuthStatusChangedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyMessageToClient(AddNotifyMessageToClientOptions options, object clientData, OnMessageToClientCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyMessageToClientOptionsInternal, AddNotifyMessageToClientOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnMessageToClientCallbackInternal onMessageToClientCallbackInternal = OnMessageToClientCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onMessageToClientCallbackInternal);
			ulong num = Bindings.EOS_AntiCheatServer_AddNotifyMessageToClient(base.InnerHandle, target, clientDataAddress, onMessageToClientCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public Result BeginSession(BeginSessionOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<BeginSessionOptionsInternal, BeginSessionOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_BeginSession(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result EndSession(EndSessionOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<EndSessionOptionsInternal, EndSessionOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_EndSession(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result GetProtectMessageOutputLength(GetProtectMessageOutputLengthOptions options, out uint outBufferLengthBytes)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetProtectMessageOutputLengthOptionsInternal, GetProtectMessageOutputLengthOptions>(ref target, options);
			outBufferLengthBytes = Helper.GetDefault<uint>();
			Result result = Bindings.EOS_AntiCheatServer_GetProtectMessageOutputLength(base.InnerHandle, target, ref outBufferLengthBytes);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result LogEvent(LogEventOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LogEventOptionsInternal, LogEventOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_LogEvent(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result LogGameRoundEnd(LogGameRoundEndOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LogGameRoundEndOptionsInternal, LogGameRoundEndOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_LogGameRoundEnd(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result LogGameRoundStart(LogGameRoundStartOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LogGameRoundStartOptionsInternal, LogGameRoundStartOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_LogGameRoundStart(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result LogPlayerDespawn(LogPlayerDespawnOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LogPlayerDespawnOptionsInternal, LogPlayerDespawnOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_LogPlayerDespawn(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result LogPlayerRevive(LogPlayerReviveOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LogPlayerReviveOptionsInternal, LogPlayerReviveOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_LogPlayerRevive(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result LogPlayerSpawn(LogPlayerSpawnOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LogPlayerSpawnOptionsInternal, LogPlayerSpawnOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_LogPlayerSpawn(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result LogPlayerTakeDamage(LogPlayerTakeDamageOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LogPlayerTakeDamageOptionsInternal, LogPlayerTakeDamageOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_LogPlayerTakeDamage(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result LogPlayerTick(LogPlayerTickOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LogPlayerTickOptionsInternal, LogPlayerTickOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_LogPlayerTick(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result LogPlayerUseAbility(LogPlayerUseAbilityOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LogPlayerUseAbilityOptionsInternal, LogPlayerUseAbilityOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_LogPlayerUseAbility(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result LogPlayerUseWeapon(LogPlayerUseWeaponOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LogPlayerUseWeaponOptionsInternal, LogPlayerUseWeaponOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_LogPlayerUseWeapon(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result ProtectMessage(ProtectMessageOptions options, out byte[] outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<ProtectMessageOptionsInternal, ProtectMessageOptions>(ref target, options);
			IntPtr target2 = IntPtr.Zero;
			uint outBufferLengthBytes = options.OutBufferSizeBytes;
			Helper.TryMarshalAllocate(ref target2, outBufferLengthBytes, out var _);
			Result result = Bindings.EOS_AntiCheatServer_ProtectMessage(base.InnerHandle, target, target2, ref outBufferLengthBytes);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(target2, out outBuffer, outBufferLengthBytes);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public Result ReceiveMessageFromClient(ReceiveMessageFromClientOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<ReceiveMessageFromClientOptionsInternal, ReceiveMessageFromClientOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_ReceiveMessageFromClient(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result RegisterClient(RegisterClientOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<RegisterClientOptionsInternal, RegisterClientOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_RegisterClient(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result RegisterEvent(RegisterEventOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<RegisterEventOptionsInternal, RegisterEventOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_RegisterEvent(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void RemoveNotifyClientActionRequired(ulong notificationId)
		{
			Helper.TryRemoveCallbackByNotificationId(notificationId);
			Bindings.EOS_AntiCheatServer_RemoveNotifyClientActionRequired(base.InnerHandle, notificationId);
		}

		public void RemoveNotifyClientAuthStatusChanged(ulong notificationId)
		{
			Helper.TryRemoveCallbackByNotificationId(notificationId);
			Bindings.EOS_AntiCheatServer_RemoveNotifyClientAuthStatusChanged(base.InnerHandle, notificationId);
		}

		public void RemoveNotifyMessageToClient(ulong notificationId)
		{
			Helper.TryRemoveCallbackByNotificationId(notificationId);
			Bindings.EOS_AntiCheatServer_RemoveNotifyMessageToClient(base.InnerHandle, notificationId);
		}

		public Result SetClientDetails(SetClientDetailsOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SetClientDetailsOptionsInternal, SetClientDetailsOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_SetClientDetails(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetClientNetworkState(SetClientNetworkStateOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SetClientNetworkStateOptionsInternal, SetClientNetworkStateOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_SetClientNetworkState(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetGameSessionId(SetGameSessionIdOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SetGameSessionIdOptionsInternal, SetGameSessionIdOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_SetGameSessionId(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result UnprotectMessage(UnprotectMessageOptions options, out byte[] outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UnprotectMessageOptionsInternal, UnprotectMessageOptions>(ref target, options);
			IntPtr target2 = IntPtr.Zero;
			uint outBufferLengthBytes = options.OutBufferSizeBytes;
			Helper.TryMarshalAllocate(ref target2, outBufferLengthBytes, out var _);
			Result result = Bindings.EOS_AntiCheatServer_UnprotectMessage(base.InnerHandle, target, target2, ref outBufferLengthBytes);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(target2, out outBuffer, outBufferLengthBytes);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public Result UnregisterClient(UnregisterClientOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UnregisterClientOptionsInternal, UnregisterClientOptions>(ref target, options);
			Result result = Bindings.EOS_AntiCheatServer_UnregisterClient(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		[MonoPInvokeCallback(typeof(OnClientActionRequiredCallbackInternal))]
		internal static void OnClientActionRequiredCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnClientActionRequiredCallback, OnClientActionRequiredCallbackInfoInternal, OnClientActionRequiredCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnClientAuthStatusChangedCallbackInternal))]
		internal static void OnClientAuthStatusChangedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnClientAuthStatusChangedCallback, OnClientAuthStatusChangedCallbackInfoInternal, OnClientAuthStatusChangedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnMessageToClientCallbackInternal))]
		internal static void OnMessageToClientCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnMessageToClientCallback, OnMessageToClientCallbackInfoInternal, OnMessageToClientCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
