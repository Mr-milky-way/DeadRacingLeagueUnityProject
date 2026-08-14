using System;

namespace Epic.OnlineServices.Stats
{
	public sealed class StatsInterface : Handle
	{
		public const int CopystatbyindexApiLatest = 1;

		public const int CopystatbynameApiLatest = 1;

		public const int GetstatcountApiLatest = 1;

		public const int GetstatscountApiLatest = 1;

		public const int IngestdataApiLatest = 1;

		public const int IngeststatApiLatest = 3;

		public const int MaxIngestStats = 3000;

		public const int MaxQueryStats = 1000;

		public const int QuerystatsApiLatest = 3;

		public const int StatApiLatest = 1;

		public const int TimeUndefined = -1;

		public StatsInterface()
		{
		}

		public StatsInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopyStatByIndex(CopyStatByIndexOptions options, out Stat outStat)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyStatByIndexOptionsInternal, CopyStatByIndexOptions>(ref target, options);
			IntPtr outStat2 = IntPtr.Zero;
			Result result = Bindings.EOS_Stats_CopyStatByIndex(base.InnerHandle, target, ref outStat2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<StatInternal, Stat>(outStat2, out outStat))
			{
				Bindings.EOS_Stats_Stat_Release(outStat2);
			}
			return result;
		}

		public Result CopyStatByName(CopyStatByNameOptions options, out Stat outStat)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyStatByNameOptionsInternal, CopyStatByNameOptions>(ref target, options);
			IntPtr outStat2 = IntPtr.Zero;
			Result result = Bindings.EOS_Stats_CopyStatByName(base.InnerHandle, target, ref outStat2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<StatInternal, Stat>(outStat2, out outStat))
			{
				Bindings.EOS_Stats_Stat_Release(outStat2);
			}
			return result;
		}

		public uint GetStatsCount(GetStatCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetStatCountOptionsInternal, GetStatCountOptions>(ref target, options);
			uint result = Bindings.EOS_Stats_GetStatsCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void IngestStat(IngestStatOptions options, object clientData, OnIngestStatCompleteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<IngestStatOptionsInternal, IngestStatOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnIngestStatCompleteCallbackInternal onIngestStatCompleteCallbackInternal = OnIngestStatCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onIngestStatCompleteCallbackInternal);
			Bindings.EOS_Stats_IngestStat(base.InnerHandle, target, clientDataAddress, onIngestStatCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryStats(QueryStatsOptions options, object clientData, OnQueryStatsCompleteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryStatsOptionsInternal, QueryStatsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryStatsCompleteCallbackInternal onQueryStatsCompleteCallbackInternal = OnQueryStatsCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryStatsCompleteCallbackInternal);
			Bindings.EOS_Stats_QueryStats(base.InnerHandle, target, clientDataAddress, onQueryStatsCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnIngestStatCompleteCallbackInternal))]
		internal static void OnIngestStatCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnIngestStatCompleteCallback, IngestStatCompleteCallbackInfoInternal, IngestStatCompleteCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryStatsCompleteCallbackInternal))]
		internal static void OnQueryStatsCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryStatsCompleteCallback, OnQueryStatsCompleteCallbackInfoInternal, OnQueryStatsCompleteCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
