using System;

namespace Epic.OnlineServices.Leaderboards
{
	public sealed class LeaderboardsInterface : Handle
	{
		public const int CopyleaderboarddefinitionbyindexApiLatest = 1;

		public const int CopyleaderboarddefinitionbyleaderboardidApiLatest = 1;

		public const int CopyleaderboardrecordbyindexApiLatest = 2;

		public const int CopyleaderboardrecordbyuseridApiLatest = 2;

		public const int CopyleaderboarduserscorebyindexApiLatest = 1;

		public const int CopyleaderboarduserscorebyuseridApiLatest = 1;

		public const int DefinitionApiLatest = 1;

		public const int GetleaderboarddefinitioncountApiLatest = 1;

		public const int GetleaderboardrecordcountApiLatest = 1;

		public const int GetleaderboarduserscorecountApiLatest = 1;

		public const int LeaderboardrecordApiLatest = 2;

		public const int LeaderboarduserscoreApiLatest = 1;

		public const int QueryleaderboarddefinitionsApiLatest = 2;

		public const int QueryleaderboardranksApiLatest = 2;

		public const int QueryleaderboarduserscoresApiLatest = 2;

		public const int TimeUndefined = -1;

		public const int UserscoresquerystatinfoApiLatest = 1;

		public LeaderboardsInterface()
		{
		}

		public LeaderboardsInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopyLeaderboardDefinitionByIndex(CopyLeaderboardDefinitionByIndexOptions options, out Definition outLeaderboardDefinition)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyLeaderboardDefinitionByIndexOptionsInternal, CopyLeaderboardDefinitionByIndexOptions>(ref target, options);
			IntPtr outLeaderboardDefinition2 = IntPtr.Zero;
			Result result = Bindings.EOS_Leaderboards_CopyLeaderboardDefinitionByIndex(base.InnerHandle, target, ref outLeaderboardDefinition2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<DefinitionInternal, Definition>(outLeaderboardDefinition2, out outLeaderboardDefinition))
			{
				Bindings.EOS_Leaderboards_Definition_Release(outLeaderboardDefinition2);
			}
			return result;
		}

