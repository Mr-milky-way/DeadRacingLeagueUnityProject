using System;

namespace Epic.OnlineServices.Achievements
{
	public sealed class AchievementsInterface : Handle
	{
		public const int AchievementUnlocktimeUndefined = -1;

		public const int AddnotifyachievementsunlockedApiLatest = 1;

		public const int Addnotifyachievementsunlockedv2ApiLatest = 2;

		public const int Copyachievementdefinitionv2ByachievementidApiLatest = 2;

		public const int Copyachievementdefinitionv2ByindexApiLatest = 2;

		public const int CopydefinitionbyachievementidApiLatest = 1;

		public const int CopydefinitionbyindexApiLatest = 1;

		public const int Copydefinitionv2ByachievementidApiLatest = 2;

		public const int Copydefinitionv2ByindexApiLatest = 2;

		public const int CopyplayerachievementbyachievementidApiLatest = 2;

		public const int CopyplayerachievementbyindexApiLatest = 2;

		public const int CopyunlockedachievementbyachievementidApiLatest = 1;

		public const int CopyunlockedachievementbyindexApiLatest = 1;

		public const int DefinitionApiLatest = 1;

		public const int Definitionv2ApiLatest = 2;

		public const int GetachievementdefinitioncountApiLatest = 1;

		public const int GetplayerachievementcountApiLatest = 1;

		public const int GetunlockedachievementcountApiLatest = 1;

		public const int PlayerachievementApiLatest = 2;

		public const int PlayerstatinfoApiLatest = 1;

		public const int QuerydefinitionsApiLatest = 3;

		public const int QueryplayerachievementsApiLatest = 2;

		public const int StatthresholdApiLatest = 1;

		public const int StatthresholdsApiLatest = 1;

		public const int UnlockachievementsApiLatest = 1;

		public const int UnlockedachievementApiLatest = 1;

		public AchievementsInterface()
		{
		}

		public AchievementsInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public ulong AddNotifyAchievementsUnlocked(AddNotifyAchievementsUnlockedOptions options, object clientData, OnAchievementsUnlockedCallback notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyAchievementsUnlockedOptionsInternal, AddNotifyAchievementsUnlockedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnAchievementsUnlockedCallbackInternal onAchievementsUnlockedCallbackInternal = OnAchievementsUnlockedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onAchievementsUnlockedCallbackInternal);
			ulong num = Bindings.EOS_Achievements_AddNotifyAchievementsUnlocked(base.InnerHandle, target, clientDataAddress, onAchievementsUnlockedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyAchievementsUnlockedV2(AddNotifyAchievementsUnlockedV2Options options, object clientData, OnAchievementsUnlockedCallbackV2 notificationFn)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyAchievementsUnlockedV2OptionsInternal, AddNotifyAchievementsUnlockedV2Options>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnAchievementsUnlockedCallbackV2Internal onAchievementsUnlockedCallbackV2Internal = OnAchievementsUnlockedCallbackV2InternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, notificationFn, onAchievementsUnlockedCallbackV2Internal);
			ulong num = Bindings.EOS_Achievements_AddNotifyAchievementsUnlockedV2(base.InnerHandle, target, clientDataAddress, onAchievementsUnlockedCallbackV2Internal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public Result CopyAchievementDefinitionByAchievementId(CopyAchievementDefinitionByAchievementIdOptions options, out Definition outDefinition)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyAchievementDefinitionByAchievementIdOptionsInternal, CopyAchievementDefinitionByAchievementIdOptions>(ref target, options);
			IntPtr outDefinition2 = IntPtr.Zero;
			Result result = Bindings.EOS_Achievements_CopyAchievementDefinitionByAchievementId(base.InnerHandle, target, ref outDefinition2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<DefinitionInternal, Definition>(outDefinition2, out outDefinition))
			{
				Bindings.EOS_Achievements_Definition_Release(outDefinition2);
			}
			return result;
		}

		public Result CopyAchievementDefinitionByIndex(CopyAchievementDefinitionByIndexOptions options, out Definition outDefinition)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyAchievementDefinitionByIndexOptionsInternal, CopyAchievementDefinitionByIndexOptions>(ref target, options);
			IntPtr outDefinition2 = IntPtr.Zero;
			Result result = Bindings.EOS_Achievements_CopyAchievementDefinitionByIndex(base.InnerHandle, target, ref outDefinition2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<DefinitionInternal, Definition>(outDefinition2, out outDefinition))
			{
				Bindings.EOS_Achievements_Definition_Release(outDefinition2);
			}
			return result;
		}

