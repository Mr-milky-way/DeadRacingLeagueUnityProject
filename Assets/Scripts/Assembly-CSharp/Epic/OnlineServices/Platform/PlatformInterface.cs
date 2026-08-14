using System;
using Epic.OnlineServices.Achievements;
using Epic.OnlineServices.AntiCheatClient;
using Epic.OnlineServices.AntiCheatServer;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Ecom;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.KWS;
using Epic.OnlineServices.Leaderboards;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Metrics;
using Epic.OnlineServices.Mods;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.PlayerDataStorage;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.Reports;
using Epic.OnlineServices.Sanctions;
using Epic.OnlineServices.Sessions;
using Epic.OnlineServices.Stats;
using Epic.OnlineServices.TitleStorage;
using Epic.OnlineServices.UI;
using Epic.OnlineServices.UserInfo;

namespace Epic.OnlineServices.Platform
{
	public sealed class PlatformInterface : Handle
	{
		public const int AndroidinitializeoptionssysteminitializeoptionsApiLatest = 2;

		public const int CountrycodeMaxBufferLen = 5;

		public const int CountrycodeMaxLength = 4;

		public const int InitializeApiLatest = 4;

		public const int InitializeThreadaffinityApiLatest = 1;

		public const int LocalecodeMaxBufferLen = 10;

		public const int LocalecodeMaxLength = 9;

		public const int OptionsApiLatest = 11;

		public static Result Initialize(AndroidInitializeOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AndroidInitializeOptionsInternal, AndroidInitializeOptions>(ref target, options);
			Result result = Bindings.EOS_Initialize(target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public PlatformInterface()
		{
		}

		public PlatformInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CheckForLauncherAndRestart()
		{
			return Bindings.EOS_Platform_CheckForLauncherAndRestart(base.InnerHandle);
		}

		public static PlatformInterface Create(Options options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<OptionsInternal, Options>(ref target, options);
			IntPtr source = Bindings.EOS_Platform_Create(target);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out PlatformInterface target2);
			return target2;
		}

		public AchievementsInterface GetAchievementsInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetAchievementsInterface(base.InnerHandle), out AchievementsInterface target);
			return target;
		}

		public Result GetActiveCountryCode(EpicAccountId localUserId, out string outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet(ref target, localUserId);
			IntPtr target2 = IntPtr.Zero;
			int inOutBufferLength = 5;
			Helper.TryMarshalAllocate(ref target2, inOutBufferLength, out var _);
			Result result = Bindings.EOS_Platform_GetActiveCountryCode(base.InnerHandle, target, target2, ref inOutBufferLength);
			Helper.TryMarshalGet(target2, out outBuffer);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public Result GetActiveLocaleCode(EpicAccountId localUserId, out string outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet(ref target, localUserId);
			IntPtr target2 = IntPtr.Zero;
			int inOutBufferLength = 10;
			Helper.TryMarshalAllocate(ref target2, inOutBufferLength, out var _);
			Result result = Bindings.EOS_Platform_GetActiveLocaleCode(base.InnerHandle, target, target2, ref inOutBufferLength);
			Helper.TryMarshalGet(target2, out outBuffer);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public AntiCheatClientInterface GetAntiCheatClientInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetAntiCheatClientInterface(base.InnerHandle), out AntiCheatClientInterface target);
			return target;
		}

