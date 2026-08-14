using System;

namespace Epic.OnlineServices.Auth
{
	public sealed class AuthInterface : Handle
	{
		public const int AccountfeaturerestrictedinfoApiLatest = 1;

		public const int AddnotifyloginstatuschangedApiLatest = 1;

		public const int CopyuserauthtokenApiLatest = 1;

		public const int CredentialsApiLatest = 3;

		public const int DeletepersistentauthApiLatest = 2;

		public const int LinkaccountApiLatest = 1;

		public const int LoginApiLatest = 2;

		public const int LogoutApiLatest = 1;

		public const int PingrantinfoApiLatest = 2;

		public const int TokenApiLatest = 2;

		public const int VerifyuserauthApiLatest = 1;

		public const int AuthIoscredentialssystemauthcredentialsoptionsApiLatest = 1;

		public AuthInterface()
		{
		}

		public AuthInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public ulong AddNotifyLoginStatusChanged(AddNotifyLoginStatusChangedOptions options, object clientData, OnLoginStatusChangedCallback notification)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyLoginStatusChangedOptionsInternal, AddNotifyLoginStatusChangedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLoginStatusChangedCallbackInternal onLoginStatusChangedCallbackInternal = OnLoginStatusChangedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notification, onLoginStatusChangedCallbackInternal);
			ulong num = Bindings.EOS_Auth_AddNotifyLoginStatusChanged(base.InnerHandle, target, clientDataAddress, onLoginStatusChangedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public Result CopyUserAuthToken(CopyUserAuthTokenOptions options, EpicAccountId localUserId, out Token outUserAuthToken)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyUserAuthTokenOptionsInternal, CopyUserAuthTokenOptions>(ref target, options);
			IntPtr target2 = IntPtr.Zero;
			Helper.TryMarshalSet(ref target2, localUserId);
			IntPtr outUserAuthToken2 = IntPtr.Zero;
			Result result = Bindings.EOS_Auth_CopyUserAuthToken(base.InnerHandle, target, target2, ref outUserAuthToken2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<TokenInternal, Token>(outUserAuthToken2, out outUserAuthToken))
			{
				Bindings.EOS_Auth_Token_Release(outUserAuthToken2);
			}
			return result;
		}

		public void DeletePersistentAuth(DeletePersistentAuthOptions options, object clientData, OnDeletePersistentAuthCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<DeletePersistentAuthOptionsInternal, DeletePersistentAuthOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnDeletePersistentAuthCallbackInternal onDeletePersistentAuthCallbackInternal = OnDeletePersistentAuthCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onDeletePersistentAuthCallbackInternal);
			Bindings.EOS_Auth_DeletePersistentAuth(base.InnerHandle, target, clientDataAddress, onDeletePersistentAuthCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public EpicAccountId GetLoggedInAccountByIndex(int index)
		{
			Helper.TryMarshalGet(Bindings.EOS_Auth_GetLoggedInAccountByIndex(base.InnerHandle, index), out EpicAccountId target);
			return target;
		}

		public int GetLoggedInAccountsCount()
		{
			return Bindings.EOS_Auth_GetLoggedInAccountsCount(base.InnerHandle);
		}

		public LoginStatus GetLoginStatus(EpicAccountId localUserId)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet(ref target, localUserId);
			return Bindings.EOS_Auth_GetLoginStatus(base.InnerHandle, target);
		}

		public void LinkAccount(LinkAccountOptions options, object clientData, OnLinkAccountCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LinkAccountOptionsInternal, LinkAccountOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLinkAccountCallbackInternal onLinkAccountCallbackInternal = OnLinkAccountCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onLinkAccountCallbackInternal);
			Bindings.EOS_Auth_LinkAccount(base.InnerHandle, target, clientDataAddress, onLinkAccountCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void Login(LoginOptions options, object clientData, OnLoginCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LoginOptionsInternal, LoginOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLoginCallbackInternal onLoginCallbackInternal = OnLoginCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onLoginCallbackInternal);
			Bindings.EOS_Auth_Login(base.InnerHandle, target, clientDataAddress, onLoginCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void Logout(LogoutOptions options, object clientData, OnLogoutCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LogoutOptionsInternal, LogoutOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLogoutCallbackInternal onLogoutCallbackInternal = OnLogoutCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onLogoutCallbackInternal);
			Bindings.EOS_Auth_Logout(base.InnerHandle, target, clientDataAddress, onLogoutCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RemoveNotifyLoginStatusChanged(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Auth_RemoveNotifyLoginStatusChanged(base.InnerHandle, inId);
		}

		public void VerifyUserAuth(VerifyUserAuthOptions options, object clientData, OnVerifyUserAuthCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<VerifyUserAuthOptionsInternal, VerifyUserAuthOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnVerifyUserAuthCallbackInternal onVerifyUserAuthCallbackInternal = OnVerifyUserAuthCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onVerifyUserAuthCallbackInternal);
			Bindings.EOS_Auth_VerifyUserAuth(base.InnerHandle, target, clientDataAddress, onVerifyUserAuthCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnDeletePersistentAuthCallbackInternal))]
		internal static void OnDeletePersistentAuthCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnDeletePersistentAuthCallback, DeletePersistentAuthCallbackInfoInternal, DeletePersistentAuthCallbackInfo>(data, out var callback, out var callbackInfo))
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

		[MonoPInvokeCallback(typeof(OnLogoutCallbackInternal))]
		internal static void OnLogoutCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnLogoutCallback, LogoutCallbackInfoInternal, LogoutCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnVerifyUserAuthCallbackInternal))]
		internal static void OnVerifyUserAuthCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnVerifyUserAuthCallback, VerifyUserAuthCallbackInfoInternal, VerifyUserAuthCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		public void Login(IOSLoginOptions options, object clientData, OnLoginCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<IOSLoginOptionsInternal, IOSLoginOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnLoginCallbackInternal onLoginCallbackInternal = OnLoginCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onLoginCallbackInternal);
			Bindings.EOS_Auth_Login(base.InnerHandle, target, clientDataAddress, onLoginCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}
	}
}