		public Result CopyAchievementDefinitionV2ByAchievementId(CopyAchievementDefinitionV2ByAchievementIdOptions options, out DefinitionV2 outDefinition)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyAchievementDefinitionV2ByAchievementIdOptionsInternal, CopyAchievementDefinitionV2ByAchievementIdOptions>(ref target, options);
			IntPtr outDefinition2 = IntPtr.Zero;
			Result result = Bindings.EOS_Achievements_CopyAchievementDefinitionV2ByAchievementId(base.InnerHandle, target, ref outDefinition2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<DefinitionV2Internal, DefinitionV2>(outDefinition2, out outDefinition))
			{
				Bindings.EOS_Achievements_DefinitionV2_Release(outDefinition2);
			}
			return result;
		}

		public Result CopyAchievementDefinitionV2ByIndex(CopyAchievementDefinitionV2ByIndexOptions options, out DefinitionV2 outDefinition)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyAchievementDefinitionV2ByIndexOptionsInternal, CopyAchievementDefinitionV2ByIndexOptions>(ref target, options);
			IntPtr outDefinition2 = IntPtr.Zero;
			Result result = Bindings.EOS_Achievements_CopyAchievementDefinitionV2ByIndex(base.InnerHandle, target, ref outDefinition2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<DefinitionV2Internal, DefinitionV2>(outDefinition2, out outDefinition))
			{
				Bindings.EOS_Achievements_DefinitionV2_Release(outDefinition2);
			}
			return result;
		}

		public Result CopyPlayerAchievementByAchievementId(CopyPlayerAchievementByAchievementIdOptions options, out PlayerAchievement outAchievement)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyPlayerAchievementByAchievementIdOptionsInternal, CopyPlayerAchievementByAchievementIdOptions>(ref target, options);
			IntPtr outAchievement2 = IntPtr.Zero;
			Result result = Bindings.EOS_Achievements_CopyPlayerAchievementByAchievementId(base.InnerHandle, target, ref outAchievement2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<PlayerAchievementInternal, PlayerAchievement>(outAchievement2, out outAchievement))
			{
				Bindings.EOS_Achievements_PlayerAchievement_Release(outAchievement2);
			}
			return result;
		}

		public Result CopyPlayerAchievementByIndex(CopyPlayerAchievementByIndexOptions options, out PlayerAchievement outAchievement)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyPlayerAchievementByIndexOptionsInternal, CopyPlayerAchievementByIndexOptions>(ref target, options);
			IntPtr outAchievement2 = IntPtr.Zero;
			Result result = Bindings.EOS_Achievements_CopyPlayerAchievementByIndex(base.InnerHandle, target, ref outAchievement2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<PlayerAchievementInternal, PlayerAchievement>(outAchievement2, out outAchievement))
			{
				Bindings.EOS_Achievements_PlayerAchievement_Release(outAchievement2);
			}
			return result;
		}

		public Result CopyUnlockedAchievementByAchievementId(CopyUnlockedAchievementByAchievementIdOptions options, out UnlockedAchievement outAchievement)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyUnlockedAchievementByAchievementIdOptionsInternal, CopyUnlockedAchievementByAchievementIdOptions>(ref target, options);
			IntPtr outAchievement2 = IntPtr.Zero;
			Result result = Bindings.EOS_Achievements_CopyUnlockedAchievementByAchievementId(base.InnerHandle, target, ref outAchievement2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<UnlockedAchievementInternal, UnlockedAchievement>(outAchievement2, out outAchievement))
			{
				Bindings.EOS_Achievements_UnlockedAchievement_Release(outAchievement2);
			}
			return result;
		}

		public Result CopyUnlockedAchievementByIndex(CopyUnlockedAchievementByIndexOptions options, out UnlockedAchievement outAchievement)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyUnlockedAchievementByIndexOptionsInternal, CopyUnlockedAchievementByIndexOptions>(ref target, options);
			IntPtr outAchievement2 = IntPtr.Zero;
			Result result = Bindings.EOS_Achievements_CopyUnlockedAchievementByIndex(base.InnerHandle, target, ref outAchievement2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<UnlockedAchievementInternal, UnlockedAchievement>(outAchievement2, out outAchievement))
			{
				Bindings.EOS_Achievements_UnlockedAchievement_Release(outAchievement2);
			}
			return result;
		}

		public uint GetAchievementDefinitionCount(GetAchievementDefinitionCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetAchievementDefinitionCountOptionsInternal, GetAchievementDefinitionCountOptions>(ref target, options);
			uint result = Bindings.EOS_Achievements_GetAchievementDefinitionCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public uint GetPlayerAchievementCount(GetPlayerAchievementCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetPlayerAchievementCountOptionsInternal, GetPlayerAchievementCountOptions>(ref target, options);
			uint result = Bindings.EOS_Achievements_GetPlayerAchievementCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public uint GetUnlockedAchievementCount(GetUnlockedAchievementCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetUnlockedAchievementCountOptionsInternal, GetUnlockedAchievementCountOptions>(ref target, options);
			uint result = Bindings.EOS_Achievements_GetUnlockedAchievementCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void QueryDefinitions(QueryDefinitionsOptions options, object clientData, OnQueryDefinitionsCompleteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryDefinitionsOptionsInternal, QueryDefinitionsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryDefinitionsCompleteCallbackInternal onQueryDefinitionsCompleteCallbackInternal = OnQueryDefinitionsCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryDefinitionsCompleteCallbackInternal);
			Bindings.EOS_Achievements_QueryDefinitions(base.InnerHandle, target, clientDataAddress, onQueryDefinitionsCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryPlayerAchievements(QueryPlayerAchievementsOptions options, object clientData, OnQueryPlayerAchievementsCompleteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryPlayerAchievementsOptionsInternal, QueryPlayerAchievementsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryPlayerAchievementsCompleteCallbackInternal onQueryPlayerAchievementsCompleteCallbackInternal = OnQueryPlayerAchievementsCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryPlayerAchievementsCompleteCallbackInternal);
			Bindings.EOS_Achievements_QueryPlayerAchievements(base.InnerHandle, target, clientDataAddress, onQueryPlayerAchievementsCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void RemoveNotifyAchievementsUnlocked(ulong inId)
		{
			Helper.TryRemoveCallbackByNotificationId(inId);
			Bindings.EOS_Achievements_RemoveNotifyAchievementsUnlocked(base.InnerHandle, inId);
		}

		public void UnlockAchievements(UnlockAchievementsOptions options, object clientData, OnUnlockAchievementsCompleteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UnlockAchievementsOptionsInternal, UnlockAchievementsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnUnlockAchievementsCompleteCallbackInternal onUnlockAchievementsCompleteCallbackInternal = OnUnlockAchievementsCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onUnlockAchievementsCompleteCallbackInternal);
			Bindings.EOS_Achievements_UnlockAchievements(base.InnerHandle, target, clientDataAddress, onUnlockAchievementsCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnAchievementsUnlockedCallbackInternal))]
		internal static void OnAchievementsUnlockedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnAchievementsUnlockedCallback, OnAchievementsUnlockedCallbackInfoInternal, OnAchievementsUnlockedCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnAchievementsUnlockedCallbackV2Internal))]
		internal static void OnAchievementsUnlockedCallbackV2InternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnAchievementsUnlockedCallbackV2, OnAchievementsUnlockedCallbackV2InfoInternal, OnAchievementsUnlockedCallbackV2Info>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryDefinitionsCompleteCallbackInternal))]
		internal static void OnQueryDefinitionsCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryDefinitionsCompleteCallback, OnQueryDefinitionsCompleteCallbackInfoInternal, OnQueryDefinitionsCompleteCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryPlayerAchievementsCompleteCallbackInternal))]
		internal static void OnQueryPlayerAchievementsCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryPlayerAchievementsCompleteCallback, OnQueryPlayerAchievementsCompleteCallbackInfoInternal, OnQueryPlayerAchievementsCompleteCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnUnlockAchievementsCompleteCallbackInternal))]
		internal static void OnUnlockAchievementsCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnUnlockAchievementsCompleteCallback, OnUnlockAchievementsCompleteCallbackInfoInternal, OnUnlockAchievementsCompleteCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
