using System;

namespace Epic.OnlineServices.Lobby
{
	public sealed class LobbySearch : Handle
	{
		public const int LobbysearchCopysearchresultbyindexApiLatest = 1;

		public const int LobbysearchFindApiLatest = 1;

		public const int LobbysearchGetsearchresultcountApiLatest = 1;

		public const int LobbysearchRemoveparameterApiLatest = 1;

		public const int LobbysearchSetlobbyidApiLatest = 1;

		public const int LobbysearchSetmaxresultsApiLatest = 1;

		public const int LobbysearchSetparameterApiLatest = 1;

		public const int LobbysearchSettargetuseridApiLatest = 1;

		public LobbySearch()
		{
		}

		public LobbySearch(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopySearchResultByIndex(LobbySearchCopySearchResultByIndexOptions options, out LobbyDetails outLobbyDetailsHandle)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbySearchCopySearchResultByIndexOptionsInternal, LobbySearchCopySearchResultByIndexOptions>(ref target, options);
			IntPtr outLobbyDetailsHandle2 = IntPtr.Zero;
			Result result = Bindings.EOS_LobbySearch_CopySearchResultByIndex(base.InnerHandle, target, ref outLobbyDetailsHandle2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outLobbyDetailsHandle2, out outLobbyDetailsHandle);
			return result;
		}

		public void Find(LobbySearchFindOptions options, object clientData, LobbySearchOnFindCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbySearchFindOptionsInternal, LobbySearchFindOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			LobbySearchOnFindCallbackInternal lobbySearchOnFindCallbackInternal = OnFindCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, lobbySearchOnFindCallbackInternal);
			Bindings.EOS_LobbySearch_Find(base.InnerHandle, target, clientDataAddress, lobbySearchOnFindCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public uint GetSearchResultCount(LobbySearchGetSearchResultCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbySearchGetSearchResultCountOptionsInternal, LobbySearchGetSearchResultCountOptions>(ref target, options);
			uint result = Bindings.EOS_LobbySearch_GetSearchResultCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void Release()
		{
			Bindings.EOS_LobbySearch_Release(base.InnerHandle);
		}

		public Result RemoveParameter(LobbySearchRemoveParameterOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbySearchRemoveParameterOptionsInternal, LobbySearchRemoveParameterOptions>(ref target, options);
			Result result = Bindings.EOS_LobbySearch_RemoveParameter(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetLobbyId(LobbySearchSetLobbyIdOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbySearchSetLobbyIdOptionsInternal, LobbySearchSetLobbyIdOptions>(ref target, options);
			Result result = Bindings.EOS_LobbySearch_SetLobbyId(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetMaxResults(LobbySearchSetMaxResultsOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbySearchSetMaxResultsOptionsInternal, LobbySearchSetMaxResultsOptions>(ref target, options);
			Result result = Bindings.EOS_LobbySearch_SetMaxResults(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetParameter(LobbySearchSetParameterOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbySearchSetParameterOptionsInternal, LobbySearchSetParameterOptions>(ref target, options);
			Result result = Bindings.EOS_LobbySearch_SetParameter(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetTargetUserId(LobbySearchSetTargetUserIdOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbySearchSetTargetUserIdOptionsInternal, LobbySearchSetTargetUserIdOptions>(ref target, options);
			Result result = Bindings.EOS_LobbySearch_SetTargetUserId(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		[MonoPInvokeCallback(typeof(LobbySearchOnFindCallbackInternal))]
		internal static void OnFindCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<LobbySearchOnFindCallback, LobbySearchFindCallbackInfoInternal, LobbySearchFindCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
