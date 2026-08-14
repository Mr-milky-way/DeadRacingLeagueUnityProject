using System;

namespace Epic.OnlineServices.Sessions
{
	public sealed class SessionSearch : Handle
	{
		public const int SessionsearchCopysearchresultbyindexApiLatest = 1;

		public const int SessionsearchFindApiLatest = 2;

		public const int SessionsearchGetsearchresultcountApiLatest = 1;

		public const int SessionsearchRemoveparameterApiLatest = 1;

		public const int SessionsearchSetmaxsearchresultsApiLatest = 1;

		public const int SessionsearchSetparameterApiLatest = 1;

		public const int SessionsearchSetsessionidApiLatest = 1;

		public const int SessionsearchSettargetuseridApiLatest = 1;

		public SessionSearch()
		{
		}

		public SessionSearch(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopySearchResultByIndex(SessionSearchCopySearchResultByIndexOptions options, out SessionDetails outSessionHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionSearchCopySearchResultByIndexOptionsInternal, SessionSearchCopySearchResultByIndexOptions>(ref target, options);
			IntPtr outSessionHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_SessionSearch_CopySearchResultByIndex(base.InnerHandle, target, ref outSessionHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outSessionHandle2, out outSessionHandle);
			return result;
		}

		public void Find(SessionSearchFindOptions options, object clientData, SessionSearchOnFindCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionSearchFindOptionsInternal, SessionSearchFindOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			SessionSearchOnFindCallbackInternal sessionSearchOnFindCallbackInternal = OnFindCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, sessionSearchOnFindCallbackInternal);
			Bindings.EOS_SessionSearch_Find(base.InnerHandle, target, clientDataAddress, sessionSearchOnFindCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public uint GetSearchResultCount(SessionSearchGetSearchResultCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionSearchGetSearchResultCountOptionsInternal, SessionSearchGetSearchResultCountOptions>(ref target, options);
			uint result = Bindings.EOS_SessionSearch_GetSearchResultCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void Release()
		{
			Bindings.EOS_SessionSearch_Release(base.InnerHandle);
		}

		public Result RemoveParameter(SessionSearchRemoveParameterOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionSearchRemoveParameterOptionsInternal, SessionSearchRemoveParameterOptions>(ref target, options);
			Result result = Bindings.EOS_SessionSearch_RemoveParameter(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetMaxResults(SessionSearchSetMaxResultsOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionSearchSetMaxResultsOptionsInternal, SessionSearchSetMaxResultsOptions>(ref target, options);
			Result result = Bindings.EOS_SessionSearch_SetMaxResults(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetParameter(SessionSearchSetParameterOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionSearchSetParameterOptionsInternal, SessionSearchSetParameterOptions>(ref target, options);
			Result result = Bindings.EOS_SessionSearch_SetParameter(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetSessionId(SessionSearchSetSessionIdOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionSearchSetSessionIdOptionsInternal, SessionSearchSetSessionIdOptions>(ref target, options);
			Result result = Bindings.EOS_SessionSearch_SetSessionId(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetTargetUserId(SessionSearchSetTargetUserIdOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionSearchSetTargetUserIdOptionsInternal, SessionSearchSetTargetUserIdOptions>(ref target, options);
			Result result = Bindings.EOS_SessionSearch_SetTargetUserId(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		[MonoPInvokeCallback(typeof(SessionSearchOnFindCallbackInternal))]
		internal static void OnFindCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<SessionSearchOnFindCallback, SessionSearchFindCallbackInfoInternal, SessionSearchFindCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