		public AntiCheatServerInterface GetAntiCheatServerInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetAntiCheatServerInterface(base.InnerHandle), out AntiCheatServerInterface target);
			return target;
		}

		public AuthInterface GetAuthInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetAuthInterface(base.InnerHandle), out AuthInterface target);
			return target;
		}

		public ConnectInterface GetConnectInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetConnectInterface(base.InnerHandle), out ConnectInterface target);
			return target;
		}

		public EcomInterface GetEcomInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetEcomInterface(base.InnerHandle), out EcomInterface target);
			return target;
		}

		public FriendsInterface GetFriendsInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetFriendsInterface(base.InnerHandle), out FriendsInterface target);
			return target;
		}

		public KWSInterface GetKWSInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetKWSInterface(base.InnerHandle), out KWSInterface target);
			return target;
		}

		public LeaderboardsInterface GetLeaderboardsInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetLeaderboardsInterface(base.InnerHandle), out LeaderboardsInterface target);
			return target;
		}

		public LobbyInterface GetLobbyInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetLobbyInterface(base.InnerHandle), out LobbyInterface target);
			return target;
		}

		public MetricsInterface GetMetricsInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetMetricsInterface(base.InnerHandle), out MetricsInterface target);
			return target;
		}

		public ModsInterface GetModsInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetModsInterface(base.InnerHandle), out ModsInterface target);
			return target;
		}

		public Result GetOverrideCountryCode(out string outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			int inOutBufferLength = 5;
			Helper.TryMarshalAllocate(ref target, inOutBufferLength, out var _);
			Result result = Bindings.EOS_Platform_GetOverrideCountryCode(base.InnerHandle, target, ref inOutBufferLength);
			Helper.TryMarshalGet(target, out outBuffer);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result GetOverrideLocaleCode(out string outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			int inOutBufferLength = 10;
			Helper.TryMarshalAllocate(ref target, inOutBufferLength, out var _);
			Result result = Bindings.EOS_Platform_GetOverrideLocaleCode(base.InnerHandle, target, ref inOutBufferLength);
			Helper.TryMarshalGet(target, out outBuffer);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public P2PInterface GetP2PInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetP2PInterface(base.InnerHandle), out P2PInterface target);
			return target;
		}

		public PlayerDataStorageInterface GetPlayerDataStorageInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetPlayerDataStorageInterface(base.InnerHandle), out PlayerDataStorageInterface target);
			return target;
		}

		public PresenceInterface GetPresenceInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetPresenceInterface(base.InnerHandle), out PresenceInterface target);
			return target;
		}

		public ReportsInterface GetReportsInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetReportsInterface(base.InnerHandle), out ReportsInterface target);
			return target;
		}

		public SanctionsInterface GetSanctionsInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetSanctionsInterface(base.InnerHandle), out SanctionsInterface target);
			return target;
		}

		public SessionsInterface GetSessionsInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetSessionsInterface(base.InnerHandle), out SessionsInterface target);
			return target;
		}

		public StatsInterface GetStatsInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetStatsInterface(base.InnerHandle), out StatsInterface target);
			return target;
		}

		public TitleStorageInterface GetTitleStorageInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetTitleStorageInterface(base.InnerHandle), out TitleStorageInterface target);
			return target;
		}

		public UIInterface GetUIInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetUIInterface(base.InnerHandle), out UIInterface target);
			return target;
		}

		public UserInfoInterface GetUserInfoInterface()
		{
			Helper.TryMarshalGet(Bindings.EOS_Platform_GetUserInfoInterface(base.InnerHandle), out UserInfoInterface target);
			return target;
		}

		public static Result Initialize(InitializeOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<InitializeOptionsInternal, InitializeOptions>(ref target, options);
			Result result = Bindings.EOS_Initialize(target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void Release()
		{
			Bindings.EOS_Platform_Release(base.InnerHandle);
		}

		public Result SetOverrideCountryCode(string newCountryCode)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet(ref target, newCountryCode);
			Result result = Bindings.EOS_Platform_SetOverrideCountryCode(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetOverrideLocaleCode(string newLocaleCode)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet(ref target, newLocaleCode);
			Result result = Bindings.EOS_Platform_SetOverrideLocaleCode(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public static Result Shutdown()
		{
			return Bindings.EOS_Shutdown();
		}

		public void Tick()
		{
			Bindings.EOS_Platform_Tick(base.InnerHandle);
		}
	}
}