		public Result CopyLeaderboardDefinitionByLeaderboardId(CopyLeaderboardDefinitionByLeaderboardIdOptions options, out Definition outLeaderboardDefinition)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyLeaderboardDefinitionByLeaderboardIdOptionsInternal, CopyLeaderboardDefinitionByLeaderboardIdOptions>(ref target, options);
			IntPtr outLeaderboardDefinition2 = IntPtr.Zero;
			Result result = Bindings.EOS_Leaderboards_CopyLeaderboardDefinitionByLeaderboardId(base.InnerHandle, target, ref outLeaderboardDefinition2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<DefinitionInternal, Definition>(outLeaderboardDefinition2, out outLeaderboardDefinition))
			{
				Bindings.EOS_Leaderboards_Definition_Release(outLeaderboardDefinition2);
			}
			return result;
		}

		public Result CopyLeaderboardRecordByIndex(CopyLeaderboardRecordByIndexOptions options, out LeaderboardRecord outLeaderboardRecord)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyLeaderboardRecordByIndexOptionsInternal, CopyLeaderboardRecordByIndexOptions>(ref target, options);
			IntPtr outLeaderboardRecord2 = IntPtr.Zero;
			Result result = Bindings.EOS_Leaderboards_CopyLeaderboardRecordByIndex(base.InnerHandle, target, ref outLeaderboardRecord2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<LeaderboardRecordInternal, LeaderboardRecord>(outLeaderboardRecord2, out outLeaderboardRecord))
			{
				Bindings.EOS_Leaderboards_LeaderboardRecord_Release(outLeaderboardRecord2);
			}
			return result;
		}

		public Result CopyLeaderboardRecordByUserId(CopyLeaderboardRecordByUserIdOptions options, out LeaderboardRecord outLeaderboardRecord)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyLeaderboardRecordByUserIdOptionsInternal, CopyLeaderboardRecordByUserIdOptions>(ref target, options);
			IntPtr outLeaderboardRecord2 = IntPtr.Zero;
			Result result = Bindings.EOS_Leaderboards_CopyLeaderboardRecordByUserId(base.InnerHandle, target, ref outLeaderboardRecord2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<LeaderboardRecordInternal, LeaderboardRecord>(outLeaderboardRecord2, out outLeaderboardRecord))
			{
				Bindings.EOS_Leaderboards_LeaderboardRecord_Release(outLeaderboardRecord2);
			}
			return result;
		}

		public Result CopyLeaderboardUserScoreByIndex(CopyLeaderboardUserScoreByIndexOptions options, out LeaderboardUserScore outLeaderboardUserScore)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyLeaderboardUserScoreByIndexOptionsInternal, CopyLeaderboardUserScoreByIndexOptions>(ref target, options);
			IntPtr outLeaderboardUserScore2 = IntPtr.Zero;
			Result result = Bindings.EOS_Leaderboards_CopyLeaderboardUserScoreByIndex(base.InnerHandle, target, ref outLeaderboardUserScore2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<LeaderboardUserScoreInternal, LeaderboardUserScore>(outLeaderboardUserScore2, out outLeaderboardUserScore))
			{
				Bindings.EOS_Leaderboards_LeaderboardUserScore_Release(outLeaderboardUserScore2);
			}
			return result;
		}

		public Result CopyLeaderboardUserScoreByUserId(CopyLeaderboardUserScoreByUserIdOptions options, out LeaderboardUserScore outLeaderboardUserScore)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyLeaderboardUserScoreByUserIdOptionsInternal, CopyLeaderboardUserScoreByUserIdOptions>(ref target, options);
			IntPtr outLeaderboardUserScore2 = IntPtr.Zero;
			Result result = Bindings.EOS_Leaderboards_CopyLeaderboardUserScoreByUserId(base.InnerHandle, target, ref outLeaderboardUserScore2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<LeaderboardUserScoreInternal, LeaderboardUserScore>(outLeaderboardUserScore2, out outLeaderboardUserScore))
			{
				Bindings.EOS_Leaderboards_LeaderboardUserScore_Release(outLeaderboardUserScore2);
			}
			return result;
		}

		public uint GetLeaderboardDefinitionCount(GetLeaderboardDefinitionCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetLeaderboardDefinitionCountOptionsInternal, GetLeaderboardDefinitionCountOptions>(ref target, options);
			uint result = Bindings.EOS_Leaderboards_GetLeaderboardDefinitionCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public uint GetLeaderboardRecordCount(GetLeaderboardRecordCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetLeaderboardRecordCountOptionsInternal, GetLeaderboardRecordCountOptions>(ref target, options);
			uint result = Bindings.EOS_Leaderboards_GetLeaderboardRecordCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public uint GetLeaderboardUserScoreCount(GetLeaderboardUserScoreCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetLeaderboardUserScoreCountOptionsInternal, GetLeaderboardUserScoreCountOptions>(ref target, options);
			uint result = Bindings.EOS_Leaderboards_GetLeaderboardUserScoreCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void QueryLeaderboardDefinitions(QueryLeaderboardDefinitionsOptions options, object clientData, OnQueryLeaderboardDefinitionsCompleteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryLeaderboardDefinitionsOptionsInternal, QueryLeaderboardDefinitionsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryLeaderboardDefinitionsCompleteCallbackInternal onQueryLeaderboardDefinitionsCompleteCallbackInternal = OnQueryLeaderboardDefinitionsCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryLeaderboardDefinitionsCompleteCallbackInternal);
			Bindings.EOS_Leaderboards_QueryLeaderboardDefinitions(base.InnerHandle, target, clientDataAddress, onQueryLeaderboardDefinitionsCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryLeaderboardRanks(QueryLeaderboardRanksOptions options, object clientData, OnQueryLeaderboardRanksCompleteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryLeaderboardRanksOptionsInternal, QueryLeaderboardRanksOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryLeaderboardRanksCompleteCallbackInternal onQueryLeaderboardRanksCompleteCallbackInternal = OnQueryLeaderboardRanksCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryLeaderboardRanksCompleteCallbackInternal);
			Bindings.EOS_Leaderboards_QueryLeaderboardRanks(base.InnerHandle, target, clientDataAddress, onQueryLeaderboardRanksCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryLeaderboardUserScores(QueryLeaderboardUserScoresOptions options, object clientData, OnQueryLeaderboardUserScoresCompleteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryLeaderboardUserScoresOptionsInternal, QueryLeaderboardUserScoresOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryLeaderboardUserScoresCompleteCallbackInternal onQueryLeaderboardUserScoresCompleteCallbackInternal = OnQueryLeaderboardUserScoresCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryLeaderboardUserScoresCompleteCallbackInternal);
			Bindings.EOS_Leaderboards_QueryLeaderboardUserScores(base.InnerHandle, target, clientDataAddress, onQueryLeaderboardUserScoresCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnQueryLeaderboardDefinitionsCompleteCallbackInternal))]
		internal static void OnQueryLeaderboardDefinitionsCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryLeaderboardDefinitionsCompleteCallback, OnQueryLeaderboardDefinitionsCompleteCallbackInfoInternal, OnQueryLeaderboardDefinitionsCompleteCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryLeaderboardRanksCompleteCallbackInternal))]
		internal static void OnQueryLeaderboardRanksCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryLeaderboardRanksCompleteCallback, OnQueryLeaderboardRanksCompleteCallbackInfoInternal, OnQueryLeaderboardRanksCompleteCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryLeaderboardUserScoresCompleteCallbackInternal))]
		internal static void OnQueryLeaderboardUserScoresCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryLeaderboardUserScoresCompleteCallback, OnQueryLeaderboardUserScoresCompleteCallbackInfoInternal, OnQueryLeaderboardUserScoresCompleteCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
